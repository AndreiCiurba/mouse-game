using UnityEngine;
using UnityEngine.UI;

namespace MouseGame.UI
{
    /// <summary>
    /// Shows a simple on-screen "Found it!" message when the MVP objective completes.
    /// </summary>
    public class ObjectiveUI : MonoBehaviour
    {
        [SerializeField] private GameObject foundMessagePanel;
        [SerializeField] private Text foundMessageText;
        [SerializeField] private string message = "Found it!";
        [Tooltip("Seconds to show the message before auto-hiding. Set to 0 to keep it on screen.")]
        [SerializeField] private float autoHideSeconds = 3f;

        private void Awake()
        {
            if (foundMessagePanel != null)
            {
                foundMessagePanel.SetActive(false);
            }
        }

        public void ShowFoundMessage()
        {
            if (foundMessageText != null)
            {
                foundMessageText.text = message;
            }

            if (foundMessagePanel != null)
            {
                foundMessagePanel.SetActive(true);
            }

            if (autoHideSeconds > 0f)
            {
                CancelInvoke(nameof(Hide));
                Invoke(nameof(Hide), autoHideSeconds);
            }
        }

        private void Hide()
        {
            if (foundMessagePanel != null)
            {
                foundMessagePanel.SetActive(false);
            }
        }
    }
}
