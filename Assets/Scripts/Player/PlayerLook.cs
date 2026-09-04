using MouseGame.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MouseGame.Player
{
    /// <summary>
    /// Mouse-look camera control. Yaw (left/right) rotates the player body so movement stays
    /// aligned with facing direction; pitch (up/down) only rotates this camera transform.
    /// Attach to the camera, with <see cref="playerBody"/> pointing at the root player transform.
    ///
    /// Cursor locking is click-to-lock / Escape-to-unlock rather than automatic on Start. A
    /// locked cursor is pinned to the screen center (only its delta is readable) — great for
    /// classic mouse-look, but it makes dragging across the screen to different points
    /// impossible, which breaks the mobile touch UI (joystick/look-area/buttons) entirely once
    /// that's wired up. The over-UI check below means this adapts automatically: with mobile
    /// controls present (the look area covers the whole Canvas) the cursor never locks, so
    /// touch/drag testing works; without them, clicking the game view locks it for normal
    /// keyboard/mouse look, same as before.
    /// </summary>
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private Transform playerBody;
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private float mouseSensitivity = 200f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;
        [SerializeField] private bool lockCursor = true;

        private float pitch;

        private void Update()
        {
            HandleCursorLock();

            float mouseX = input.Look.x * mouseSensitivity * Time.deltaTime;
            float mouseY = input.Look.y * mouseSensitivity * Time.deltaTime;

            pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);
            transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            playerBody.Rotate(Vector3.up * mouseX);
        }

        private void HandleCursorLock()
        {
            if (!lockCursor)
            {
                return;
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }

                return;
            }

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (!overUI && UnityEngine.Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
