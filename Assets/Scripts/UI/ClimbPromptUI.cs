using UnityEngine;

namespace MouseGame.UI
{
    /// <summary>
    /// Toggles a simple "Press E to climb" prompt. Driven every frame by PlayerClimb's
    /// detection state; Show/Hide are cheap no-ops when already in that state.
    /// </summary>
    public class ClimbPromptUI : MonoBehaviour
    {
        [SerializeField] private GameObject promptRoot;

        public void ShowPrompt()
        {
            if (promptRoot != null && !promptRoot.activeSelf)
            {
                promptRoot.SetActive(true);
            }
        }

        public void HidePrompt()
        {
            if (promptRoot != null && promptRoot.activeSelf)
            {
                promptRoot.SetActive(false);
            }
        }
    }
}
