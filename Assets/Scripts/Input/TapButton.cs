using UnityEngine;
using UnityEngine.EventSystems;

namespace MouseGame.Input
{
    /// <summary>
    /// A one-shot mobile button (e.g. Jump): fires on press, not release, for responsiveness.
    /// PlayerInputReader consumes (and clears) the pressed flag once per Update, the same way it
    /// reads a keyboard GetKeyDown.
    /// </summary>
    public class TapButton : MonoBehaviour, IPointerDownHandler
    {
        private bool pressed;

        public void OnPointerDown(PointerEventData eventData)
        {
            pressed = true;
        }

        /// <summary>Returns true exactly once per press, then clears until the next press.</summary>
        public bool ConsumePressed()
        {
            if (!pressed)
            {
                return false;
            }

            pressed = false;
            return true;
        }
    }
}
