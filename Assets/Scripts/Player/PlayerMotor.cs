using MouseGame.Input;
using UnityEngine;

namespace MouseGame.Player
{
    /// <summary>
    /// Basic first-person movement: walk, sprint, jump, gravity, collision (via CharacterController).
    /// Climbing lives in the separate PlayerClimb component, which disables this one while a climb
    /// is in progress. Jump height is intentionally small — see the jumpHeight tooltip.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float sprintSpeed = 6f;

        [Header("Jump / Gravity")]
        [Tooltip("Keep this below PlayerClimb's ledge height range, or a plain jump lets you " +
                 "skip climbing altogether and land straight on climbable furniture.")]
        [SerializeField] private float jumpHeight = 0.3f;
        [SerializeField] private float gravity = -20f;
        [Tooltip("Small downward force applied while grounded so CharacterController.isGrounded stays reliable.")]
        [SerializeField] private float groundedStickForce = -2f;

        private CharacterController controller;
        private PlayerInputReader input;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<PlayerInputReader>();
        }

        private void Update()
        {
            ApplyHorizontalMovement();
            ApplyVerticalMovement();
        }

        private void ApplyHorizontalMovement()
        {
            Vector3 moveDirection = transform.right * input.Move.x + transform.forward * input.Move.y;
            moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

            float speed = input.SprintHeld ? sprintSpeed : walkSpeed;
            controller.Move(moveDirection * speed * Time.deltaTime);
        }

        private void ApplyVerticalMovement()
        {
            bool grounded = controller.isGrounded;

            if (grounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickForce;
            }

            if (grounded && input.JumpPressed)
            {
                // v = sqrt(h * -2 * g)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;
            controller.Move(new Vector3(0f, verticalVelocity * Time.deltaTime, 0f));
        }
    }
}
