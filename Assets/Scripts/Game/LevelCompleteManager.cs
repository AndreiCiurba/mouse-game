using MouseGame.Player;
using MouseGame.UI;
using UnityEngine;

namespace MouseGame.Game
{
    /// <summary>
    /// Handles the win-state: player returned to the EscapeZone after completing the objective.
    /// Freezes player movement/look and shows a win message — the win-state mirror of
    /// GameOverManager's lose-state.
    /// </summary>
    public class LevelCompleteManager : MonoBehaviour
    {
        [SerializeField] private LevelCompleteUI levelCompleteUI;
        [SerializeField] private PlayerMotor playerMotor;
        [SerializeField] private PlayerLook playerLook;

        public bool HasEscaped { get; private set; }

        public void PlayerEscaped()
        {
            if (HasEscaped)
            {
                return;
            }

            HasEscaped = true;

            if (playerMotor != null)
            {
                playerMotor.enabled = false;
            }

            if (playerLook != null)
            {
                playerLook.enabled = false;
            }

            if (levelCompleteUI != null)
            {
                levelCompleteUI.ShowWinMessage();
            }

            Debug.Log("Level complete: escaped with the cheese!");
        }
    }
}
