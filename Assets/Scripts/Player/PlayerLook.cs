using MouseGame.Input;
using UnityEngine;

namespace MouseGame.Player
{
    /// <summary>
    /// Mouse-look camera control. Yaw (left/right) rotates the player body so movement stays
    /// aligned with facing direction; pitch (up/down) only rotates this camera transform.
    /// Attach to the camera, with <see cref="playerBody"/> pointing at the root player transform.
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

        private void Start()
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            float mouseX = input.Look.x * mouseSensitivity * Time.deltaTime;
            float mouseY = input.Look.y * mouseSensitivity * Time.deltaTime;

            pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);
            transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
