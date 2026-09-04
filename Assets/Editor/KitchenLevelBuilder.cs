using MouseGame.Game;
using MouseGame.Interaction;
using MouseGame.Player;
using MouseGame.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MouseGame.EditorTools
{
    /// <summary>
    /// One-click "First Complete Level" wiring (README): a real traversal path — cabinet (start)
    /// -> box -> chair -> table -> countertop, each hop a genuine jump (height gaps exceed
    /// CharacterController.stepOffset, within PlayerMotor.jumpHeight reach) rather than an
    /// auto-climbed step — the cheese moved onto the countertop, the cat repositioned to guard
    /// the path, and an EscapeZone back at the start so reaching the cheese alone doesn't end
    /// the level; you have to make it back. Run "Build MVP Scene", "Build Test Room", and
    /// "Build Cat AI" first. Safe to re-run.
    /// </summary>
    public static class KitchenLevelBuilder
    {
        // Straight line along +Z (constant X) rather than a diagonal — keeps each hop a plain
        // forward jump, no strafing needed. Edge-to-edge gap between consecutive pieces is a
        // deliberately tight ~0.08m: with PlayerMotor's jumpHeight (0.22) and gravity (-9), the
        // jump's time-to-apex is ~0.22s, giving only ~0.11m of horizontal travel at walkSpeed
        // (0.5) — the original diagonal layout's ~0.2m edge gaps were right at the edge of
        // physically reachable and prone to feeling unfair. Each entry: name, top height
        // (world Y), horizontal position (X, Z), footprint (X, Z).
        private const float PathX = -3.3f;
        private static readonly (string name, float topHeight, float x, float z, float sizeX, float sizeZ)[] Path =
        {
            ("KitchenBox", 0.12f, PathX, 2.20f, 0.20f, 0.20f),
            ("KitchenChair", 0.28f, PathX, 2.49f, 0.22f, 0.22f),
            ("KitchenTable", 0.45f, PathX, 2.83f, 0.30f, 0.30f),
            ("KitchenCountertop", 0.62f, PathX, 3.235f, 0.35f, 0.35f),
        };

        /// <summary>
        /// Where a guarding cat should stand — open floor beside the path, not the same X as it.
        /// The furniture blocks are solid from the floor up to their top (BuildBlock spans
        /// y: 0..topHeight, not a thin floating platform), so a guard position at PathX would
        /// land inside/under a piece of furniture (found this placing the cat directly inside
        /// KitchenTable's footprint). Offset clears the widest footprint (Countertop, half-size
        /// 0.175) plus the cat's own radius (0.15) with margin. Exposed so CatAIBuilder can use
        /// the same spot instead of hardcoding a duplicate.
        /// </summary>
        internal static readonly Vector3 GuardPosition = new Vector3(PathX + 0.5f, 0f, 2.75f);

        private const float CabinetX = -3.5f;
        private const float CabinetZ = 2f;

        [MenuItem("Mouse Game/Build Kitchen Level")]
        private static void BuildKitchenLevel()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Exit Play mode before building the kitchen level.");
                return;
            }

            GameObject player = GameObject.Find("Player");
            GameObject gameManager = GameObject.Find("GameManager");
            GameObject cheese = GameObject.Find("Cheese");
            if (player == null || gameManager == null || cheese == null)
            {
                Debug.LogError("Missing Player/GameManager/Cheese. Run 'Mouse Game -> Build MVP " +
                                "Scene' first.");
                return;
            }

            Undo.SetCurrentGroupName("Build Kitchen Level");
            int undoGroup = Undo.GetCurrentGroup();

            BuildFurniturePath();
            RepositionCheese(cheese);
            LevelCompleteManager levelCompleteManager = BuildWinState(player, gameManager);
            BuildEscapeZone(gameManager, levelCompleteManager);
            RepositionCat();

            Undo.CollapseUndoOperations(undoGroup);
            MvpSceneBuilder.SaveActiveScene();

            Debug.Log("Kitchen level built and saved: cabinet -> box -> chair -> table -> " +
                      $"countertop, a straight line at x={PathX}, z=2.1..3.41. Cheese moved onto " +
                      "the countertop, EscapeZone near the cabinet, Cat repositioned to guard the " +
                      "path. Get the cheese, then return to the escape zone to win.");
        }

        private static void BuildFurniturePath()
        {
            BuildBlock("Cabinet", new Vector3(CabinetX, 0.3f, CabinetZ), new Vector3(0.3f, 0.6f, 0.3f));

            foreach ((string name, float topHeight, float x, float z, float sizeX, float sizeZ) in Path)
            {
                BuildBlock(name, new Vector3(x, topHeight * 0.5f, z), new Vector3(sizeX, topHeight, sizeZ));
            }
        }

        private static void RepositionCheese(GameObject cheese)
        {
            (string name, float topHeight, float x, float z, float sizeX, float sizeZ) countertop = Path[Path.Length - 1];
            cheese.transform.position = new Vector3(countertop.x, countertop.topHeight + 0.05f, countertop.z);
        }

        private static LevelCompleteManager BuildWinState(GameObject player, GameObject gameManager)
        {
            LevelCompleteUI winUI = GetOrAddComponent<LevelCompleteUI>(gameManager);
            LevelCompleteManager levelCompleteManager = GetOrAddComponent<LevelCompleteManager>(gameManager);

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found. Run 'Mouse Game -> Build MVP Scene' first.");
                return levelCompleteManager;
            }

            GameObject panelGO = FindChild(canvas.transform, "LevelCompletePanel");
            Text text;
            if (panelGO == null)
            {
                panelGO = new GameObject("LevelCompletePanel", typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(panelGO, "Build Kitchen Level");
                panelGO.transform.SetParent(canvas.transform, false);

                RectTransform panelRect = panelGO.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                panelRect.anchoredPosition = Vector2.zero;
                panelRect.sizeDelta = new Vector2(600f, 220f);

                GameObject textGO = new GameObject("LevelCompleteText", typeof(Text));
                Undo.RegisterCreatedObjectUndo(textGO, "Build Kitchen Level");
                textGO.transform.SetParent(panelGO.transform, false);

                text = textGO.GetComponent<Text>();
                text.text = "You escaped! Level Complete!";
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 44;
                text.color = Color.green;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                RectTransform textRect = textGO.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0.5f, 1f);
                textRect.anchorMax = new Vector2(0.5f, 1f);
                textRect.pivot = new Vector2(0.5f, 1f);
                textRect.anchoredPosition = Vector2.zero;
                textRect.sizeDelta = new Vector2(600f, 120f);

                CatAIBuilder.BuildRestartButton(panelGO.transform, new Vector2(0f, -140f));
            }
            else
            {
                text = panelGO.transform.Find("LevelCompleteText").GetComponent<Text>();
            }

            SetSerializedField(winUI, "messagePanel", panelGO);
            SetSerializedField(winUI, "messageText", text);

            PlayerMotor motor = player.GetComponent<PlayerMotor>();
            PlayerLook look = player.GetComponentInChildren<PlayerLook>();
            SetSerializedField(levelCompleteManager, "levelCompleteUI", winUI);
            SetSerializedField(levelCompleteManager, "playerMotor", motor);
            SetSerializedField(levelCompleteManager, "playerLook", look);

            return levelCompleteManager;
        }

        private static void BuildEscapeZone(GameObject gameManager, LevelCompleteManager levelCompleteManager)
        {
            GameObject zone = GameObject.Find("EscapeZone");
            if (zone == null)
            {
                zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                zone.name = "EscapeZone";
                Undo.RegisterCreatedObjectUndo(zone, "Build Kitchen Level");
            }

            zone.transform.position = new Vector3(CabinetX, 0.15f, CabinetZ - 0.4f);
            zone.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);

            Collider collider = zone.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            // Hide the trigger's own render, or it looks like solid furniture blocking the path.
            Renderer renderer = zone.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            ObjectiveManager objectiveManager = gameManager.GetComponent<ObjectiveManager>();
            EscapeZone escapeZone = GetOrAddComponent<EscapeZone>(zone);
            SetSerializedField(escapeZone, "objectiveManager", objectiveManager);
            SetSerializedField(escapeZone, "levelCompleteManager", levelCompleteManager);
        }

        private static void RepositionCat()
        {
            GameObject cat = GameObject.Find("Cat");
            if (cat == null)
            {
                return; // Cat AI not built yet - not this tool's job to require it
            }

            // Guard the floor at the base of the traversal path (the cat can't jump/climb the
            // furniture itself). CatAI reads its spawn/patrol center from transform.position at
            // Awake(), so just moving it here is enough.
            cat.transform.position = GuardPosition;
        }

        private static void BuildBlock(string name, Vector3 position, Vector3 scale)
        {
            GameObject block = GameObject.Find(name);
            if (block == null)
            {
                block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.name = name;
                Undo.RegisterCreatedObjectUndo(block, "Build Kitchen Level");
            }

            block.transform.position = position;
            block.transform.localScale = scale;
        }

        private static GameObject FindChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            return child != null ? child.gameObject : null;
        }

        private static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = Undo.AddComponent<T>(go);
            }
            return component;
        }

        private static void SetSerializedField(Object target, string fieldName, Object value)
        {
            if (target == null)
            {
                return;
            }

            var so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"KitchenLevelBuilder: field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
