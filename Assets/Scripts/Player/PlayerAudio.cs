using MouseGame.Audio;
using MouseGame.Input;
using UnityEngine;

namespace MouseGame.Player
{
    /// <summary>
    /// Plays procedurally generated placeholder SFX for footsteps (while grounded and moving,
    /// faster cadence while sprinting) and PlayerMotor's Jumped/Landed events. See
    /// MouseGame.Audio.ProceduralAudio for why these are generated rather than real clips.
    /// </summary>
    [RequireComponent(typeof(PlayerMotor))]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(AudioSource))]
    public class PlayerAudio : MonoBehaviour
    {
        [Header("Footstep cadence")]
        [SerializeField] private float walkStepInterval = 0.35f;
        [SerializeField] private float sprintStepInterval = 0.22f;

        [Header("Volumes")]
        [SerializeField] private float footstepVolume = 0.25f;
        [SerializeField] private float jumpVolume = 0.35f;
        [SerializeField] private float landVolume = 0.35f;

        private PlayerMotor motor;
        private PlayerInputReader input;
        private AudioSource audioSource;
        private AudioClip footstepClip;
        private AudioClip jumpClip;
        private AudioClip landClip;
        private float stepTimer;

        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            input = GetComponent<PlayerInputReader>();
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D — this is the player's own sound, not a world source

            footstepClip = ProceduralAudio.CreateNoiseBurst("Footstep", 0.06f);
            jumpClip = ProceduralAudio.CreateTone("Jump", 900f, 0.12f);
            landClip = ProceduralAudio.CreateTone("Land", 300f, 0.1f);
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
            if (!motor.IsGrounded || input.Move.sqrMagnitude < 0.01f)
            {
                stepTimer = 0f;
                return;
            }

            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                audioSource.PlayOneShot(footstepClip, footstepVolume);
                stepTimer = input.SprintHeld ? sprintStepInterval : walkStepInterval;
            }
        }

        private void HandleJumped() => audioSource.PlayOneShot(jumpClip, jumpVolume);

        private void HandleLanded() => audioSource.PlayOneShot(landClip, landVolume);
    }
}
