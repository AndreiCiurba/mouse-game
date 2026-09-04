using MouseGame.Player;
using UnityEngine;

namespace MouseGame.AI
{
    /// <summary>
    /// Hears the player based on their current noise radius (NoiseEmitter) — walking is quiet,
    /// sprinting/jumping/landing carry further — rather than a flat distance check.
    /// </summary>
    public class CatHearing : MonoBehaviour
    {
        private Transform player;
        private NoiseEmitter noiseEmitter;

        private void Awake()
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                player = playerGO.transform;
                noiseEmitter = playerGO.GetComponent<NoiseEmitter>();
            }
        }

        public bool CanHearPlayer(out Vector3 playerPosition)
        {
            playerPosition = default;
            if (player == null || noiseEmitter == null)
            {
                return false;
            }

            float noiseRadius = noiseEmitter.CurrentNoiseRadius;
            if (noiseRadius <= 0f)
            {
                return false;
            }

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > noiseRadius)
            {
                return false;
            }

            playerPosition = player.position;
            return true;
        }
    }
}
