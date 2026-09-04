using UnityEngine;

namespace MouseGame.AI
{
    /// <summary>
    /// Stand-in for real hearing: a flat proximity check. Milestone 6's noise/stealth system
    /// (walking = quiet, sprinting/jumping/landing = noticeable) will replace this with an
    /// actual noise-level/radius check — CatAI's usage (CanHearPlayer) shouldn't need to change
    /// when that happens, just this method's internals.
    /// </summary>
    public class CatHearing : MonoBehaviour
    {
        [SerializeField] private float hearingRadius = 1f;

        private Transform player;

        private void Awake()
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                player = playerGO.transform;
            }
        }

        public bool CanHearPlayer(out Vector3 playerPosition)
        {
            playerPosition = default;
            if (player == null)
            {
                return false;
            }

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > hearingRadius)
            {
                return false;
            }

            playerPosition = player.position;
            return true;
        }
    }
}
