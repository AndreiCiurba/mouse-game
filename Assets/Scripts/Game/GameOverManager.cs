using MouseGame.Player;
using MouseGame.UI;
using UnityEngine;

namespace MouseGame.Game
{
    /// <summary>
    /// Handles the "player caught" lose-state: freezes player movement/look and shows a
    /// Game Over message. The lose-state mirror of ObjectiveManager's win-state.
    /// </summary>
    public class GameOverManager : MonoBehaviour
    {
        [SerializeField] private GameOverUI gameOverUI;
        [SerializeField] private PlayerMotor playerMotor;
        [SerializeField] private PlayerLook playerLook;

        public bool IsGameOver { get; private set; }

        public void PlayerCaught()
        {
            if (IsGameOver)
            {
                return;
            }

            IsGameOver = true;

            if (playerMotor != null)
            {
                playerMotor.enabled = false;
            }

            if (playerLook != null)
            {
                playerLook.enabled = false;
            }

            if (gameOverUI != null)
            {
                gameOverUI.ShowGameOverMessage();
            }

            Debug.Log("Game Over: caught by the cat!");
        }
    }
}
