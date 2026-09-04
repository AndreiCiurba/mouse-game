using MouseGame.AI;
using MouseGame.Game;
using MouseGame.Player;
using MouseGame.UI;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace MouseGame.EditorTools
{
    /// <summary>
    /// One-click Milestone 5 wiring: bakes a NavMesh over the room, builds the "Cat" GameObject
    /// (NavMeshAgent + CatVision + CatHearing + CatAI + primitive model), and wires up the
    /// GameOverManager/GameOverUI lose-state. Run "Mouse Game -> Build MVP Scene" and
    /// "Mouse Game -> Build Test Room" first. Safe to re-run.
    ///
    /// Simplification: the NavMesh itself is baked with Unity's default "Humanoid" agent type
    /// (radius 0.5, height 2) rather than a custom cat-sized bake — creating/registering a new
    /// agent type is editor-UI territory with no reliable scripting path, and the room is
    /// human-scale anyway (deliberately, so the mouse reads as small — see MvpSceneBuilder), so
    /// a human-sized walkable area comfortably contains a cat-sized agent. The NavMeshAgent
    /// component itself IS set to cat-appropriate radius/height for movement/avoidance; it just
    /// won't hug walls quite as tightly as a true cat-sized bake would allow.
    /// </summary>
    public static class CatAIBuilder
    {
        [MenuItem("Mouse Game/Build Cat AI (Milestone 5)")]
        private static void BuildCatAI()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Exit Play mode before building the cat AI.");
                return;
            }

            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("No 'Player' found. Run 'Mouse Game -> Build MVP Scene' first.");
                return;
            }

            if (GameObject.Find("Floor") == null)
            {
                Debug.LogError("No 'Floor' found. Run 'Mouse Game -> Build Test Room' first.");
                return;
            }

            Undo.SetCurrentGroupName("Build Cat AI");
            int undoGroup = Undo.GetCurrentGroup();

            BakeNavMesh(player.layer);
            GameOverManager gameOverManager = BuildGameOver(player);
            BuildCat(gameOverManager);

            Undo.CollapseUndoOperations(undoGroup);
            MvpSceneBuilder.SaveActiveScene();

            Debug.Log("Cat AI built and saved: NavMesh baked, 'Cat' patrolling near (2, 0, -2), " +
                      "Game Over wired up. The cat sees within its vision cone and hears within " +
                      "a small radius (Milestone 6 will replace hearing with real noise levels).");
        }

        private static void BakeNavMesh(int playerLayer)
        {
            GameObject surfaceGO = GameObject.Find("NavMeshSurface");
            if (surfaceGO == null)
            {
                surfaceGO = new GameObject("NavMeshSurface", typeof(NavMeshSurface));
                Undo.RegisterCreatedObjectUndo(surfaceGO, "Build Cat AI");
            }

            NavMeshSurface surface = surfaceGO.GetComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            // Exclude the Player's layer - its mouse-model parts have renderers the baker would
            // otherwise try to carve around, which makes no sense for a moving character.
            surface.layerMask = ~(1 << playerLayer);
            surface.BuildNavMesh();
        }

        private static GameOverManager BuildGameOver(GameObject player)
        {
            GameObject gameManager = GameObject.Find("GameManager");
            if (gameManager == null)
            {
                gameManager = new GameObject("GameManager");
                Undo.RegisterCreatedObjectUndo(gameManager, "Build Cat AI");
            }

            GameOverUI gameOverUI = GetOrAddComponent<GameOverUI>(gameManager);
            GameOverManager gameOverManager = GetOrAddComponent<GameOverManager>(gameManager);

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found. Run 'Mouse Game -> Build MVP Scene' first.");
                return gameOverManager;
            }

            GameObject textGO = FindChild(canvas.transform, "GameOverText");
            Text text;
            if (textGO == null)
            {
                textGO = new GameObject("GameOverText", typeof(Text));
                Undo.RegisterCreatedObjectUndo(textGO, "Build Cat AI");
                textGO.transform.SetParent(canvas.transform, false);

                text = textGO.GetComponent<Text>();
                text.text = "Caught! Game Over";
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 48;
                text.color = Color.red;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                RectTransform rt = textGO.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(600f, 120f);
            }
            else
            {
                text = textGO.GetComponent<Text>();
            }

            SetSerializedField(gameOverUI, "messagePanel", textGO);
            SetSerializedField(gameOverUI, "messageText", text);

            PlayerMotor motor = player.GetComponent<PlayerMotor>();
            PlayerLook look = player.GetComponentInChildren<PlayerLook>();
            SetSerializedField(gameOverManager, "gameOverUI", gameOverUI);
            SetSerializedField(gameOverManager, "playerMotor", motor);
            SetSerializedField(gameOverManager, "playerLook", look);

            return gameOverManager;
        }

        private static void BuildCat(GameOverManager gameOverManager)
        {
            GameObject cat = GameObject.Find("Cat");
            if (cat == null)
            {
                cat = new GameObject("Cat");
                Undo.RegisterCreatedObjectUndo(cat, "Build Cat AI");
            }

            cat.transform.position = new Vector3(2f, 0f, -2f);

            NavMeshAgent agent = GetOrAddComponent<NavMeshAgent>(cat);
            agent.radius = 0.15f;
            agent.height = 0.3f;
            agent.baseOffset = 0f;
            agent.stoppingDistance = 0.05f;

            GetOrAddComponent<CatVision>(cat);
            GetOrAddComponent<CatHearing>(cat);
            CatAI catAI = GetOrAddComponent<CatAI>(cat);
            SetSerializedField(catAI, "gameOverManager", gameOverManager);

            CatModelBuilder.BuildCatModel(cat.transform);
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
                Debug.LogError($"CatAIBuilder: field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
