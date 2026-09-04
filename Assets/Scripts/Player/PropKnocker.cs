using MouseGame.Interaction;
using UnityEngine;

namespace MouseGame.Player
{
    /// <summary>
    /// Detects the player's CharacterController bumping into a KnockableProp and reports a
    /// noise pulse through NoiseEmitter. OnControllerColliderHit is called by Unity automatically
    /// on whatever component sits on the same GameObject as the CharacterController.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NoiseEmitter))]
    public class PropKnocker : MonoBehaviour
    {
        [Tooltip("Minimum time between knock noises from the same continuous contact, so pushing into a prop doesn't spam a noise every frame.")]
        [SerializeField] private float knockCooldown = 0.5f;

        private NoiseEmitter noiseEmitter;
        private float cooldownTimer;

        private void Awake()
        {
            noiseEmitter = GetComponent<NoiseEmitter>();
        }

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (cooldownTimer > 0f)
            {
                return;
            }

            KnockableProp prop = hit.collider.GetComponent<KnockableProp>();
            if (prop == null)
            {
                return;
            }

            noiseEmitter.EmitNoise(prop.NoiseRadius, prop.NoiseDuration);
            cooldownTimer = knockCooldown;
        }
    }
}
