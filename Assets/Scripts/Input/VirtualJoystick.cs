using UnityEngine;
using UnityEngine.EventSystems;

namespace MouseGame.Input
{
    /// <summary>
    /// Fixed-position on-screen joystick: drag the handle within a radius of the background to
    /// produce a -1..1 Value per axis. Works identically for a mouse (Editor/Device Simulator)
    /// or a real touch, since it only uses Unity's pointer event interfaces.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 60f;

        /// <summary>Current stick position, each axis in [-1, 1].</summary>
        public Vector2 Value { get; private set; }

        public void OnPointerDown(PointerEventData eventData) => UpdateHandle(eventData);

        public void OnDrag(PointerEventData eventData) => UpdateHandle(eventData);

        public void OnPointerUp(PointerEventData eventData)
        {
            Value = Vector2.zero;
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }

        private void UpdateHandle(PointerEventData eventData)
        {
            if (background == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

            Vector2 clamped = Vector2.ClampMagnitude(localPoint, handleRange);
            Value = clamped / handleRange;

            if (handle != null)
            {
                handle.anchoredPosition = clamped;
            }
        }
    }
}
