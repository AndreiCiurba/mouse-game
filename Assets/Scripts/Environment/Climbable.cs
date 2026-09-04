using UnityEngine;

namespace MouseGame.Environment
{
    /// <summary>
    /// Marker component — any collider carrying this can be mantled by PlayerClimb.
    /// No behaviour of its own; presence on the object is the whole contract (the README's
    /// "simple Climbable component/tag" approach). Drop this on boxes, chairs, tables, etc.
    /// </summary>
    public class Climbable : MonoBehaviour
    {
    }
}
