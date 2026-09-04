using UnityEngine;
using UnityEngine.EventSystems;

namespace MouseGame.Input
{
    /// <summary>
    /// Right-side drag-to-look area. Accumulates pointer drag delta each frame; PlayerInputReader
    /// consumes (and clears) it once per Update, the same way it reads mouse delta.
    /// </summary>
    public class TouchLookArea : MonoBehaviour, IDragHandler
    {
        private Vector2 accumulatedDelta;

        public void OnDrag(PointerEventData eventData)
        {
            accumulatedDelta += eventData.delta;
        }

        /// <summary>Returns the accumulated drag delta since the last call, then clears it.</summary>
        public Vector2 ConsumeDelta()
        {
            Vector2 delta = accumulatedDelta;
            accumulatedDelta = Vector2.zero;
            return delta;
        }
    }
}
