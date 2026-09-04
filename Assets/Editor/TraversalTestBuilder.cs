using MouseGame.Environment;
using MouseGame.Player;
using MouseGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MouseGame.EditorTools
{
    /// <summary>
    /// One-click Milestone 2 wiring: adds PlayerClimb to the existing Player, a "Press E to
    /// climb" prompt, and a couple of Climbable test props (a low box, then a higher table) to
    /// practice mantling. Run "Mouse Game -> Build MVP Scene" first — this expects Player and
    /// GameManager to already exist. Safe to re-run.
    /// </summary>
    public static class TraversalTestBuilder
    {
        [MenuItem("Mouse Game/Build Traversal Test (Milestone 2)")]
        private static void BuildTraversalTest()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Exit Play mode before building the traversal test.");
                return;
            }

            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("No 'Player' found. Run 'Mouse Game -> Build MVP Scene' first.");
                return;
            }

            Undo.SetCurrentGroupName("Build Traversal Test");
            int undoGroup = Undo.GetCurrentGroup();

            ClimbPromptUI promptUI = BuildClimbPrompt();
            PlayerClimb climb = GetOrAddComponent<PlayerClimb>(player);
            SetSerializedField(climb, "promptUI", promptUI);

            BuildClimbProp("ClimbBox", new Vector3(-1.5f, 0.25f, 0f), new Vector3(0.6f, 0.5f, 0.6f));
            BuildClimbProp("ClimbTable", new Vector3(-1.5f, 0.5f, 1f), new Vector3(0.8f, 1f, 0.8f));

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log("Traversal test built: PlayerClimb wired on Player, 'ClimbBox' (0.5m) and " +
                      "'ClimbTable' (1m) added near (-1.5, *, 0..1). Drag them into your room if " +
                      "that's outside your walls. Walk up to a box and press E when prompted.");
        }

        private static ClimbPromptUI BuildClimbPrompt()
        {
            GameObject gameManager = GameObject.Find("GameManager");
            if (gameManager == null)
            {
                gameManager = new GameObject("GameManager");
                Undo.RegisterCreatedObjectUndo(gameManager, "Build Traversal Test");
            }

            ClimbPromptUI promptUI = GetOrAddComponent<ClimbPromptUI>(gameManager);

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            GameObject canvasGO;
            if (canvas == null)
            {
                canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Undo.RegisterCreatedObjectUndo(canvasGO, "Build Traversal Test");
                canvas = canvasGO.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            else
            {
                canvasGO = canvas.gameObject;
            }

            GameObject textGO = FindChild(canvasGO.transform, "ClimbPromptText");
            if (textGO == null)
            {
                textGO = new GameObject("ClimbPromptText", typeof(Text));
                Undo.RegisterCreatedObjectUndo(textGO, "Build Traversal Test");
                textGO.transform.SetParent(canvasGO.transform, false);

                Text text = textGO.GetComponent<Text>();
                text.text = "Press E to climb";
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 28;
                text.color = Color.white;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                RectTransform rt = textGO.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, 80f);
                rt.sizeDelta = new Vector2(400f, 60f);

                textGO.SetActive(false);
            }

            SetSerializedField(promptUI, "promptRoot", textGO);
            return promptUI;
        }

        private static void BuildClimbProp(string name, Vector3 position, Vector3 scale)
        {
            GameObject prop = GameObject.Find(name);
            if (prop == null)
            {
                prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                prop.name = name;
                Undo.RegisterCreatedObjectUndo(prop, "Build Traversal Test");
            }

            prop.transform.position = position;
            prop.transform.localScale = scale;
            GetOrAddComponent<Climbable>(prop);
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
                Debug.LogError($"TraversalTestBuilder: field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
