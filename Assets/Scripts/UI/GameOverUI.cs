using UnityEngine;
using UnityEngine.UI;

namespace MouseGame.UI
{
    /// <summary>
    /// Shows a simple "Caught! Game Over" message. The lose-state mirror of ObjectiveUI.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject messagePanel;
        [SerializeField] private Text messageText;
        [SerializeField] private string message = "Caught! Game Over";

        private void Awake()
        {
            if (messagePanel != null)
            {
                messagePanel.SetActive(false);
            }
        }

        public void ShowGameOverMessage()
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
