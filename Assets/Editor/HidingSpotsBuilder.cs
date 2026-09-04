using UnityEditor;
using UnityEngine;

namespace MouseGame.EditorTools
{
    /// <summary>
    /// Scatters a handful of solid cover blocks around the test room. CatVision's line-of-sight
    /// raycast already treats any solid collider as an obstruction (obstacleMask defaults to
    /// everything), so these need no special script — standing behind one just works.
    /// Taller than CatAIBuilder's cat eye height (0.20) so they fully block vision, not just
    /// duck it. Run after "Mouse Game -> Build Test Room". Safe to re-run.
    /// </summary>
    public static class HidingSpotsBuilder
    {
        private const float Width = 0.18f;
        private const float Height = 0.26f;
        private const float Depth = 0.18f;

        private static readonly Vector2[] Positions =
        {
            new Vector2(1f, 0.5f),
            new Vector2(-1f, -1.5f),
            new Vector2(0.5f, -2.5f),
            new Vector2(-2f, 1.5f),
            new Vector2(2.5f, 1f),
        };

        [MenuItem("Mouse Game/Build Hiding Spots")]
        private static void BuildHidingSpots()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Exit Play mode before building hiding spots.");
                return;
            }

            Undo.SetCurrentGroupName("Build Hiding Spots");
            int undoGroup = Undo.GetCurrentGroup();

            for (int i = 0; i < Positions.Length; i++)
            {
                BuildCover($"HidingSpot{i + 1:00}", new Vector3(Positions[i].x, Height * 0.5f, Positions[i].y));
            }

            Undo.CollapseUndoOperations(undoGroup);
            MvpSceneBuilder.SaveActiveScene();

            Debug.Log($"{Positions.Length} hiding spots built and saved. Duck behind one (between " +
                      "yourself and the cat) to break its line of sight — vision is blocked by any " +
                      "solid collider, so this works with no extra wiring.");
        }

        private static void BuildCover(string name, Vector3 position)
        {
            GameObject cover = GameObject.Find(name);
            if (cover == null)
            {
                cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cover.name = name;
                Undo.RegisterCreatedObjectUndo(cover, "Build Hiding Spots");
            }

            cover.transform.position = position;
            cover.transform.localScale = new Vector3(Width, Height, Depth);
        }
    }
}
