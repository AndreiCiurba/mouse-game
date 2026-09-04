using MouseGame.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MouseGame.EditorTools
{
    /// <summary>
    /// One-click Milestone 3 wiring: builds the on-screen mobile controls (left joystick,
    /// right drag-to-look area, Jump/Sprint buttons) onto the existing objective Canvas and
    /// wires them into the Player's PlayerInputReader. Keyboard/mouse keeps working
    /// side by side — see PlayerInputReader's merge logic. Run "Mouse Game -> Build MVP
    /// Scene" first. Safe to re-run.
    /// </summary>
    public static class MobileControlsBuilder
    {
        [MenuItem("Mouse Game/Build Mobile Controls (Milestone 3)")]
        private static void BuildMobileControls()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Exit Play mode before building mobile controls.");
                return;
            }

            GameObject player = GameObject.Find("Player");
            PlayerInputReader inputReader = player != null ? player.GetComponent<PlayerInputReader>() : null;
            if (inputReader == null)
            {
                Debug.LogError("No 'Player' with a PlayerInputReader found. Run 'Mouse Game -> " +
                                "Build MVP Scene' first.");
                return;
            }

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found. Run 'Mouse Game -> Build MVP Scene' first.");
                return;
            }

            LockToLandscape();

            Undo.SetCurrentGroupName("Build Mobile Controls");
            int undoGroup = Undo.GetCurrentGroup();

            // Earliest sibling so the joystick/buttons (added after) render on top and correctly
            // intercept their own areas instead of this full-screen drag area stealing the touch.
            TouchLookArea lookArea = BuildLookArea(canvas.transform);
            VirtualJoystick joystick = BuildJoystick(canvas.transform);
            TapButton jumpButton = BuildTapButton(canvas.transform, "JumpButton", "Jump",
                new Vector2(1f, 0f), new Vector2(-90f, 140f));
            HoldButton sprintButton = BuildHoldButton(canvas.transform, "SprintButton", "Sprint",
                new Vector2(1f, 0f), new Vector2(-220f, 90f));

            SetSerializedField(inputReader, "moveJoystick", joystick);
            SetSerializedField(inputReader, "lookArea", lookArea);
            SetSerializedField(inputReader, "jumpButton", jumpButton);
            SetSerializedField(inputReader, "sprintButton", sprintButton);

            Undo.CollapseUndoOperations(undoGroup);
            MvpSceneBuilder.SaveActiveScene();

            Debug.Log("Mobile controls built and saved: left joystick (move), right drag area " +
                      "(look), Jump/Sprint buttons bottom-right. Keyboard/mouse still work too. " +
                      "Test via Window -> General -> Device Simulator.");
        }

        /// <summary>
        /// The joystick/look-area/button layout assumes a landscape screen. Player Settings
        /// default to Portrait for a new project — without this, the Device Simulator's rotate
        /// button does nothing (a real device locked to Portrait wouldn't rotate its content
        /// either, and the Simulator matches that faithfully), which looks like a bug but is
        /// actually just this setting never having been touched.
        /// </summary>
        private static void LockToLandscape()
        {
            // Also fixes a leftover from the temp-project merge during initial setup — the
            // product name was still "mouse-game-unity" (would show as the app's display name
            // on an actual Android install). Fixed here rather than a one-off file edit so it's
            // applied through the API (correct regardless of what the Editor has cached) and
            // covered by the same explicit save below.
            if (PlayerSettings.productName != "Mouse Game")
            {
                PlayerSettings.productName = "Mouse Game";
            }

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            // Same lesson as the scene-save bug: PlayerSettings changes aren't guaranteed to hit
            // disk just because they were applied in-memory (found this one still unpersisted —
            // ProjectSettings.asset still showed AutoRotation/portrait allowed — during a later
            // review, despite this method having already been run at least once).
            AssetDatabase.SaveAssets();
        }

        private static TouchLookArea BuildLookArea(Transform canvasTransform)
        {
            GameObject go = FindChild(canvasTransform, "LookArea");
            TouchLookArea area;
            if (go == null)
            {
                go = new GameObject("LookArea", typeof(Image), typeof(TouchLookArea));
                Undo.RegisterCreatedObjectUndo(go, "Build Mobile Controls");
                go.transform.SetParent(canvasTransform, false);
                go.transform.SetAsFirstSibling();

                Image image = go.GetComponent<Image>();
                image.color = new Color(0f, 0f, 0f, 0f); // invisible, but still a raycast target
                image.raycastTarget = true;

                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                area = go.GetComponent<TouchLookArea>();
            }
            else
            {
                go.transform.SetAsFirstSibling();
                area = go.GetComponent<TouchLookArea>();
            }

            return area;
        }

        private static VirtualJoystick BuildJoystick(Transform canvasTransform)
        {
            GameObject bg = FindChild(canvasTransform, "MoveJoystickBackground");
            VirtualJoystick joystick;
            RectTransform handleRect;

            if (bg == null)
            {
                bg = new GameObject("MoveJoystickBackground", typeof(Image), typeof(VirtualJoystick));
                Undo.RegisterCreatedObjectUndo(bg, "Build Mobile Controls");
                bg.transform.SetParent(canvasTransform, false);

                Image bgImage = bg.GetComponent<Image>();
                bgImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
                bgImage.color = new Color(1f, 1f, 1f, 0.25f);
                bgImage.raycastTarget = true;

                RectTransform bgRect = bg.GetComponent<RectTransform>();
                bgRect.anchorMin = new Vector2(0f, 0f);
                bgRect.anchorMax = new Vector2(0f, 0f);
                bgRect.pivot = new Vector2(0.5f, 0.5f);
                bgRect.sizeDelta = new Vector2(150f, 150f);
                bgRect.anchoredPosition = new Vector2(140f, 140f);

                GameObject handle = new GameObject("MoveJoystickHandle", typeof(Image));
                Undo.RegisterCreatedObjectUndo(handle, "Build Mobile Controls");
                handle.transform.SetParent(bg.transform, false);

                Image handleImage = handle.GetComponent<Image>();
                handleImage.sprite = bgImage.sprite;
                handleImage.color = new Color(1f, 1f, 1f, 0.6f);
                handleImage.raycastTarget = false;

                handleRect = handle.GetComponent<RectTransform>();
                handleRect.sizeDelta = new Vector2(70f, 70f);
                handleRect.anchoredPosition = Vector2.zero;

                joystick = bg.GetComponent<VirtualJoystick>();
                SetSerializedField(joystick, "background", bgRect);
                SetSerializedField(joystick, "handle", handleRect);
            }
            else
            {
                joystick = bg.GetComponent<VirtualJoystick>();
            }

            return joystick;
        }

        private static TapButton BuildTapButton(Transform canvasTransform, string name, string label,
            Vector2 anchor, Vector2 anchoredPosition)
        {
            GameObject go = BuildButtonBase(canvasTransform, name, label, anchor, anchoredPosition,
                typeof(TapButton));
            return go.GetComponent<TapButton>();
        }

        private static HoldButton BuildHoldButton(Transform canvasTransform, string name, string label,
            Vector2 anchor, Vector2 anchoredPosition)
        {
            GameObject go = BuildButtonBase(canvasTransform, name, label, anchor, anchoredPosition,
                typeof(HoldButton));
            return go.GetComponent<HoldButton>();
        }

        private static GameObject BuildButtonBase(Transform canvasTransform, string name, string label,
            Vector2 anchor, Vector2 anchoredPosition, System.Type inputComponentType)
        {
            GameObject go = FindChild(canvasTransform, name);
            if (go != null)
            {
                return go;
            }

            go = new GameObject(name, typeof(Image), inputComponentType);
            Undo.RegisterCreatedObjectUndo(go, "Build Mobile Controls");
            go.transform.SetParent(canvasTransform, false);

            Image image = go.GetComponent<Image>();
            image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            image.color = new Color(1f, 1f, 1f, 0.4f);
            image.raycastTarget = true;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(110f, 110f);
            rt.anchoredPosition = anchoredPosition;

            GameObject textGO = new GameObject("Label", typeof(Text));
            Undo.RegisterCreatedObjectUndo(textGO, "Build Mobile Controls");
            textGO.transform.SetParent(go.transform, false);

            Text text = textGO.GetComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.raycastTarget = false;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return go;
        }

        private static GameObject FindChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            return child != null ? child.gameObject : null;
        }

        private static void SetSerializedField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"MobileControlsBuilder: field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
