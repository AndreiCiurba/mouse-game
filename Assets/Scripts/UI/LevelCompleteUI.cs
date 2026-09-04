using UnityEngine;
using UnityEngine.UI;

namespace MouseGame.UI
{
    /// <summary>
    /// Shows a simple "You escaped!" message. The win-state mirror of GameOverUI.
    /// </summary>
    public class LevelCompleteUI : MonoBehaviour
    {
        [SerializeField] private GameObject messagePanel;
        [SerializeField] private Text messageText;
        [SerializeField] private string message = "You escaped! Level Complete!";

        private void Awake()
        {
            if (messagePanel != null)
            {
                messagePanel.SetActive(false);
            }
        }

        public void ShowWinMessage()
        {
            if (messageText != null)
            {
                messageText.text = message;
            }

            if (messagePanel != null)
            {
                messagePanel.SetActive(true);
            }
        }
    }
}
