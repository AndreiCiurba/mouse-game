using UnityEngine;
using UnityEngine.EventSystems;

namespace MouseGame.Input
{
    /// <summary>
    /// A held mobile button (e.g. Sprint): true for as long as it's pressed down.
    /// </summary>
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool IsHeld { get; private set; }

        public void OnPointerDown(PointerEventData eventData) => IsHeld = true;

        public void OnPointerUp(PointerEventData eventData) => IsHeld = false;

        // Release if the finger/cursor drags off the button without a proper pointer-up on it —
        // otherwise sprint could get stuck "on" after a sloppy drag.
        public void OnPointerExit(PointerEventData eventData) => IsHeld = false;
    }
}
