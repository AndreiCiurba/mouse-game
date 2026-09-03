using MouseGame.Game;
using UnityEngine;

namespace MouseGame.Interaction
{
    /// <summary>
    /// A pickup placeholder (stand-in for the cheese/ring model). Requires a trigger Collider
    /// on this GameObject and a "Player" tag on the object that touches it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CollectibleItem : MonoBehaviour
    {
        [SerializeField] private ObjectiveManager objectiveManager;
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

            if (objectiveManager != null)
            {
                objectiveManager.ItemCollected();
            }

            gameObject.SetActive(false);
        }
    }
}
