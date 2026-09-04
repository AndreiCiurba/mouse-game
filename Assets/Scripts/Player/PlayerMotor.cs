using MouseGame.Input;
using UnityEngine;

namespace MouseGame.Player
{
    /// <summary>
    /// Basic first-person movement: walk, sprint, jump (up to maxJumps before landing again),
    /// gravity, collision (via CharacterController). Whether a jump actually lands you on
    /// something is just a product of the physics (arc height vs. what's in the way), same as
    /// any normal jump. Stairs/low ledges aren't handled here at all — see
    /// CharacterController.stepOffset, configured by MvpSceneBuilder, which walks the player up
    /// steps shorter than that threshold automatically as part of normal movement.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float sprintSpeed = 6f;

        [Header("Jump / Gravity")]
        [SerializeField] private float jumpHeight = 0.9f;
        [SerializeField] private float gravity = -20f;
        [Tooltip("Small downward force applied while grounded to keep the character settled onto the floor.")]
        [SerializeField] private float groundedStickForce = -2f;
        [Tooltip("How many jumps are allowed before you must touch ground again (2 = one air jump/double jump).")]
        [SerializeField] private int maxJumps = 2;

        [Header("Ground Check")]
        [Tooltip("CharacterController.isGrounded is unreliable on flat/open ground (it can flicker " +
                 "false with nothing nearby to help register contact, while feeling fine next to " +
                 "walls/steps) — so grounding uses an explicit overlap check instead. Keep this tight " +
                 "(barely reaching past the CharacterController's skin width) — too generous and it " +
                 "reads 'grounded' while still visibly airborne, which breaks the jump-count limit.")]
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private float groundCheckRadius = 0.12f;
        [Tooltip("Should exclude the Player's own layer, or the check can detect itself. " +
                 "MvpSceneBuilder sets this to everything except the auto-created 'Player' layer.")]
        [SerializeField] private LayerMask groundMask = ~0;

        private CharacterController controller;
        private PlayerInputReader input;
        private float verticalVelocity;
        private int jumpsUsedSinceGrounded;

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
            bool grounded = CheckGrounded();

            if (grounded)
            {
                jumpsUsedSinceGrounded = 0;

                if (verticalVelocity < 0f)
                {
                    verticalVelocity = groundedStickForce;
                }
            }

            if (input.JumpPressed && jumpsUsedSinceGrounded < maxJumps)
            {
                // v = sqrt(h * -2 * g)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpsUsedSinceGrounded++;
            }

            verticalVelocity += gravity * Time.deltaTime;
            controller.Move(new Vector3(0f, verticalVelocity * Time.deltaTime, 0f));
        }

        private bool CheckGrounded()
        {
            float halfHeight = controller.height * 0.5f;
            Vector3 feet = transform.position + Vector3.up * (controller.center.y - halfHeight);
            Vector3 checkCenter = feet - Vector3.up * groundCheckDistance;

            return Physics.CheckSphere(checkCenter, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
        }
    }
}
