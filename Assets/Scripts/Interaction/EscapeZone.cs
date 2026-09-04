using MouseGame.Game;
using UnityEngine;

namespace MouseGame.Interaction
{
    /// <summary>
    /// Trigger volume the player must return to after completing the objective to win the level
    /// ("steal the cheese and escape" — reaching the cheese alone isn't enough). No-op if the
    /// objective isn't done yet, so walking through it early does nothing.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class EscapeZone : MonoBehaviour
    {
        [SerializeField] private ObjectiveManager objectiveManager;
        [SerializeField] private LevelCompleteManager levelCompleteManager;
        [SerializeField] private string playerTag = "Player";

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag))
            {
                return;
            }

            if (objectiveManager != null && !objectiveManager.ObjectiveComplete)
            {
                return; // haven't grabbed the cheese yet
            }

            if (levelCompleteManager != null)
            {
                levelCompleteManager.PlayerEscaped();
            }
        }
    }
}
