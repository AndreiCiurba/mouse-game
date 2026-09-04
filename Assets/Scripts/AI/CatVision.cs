using UnityEngine;

namespace MouseGame.AI
{
    /// <summary>
    /// Cone + line-of-sight player detection. Purely visual — see CatHearing for the
    /// (currently proximity-based, eventually noise-based) hearing sense.
    /// </summary>
    public class CatVision : MonoBehaviour
    {
        [SerializeField] private Transform eye;
        [SerializeField] private float visionRange = 3f;
        [Tooltip("Full cone angle, in degrees, centered on forward.")]
        [SerializeField] private float visionAngle = 100f;
        [Tooltip("Anything on this mask blocks line of sight (walls, furniture, ...).")]
        [SerializeField] private LayerMask obstacleMask = ~0;

        private Transform player;

        private void Awake()
        {
            if (eye == null)
            {
                eye = transform;
            }

            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                player = playerGO.transform;
            }
        }

        /// <summary>True if the player is within range, within the vision cone, and unobstructed.</summary>
        public bool CanSeePlayer(out Vector3 playerPosition)
        {
            playerPosition = default;
            if (player == null)
            {
                return false;
            }

            Vector3 toPlayer = player.position - eye.position;
            float distance = toPlayer.magnitude;
            if (distance > visionRange)
            {
                return false;
            }

            float angle = Vector3.Angle(eye.forward, toPlayer);
            if (angle > visionAngle * 0.5f)
            {
                return false;
            }

            if (Physics.Raycast(eye.position, toPlayer.normalized, out RaycastHit hit, distance,
                    obstacleMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != player && !hit.transform.IsChildOf(player))
                {
                    return false; // something else is in the way
                }
            }

            playerPosition = player.position;
            return true;
        }
    }
}
