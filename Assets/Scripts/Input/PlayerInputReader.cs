using UnityEngine;

namespace MouseGame.Input
{
    /// <summary>
    /// Reads raw keyboard/mouse input and exposes it as a small, device-agnostic API.
    /// PlayerMotor and PlayerLook only ever talk to this component, never to UnityEngine.Input
    /// directly, so Milestone 3 (mobile touch controls) can swap the internals of this class
    /// for a virtual joystick / touch-drag implementation without touching the player scripts.
    ///
    /// Runs before any other script's default Update (see DefaultExecutionOrder) so every
    /// consumer reads this frame's input, not a stale value left over from last frame —
    /// Unity does not otherwise guarantee Update() order between sibling components.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private string horizontalAxis = "Horizontal";
        [SerializeField] private string verticalAxis = "Vertical";
        [SerializeField] private string mouseXAxis = "Mouse X";
        [SerializeField] private string mouseYAxis = "Mouse Y";
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode climbKey = KeyCode.E;

        /// <summary>Move direction in local X/Z space, each axis in [-1, 1].</summary>
        public Vector2 Move { get; private set; }

        /// <summary>Raw mouse delta for this frame, in [-1, 1]-ish range (unscaled by sensitivity).</summary>
        public Vector2 Look { get; private set; }

        /// <summary>True for exactly the frame the jump input was pressed.</summary>
        public bool JumpPressed { get; private set; }

        /// <summary>True for every frame the sprint input is held down.</summary>
        public bool SprintHeld { get; private set; }

        /// <summary>True for exactly the frame the climb input was pressed.</summary>
        public bool ClimbPressed { get; private set; }

        private void Update()
        {
            Move = new Vector2(
                UnityEngine.Input.GetAxisRaw(horizontalAxis),
                UnityEngine.Input.GetAxisRaw(verticalAxis));

            Look = new Vector2(
                UnityEngine.Input.GetAxis(mouseXAxis),
                UnityEngine.Input.GetAxis(mouseYAxis));

            JumpPressed = UnityEngine.Input.GetKeyDown(jumpKey);
            SprintHeld = UnityEngine.Input.GetKey(sprintKey);
            ClimbPressed = UnityEngine.Input.GetKeyDown(climbKey);
        }
    }
}
