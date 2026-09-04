using UnityEngine;
using UnityEngine.SceneManagement;

namespace MouseGame.UI
{
    /// <summary>Reloads the current scene. Wire Restart() to a UI Button's OnClick.</summary>
    public class RestartButton : MonoBehaviour
    {
        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
