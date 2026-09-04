using UnityEngine;

namespace MouseGame.Input
{
    /// <summary>
    /// Reads keyboard/mouse AND mobile touch UI input and merges them into one small,
    /// device-agnostic API. PlayerMotor and PlayerLook only ever talk to this component, never
    /// to UnityEngine.Input or the touch UI components directly — that's what let mobile
    /// controls (Milestone 3) get added here without touching either of them.
    ///
    /// The mobile references are optional (nullable): a keyboard-only scene still works with
    /// them unassigned, and a scene with mobile UI still keeps keyboard/mouse working
    /// side by side, since both contribute to the same Move/Look/JumpPressed/SprintHeld values.
    ///
    /// Runs before any other script's default Update (see DefaultExecutionOrder) so every
    /// consumer reads this frame's input, not a stale value left over from last frame —
    /// Unity does not otherwise guarantee Update() order between sibling components.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Keyboard / Mouse")]
        [SerializeField] private string horizontalAxis = "Horizontal";
        [SerializeField] private string verticalAxis = "Vertical";
        [SerializeField] private string mouseXAxis = "Mouse X";
        [SerializeField] private string mouseYAxis = "Mouse Y";
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

        [Header("Mobile Touch UI (optional)")]
        [SerializeField] private VirtualJoystick moveJoystick;
        [SerializeField] private TouchLookArea lookArea;
        [SerializeField] private TapButton jumpButton;
        [SerializeField] private HoldButton sprintButton;
        [Tooltip("Scales touch drag delta (pixels) down to roughly the same range as Mouse X/Y " +
                 "from Input.GetAxis, so PlayerLook's one sensitivity value feels similar on both. " +
                 "Tune by feel once testing on the Device Simulator or a real device.")]
        [SerializeField] private float touchLookSensitivity = 0.05f;

        /// <summary>Move direction in local X/Z space, each axis in [-1, 1].</summary>
        public Vector2 Move { get; private set; }

        /// <summary>Raw look delta for this frame, in [-1, 1]-ish range (unscaled by sensitivity).</summary>
        public Vector2 Look { get; private set; }

        /// <summary>True for exactly the frame the jump input was pressed.</summary>
        public bool JumpPressed { get; private set; }

        /// <summary>True for every frame the sprint input is held down.</summary>
        public bool SprintHeld { get; private set; }

        private void Update()
        {
            Vector2 keyboardMove = new Vector2(
                UnityEngine.Input.GetAxisRaw(horizontalAxis),
                UnityEngine.Input.GetAxisRaw(verticalAxis));
            Vector2 touchMove = moveJoystick != null ? moveJoystick.Value : Vector2.zero;
            Move = Vector2.ClampMagnitude(keyboardMove + touchMove, 1f);

            Vector2 mouseLook = new Vector2(
                UnityEngine.Input.GetAxis(mouseXAxis),
                UnityEngine.Input.GetAxis(mouseYAxis));
            Vector2 touchLook = lookArea != null ? lookArea.ConsumeDelta() * touchLookSensitivity : Vector2.zero;
            Look = mouseLook + touchLook;

            JumpPressed = UnityEngine.Input.GetKeyDown(jumpKey)
                          || (jumpButton != null && jumpButton.ConsumePressed());
            SprintHeld = UnityEngine.Input.GetKey(sprintKey)
                         || (sprintButton != null && sprintButton.IsHeld);
        }
    }
}
