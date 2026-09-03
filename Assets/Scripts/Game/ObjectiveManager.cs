using MouseGame.UI;
using UnityEngine;

namespace MouseGame.Game
{
    /// <summary>
    /// Tracks the single MVP objective ("find the item"). Deliberately minimal — this is the
    /// seed of the README's ObjectiveSystem; multi-objective/inventory logic can grow here later
    /// without CollectibleItem or ObjectiveUI needing to change.
    /// </summary>
    public class ObjectiveManager : MonoBehaviour
    {
        [SerializeField] private ObjectiveUI objectiveUI;

        public bool ObjectiveComplete { get; private set; }

        /// <summary>Called by CollectibleItem when the player picks it up.</summary>
        public void ItemCollected()
        {
            if (ObjectiveComplete)
            {
                return;
            }

            ObjectiveComplete = true;

            if (objectiveUI != null)
            {
                objectiveUI.ShowFoundMessage();
            }

            Debug.Log("Objective complete: item found!");
        }
    }
}
