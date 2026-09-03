using UnityEngine;

namespace MouseGame.Input
{
    /// <summary>
    /// Reads raw keyboard/mouse input and exposes it as a small, device-agnostic API.
    /// PlayerMotor and PlayerLook only ever talk to this component, never to UnityEngine.Input
    /// directly, so Milestone 3 (mobile touch controls) can swap the internals of this class
    /// for a virtual joystick / touch-drag implementation without touching the player scripts.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private string horizontalAxis = "Horizontal";
        [SerializeField] private string verticalAxis = "Vertical";
        [SerializeField] private string mouseXAxis = "Mouse X";
        [SerializeField] private string mouseYAxis = "Mouse Y";
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

        /// <summary>Move direction in local X/Z space, each axis in [-1, 1].</summary>
        public Vector2 Move { get; private set; }

        /// <summary>Raw mouse delta for this frame, in [-1, 1]-ish range (unscaled by sensitivity).</summary>
        public Vector2 Look { get; private set; }

        /// <summary>True for exactly the frame the jump input was pressed.</summary>
        public bool JumpPressed { get; private set; }

        /// <summary>True for every frame the sprint input is held down.</summary>
        public bool SprintHeld { get; private set; }

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
        }
    }
}
