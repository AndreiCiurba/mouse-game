using UnityEngine;

namespace MouseGame.Interaction
{
    /// <summary>
    /// A small prop that emits a loud noise when the player bumps into it — the README's
    /// "knocking a prop = loud" noise level, the one piece of the noise system NoiseEmitter left
    /// as an unused hook (EmitNoise()) since nothing called it yet.
    /// </summary>
    public class KnockableProp : MonoBehaviour
    {
        [SerializeField] private float noiseRadius = 1f;
        [SerializeField] private float noiseDuration = 0.4f;

        public float NoiseRadius => noiseRadius;
        public float NoiseDuration => noiseDuration;
    }
}
