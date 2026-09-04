using MouseGame.Input;
using UnityEngine;

namespace MouseGame.Player
{
    /// <summary>
    /// Turns player movement into a noise radius: walking is quiet, sprinting is noticeable, and
    /// jumping/landing are brief noticeable pulses. CatHearing reads CurrentNoiseRadius each
    /// frame instead of using a flat proximity check.
    ///
    /// Also exposes EmitNoise() as an extension point for future systems (e.g. knocking into a
    /// prop = loud, per the README) — nothing calls it yet since there's no prop-interaction
    /// system built, but CatHearing doesn't need to change when one is.
    /// </summary>
    [RequireComponent(typeof(PlayerMotor))]
    [RequireComponent(typeof(PlayerInputReader))]
    public class NoiseEmitter : MonoBehaviour
    {
        [Header("Noise radii")]
        [SerializeField] private float walkNoiseRadius = 0.3f;
        [SerializeField] private float sprintNoiseRadius = 0.6f;
        [SerializeField] private float jumpNoiseRadius = 0.6f;
        [SerializeField] private float landNoiseRadius = 0.6f;
        [Tooltip("How long a jump/land noise pulse stays audible before fading out.")]
        [SerializeField] private float pulseDuration = 0.3f;

        private PlayerMotor motor;
        private PlayerInputReader input;
        private float pulseRadius;
        private float pulseTimer;

        /// <summary>How far this frame's noise can currently be heard from (0 = silent).</summary>
        public float CurrentNoiseRadius { get; private set; }

        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            input = GetComponent<PlayerInputReader>();
        }

        private void OnEnable()
        {
            motor.Jumped += HandleJumped;
            motor.Landed += HandleLanded;
        }

        private void OnDisable()
        {
            motor.Jumped -= HandleJumped;
            motor.Landed -= HandleLanded;
        }

        private void Update()
        {
            float movementRadius = 0f;
            if (input.Move.sqrMagnitude > 0.01f)
            {
                movementRadius = input.SprintHeld ? sprintNoiseRadius : walkNoiseRadius;
            }

            if (pulseTimer > 0f)
            {
                pulseTimer -= Time.deltaTime;
            }
            else
            {
                pulseRadius = 0f;
            }

            CurrentNoiseRadius = Mathf.Max(movementRadius, pulseRadius);
        }

        /// <summary>Reports a one-off noise pulse from outside normal movement (e.g. knocking a prop).</summary>
        public void EmitNoise(float radius, float duration = -1f)
        {
            Pulse(radius, duration > 0f ? duration : pulseDuration);
        }

        private void HandleJumped() => Pulse(jumpNoiseRadius, pulseDuration);

        private void HandleLanded() => Pulse(landNoiseRadius, pulseDuration);

        private void Pulse(float radius, float duration)
        {
            // Don't let a smaller/shorter pulse cut a bigger one already in progress.
            if (radius < pulseRadius && pulseTimer > 0f)
            {
                return;
            }

            pulseRadius = radius;
            pulseTimer = duration;
        }
    }
}
