using MouseGame.Game;
using MouseGame.Input;
using MouseGame.Interaction;
using MouseGame.Player;
using MouseGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MouseGame.EditorTools
{
    /// <summary>
    /// One-click MVP-1 scene wiring: builds the Player (CharacterController + camera),
    /// the objective UI, and a pickup item, and wires their Inspector references —
    /// everything from SETUP.md steps 3-5 that would otherwise be manual Inspector work.
    /// Does NOT touch room geometry (floor/walls) — build that by hand first.
    /// Safe to re-run: every piece is looked up by name/type before being created.
    /// </summary>
    public static class MvpSceneBuilder
    {
        [MenuItem("Mouse Game/Build MVP Scene (Player + Objective)")]
        private static void BuildMvpScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Exit Play mode before building the MVP scene.");
                return;
            }

            Undo.SetCurrentGroupName("Build MVP Scene");
            int undoGroup = Undo.GetCurrentGroup();

            GameObject player = BuildPlayer(out PlayerInputReader inputReader);
            ObjectiveManager objectiveManager = BuildObjective();
            BuildPickupItem(objectiveManager);
            CleanUpOldClimbSystem(player);

            Selection.activeGameObject = player;
            Undo.CollapseUndoOperations(undoGroup);
            SaveActiveScene();

            Debug.Log("MVP scene built and saved to disk: Player (move/look/jump/sprint), " +
                      "objective UI, and a 'Cheese' pickup near the world origin. Press Play to " +
                      "test; move the Cheese object in the Scene view if it doesn't land inside " +
                      "your room.");
        }

        private static GameObject BuildPlayer(out PlayerInputReader inputReader)
        {
            GameObject player = GetOrCreateRoot("Player");
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.tag = "Player";

            CharacterController controller = GetOrAddComponent<CharacterController>(player);
            // Steps shorter than this are walked up automatically as part of normal movement —
            // this is what makes stairs work with no custom script at all. Keep individual
            // stair-tread rises (see StairsTestBuilder) safely under this value.
            controller.stepOffset = 0.3f;

            inputReader = GetOrAddComponent<PlayerInputReader>(player);
            PlayerMotor motor = GetOrAddComponent<PlayerMotor>(player);
            // Force this even on a pre-existing PlayerMotor, in case an older build left a
            // different value here.
            SetSerializedField(motor, "jumpHeight", 0.9f);

            // Visible capsule body (CharacterController itself is invisible).
            GameObject body = FindChild(player.transform, "Body");
            if (body == null)
            {
                body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body";
                Undo.RegisterCreatedObjectUndo(body, "Build MVP Scene");
                body.transform.SetParent(player.transform, false);
                Object.DestroyImmediate(body.GetComponent<CapsuleCollider>());
            }

            // Remove the template's default root-level camera so we don't end up with two
            // MainCamera/AudioListener objects in the scene.
            GameObject defaultCam = GameObject.Find("Main Camera");
            if (defaultCam != null && defaultCam.transform.parent == null)
            {
                Undo.DestroyObjectImmediate(defaultCam);
            }

            GameObject camGO = FindChild(player.transform, "PlayerCamera");
            if (camGO == null)
            {
                camGO = new GameObject("PlayerCamera", typeof(Camera), typeof(AudioListener));
                Undo.RegisterCreatedObjectUndo(camGO, "Build MVP Scene");
                camGO.transform.SetParent(player.transform, false);
            }
            camGO.tag = "MainCamera";
            camGO.transform.localPosition = new Vector3(0f, 0.7f, 0f);

            PlayerLook look = GetOrAddComponent<PlayerLook>(camGO);
            SetSerializedField(look, "playerBody", player.transform);
            SetSerializedField(look, "input", inputReader);

            return player;
        }

        private static ObjectiveManager BuildObjective()
        {
            GameObject gameManager = GetOrCreateRoot("GameManager");
            ObjectiveUI objectiveUI = GetOrAddComponent<ObjectiveUI>(gameManager);
            ObjectiveManager objectiveManager = GetOrAddComponent<ObjectiveManager>(gameManager);

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            GameObject canvasGO;
            if (canvas == null)
            {
                canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Undo.RegisterCreatedObjectUndo(canvasGO, "Build MVP Scene");
                canvas = canvasGO.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            else
            {
                canvasGO = canvas.gameObject;
            }

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(eventSystem, "Build MVP Scene");
            }

            GameObject textGO = FindChild(canvasGO.transform, "FoundMessageText");
            Text foundText;
            if (textGO == null)
            {
                textGO = new GameObject("FoundMessageText", typeof(Text));
                Undo.RegisterCreatedObjectUndo(textGO, "Build MVP Scene");
                textGO.transform.SetParent(canvasGO.transform, false);

                foundText = textGO.GetComponent<Text>();
                foundText.text = "Found it!";
                foundText.alignment = TextAnchor.MiddleCenter;
                foundText.fontSize = 36;
                foundText.color = Color.white;
                foundText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                RectTransform rt = textGO.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, -60f);
                rt.sizeDelta = new Vector2(400f, 80f);
            }
            else
            {
                foundText = textGO.GetComponent<Text>();
            }

            SetSerializedField(objectiveUI, "foundMessagePanel", textGO);
            SetSerializedField(objectiveUI, "foundMessageText", foundText);
            SetSerializedField(objectiveManager, "objectiveUI", objectiveUI);

            return objectiveManager;
        }

        private static void BuildPickupItem(ObjectiveManager objectiveManager)
        {
            GameObject cheese = GameObject.Find("Cheese");
            if (cheese == null)
            {
                cheese = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cheese.name = "Cheese";
                Undo.RegisterCreatedObjectUndo(cheese, "Build MVP Scene");
                cheese.transform.position = new Vector3(1f, 0.3f, 1f);
                cheese.transform.localScale = Vector3.one * 0.3f;
            }

            Collider col = cheese.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            CollectibleItem collectible = GetOrAddComponent<CollectibleItem>(cheese);
            SetSerializedField(collectible, "objectiveManager", objectiveManager);
        }

        /// <summary>
        /// Removes leftovers from the earlier E-climb prototype (now replaced by automatic
        /// stair-stepping): the ClimbBox/ClimbTable/ClimbPromptText test objects, and any
        /// "Missing Script" components left behind on Player/GameManager now that
        /// PlayerClimb/Climbable/ClimbPromptUI no longer exist as scripts.
        /// </summary>
        private static void CleanUpOldClimbSystem(GameObject player)
        {
            foreach (string name in new[] { "ClimbBox", "ClimbTable", "ClimbPromptText" })
            {
                GameObject leftover = GameObject.Find(name);
                if (leftover != null)
                {
                    Undo.DestroyObjectImmediate(leftover);
                }
            }

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(player);

            GameObject gameManager = GameObject.Find("GameManager");
            if (gameManager != null)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameManager);
            }
        }

        /// <summary>
        /// Marking a scene dirty does NOT persist it — only an explicit save (or the user
        /// pressing Ctrl+S) writes it to disk. Every build tool must call this at the end, or
        /// everything it built only lives in the Editor's memory until the next domain reload
        /// silently drops it.
        /// </summary>
        internal static void SaveActiveScene()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject GetOrCreateRoot(string name)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
            {
                return existing;
            }

            GameObject created = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(created, "Build MVP Scene");
            return created;
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
            var so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"MvpSceneBuilder: field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }

        private static void SetSerializedField(Object target, string fieldName, float value)
        {
            var so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"MvpSceneBuilder: field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            prop.floatValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
