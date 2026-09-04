using UnityEditor;
using UnityEngine;

namespace MouseGame.EditorTools
{
    /// <summary>
    /// One-click Milestone 2 wiring (traversal, no custom climb script): builds a small
    /// staircase out of plain Cube primitives. Each tread rises less than
    /// CharacterController.stepOffset (set by MvpSceneBuilder), so Unity's built-in movement
    /// collision walks the player up them automatically — no Climbable/PlayerClimb needed.
    /// Run "Mouse Game -> Build MVP Scene" first. Safe to re-run.
    /// </summary>
    public static class StairsTestBuilder
    {
        // Mouse-scale (Player's CharacterController is height=0.2, radius=0.06, stepOffset=0.04
        // — see MvpSceneBuilder). These were originally sized for a human-scale character;
        // rescaled proportionally for Milestone 4.
        private const float StepWidth = 0.15f;
        private const float StepDepth = 0.06f;
        private const float StepThickness = 0.02f;
        private const float StepRise = 0.025f; // must stay below CharacterController.stepOffset
        private const int StepCount = 5;
        private const float LandingDepth = 0.2f;

        [MenuItem("Mouse Game/Build Stairs Test (Milestone 2)")]
        private static void BuildStairsTest()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Exit Play mode before building the stairs test.");
                return;
            }

            if (GameObject.Find("Player") == null)
            {
                Debug.LogError("No 'Player' found. Run 'Mouse Game -> Build MVP Scene' first.");
                return;
            }

            Undo.SetCurrentGroupName("Build Stairs Test");
            int undoGroup = Undo.GetCurrentGroup();

            float baseX = -1.5f;
            float baseZ = 0f;
            float z = baseZ;
            float lastTopSurface = 0f;

            for (int i = 1; i <= StepCount; i++)
            {
                float topSurface = i * StepRise;
                BuildTread($"Stair{i:00}", new Vector3(baseX, topSurface - StepThickness * 0.5f, z),
                    new Vector3(StepWidth, StepThickness, StepDepth));
                z += StepDepth;
                lastTopSurface = topSurface;
            }

            // Landing: the flat surface the stairs lead onto.
            float landingZ = z + LandingDepth * 0.5f - StepDepth * 0.5f;
            BuildTread("StairLanding", new Vector3(baseX, lastTopSurface - StepThickness * 0.5f, landingZ),
                new Vector3(StepWidth, StepThickness, LandingDepth));

            Undo.CollapseUndoOperations(undoGroup);
            MvpSceneBuilder.SaveActiveScene();

            Debug.Log($"Stairs built and saved near ({baseX}, *, {baseZ}..{landingZ:F1}): {StepCount} steps " +
                      $"rising {StepRise:F2}m each to {lastTopSurface:F2}m, then a landing. Walk into them — " +
                      "no button needed, CharacterController's step offset climbs them as you walk. Drag " +
                      "the whole group into your room if it lands outside your walls.");
        }

        private static void BuildTread(string name, Vector3 position, Vector3 scale)
        {
            GameObject tread = GameObject.Find(name);
            if (tread == null)
            {
                tread = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tread.name = name;
                Undo.RegisterCreatedObjectUndo(tread, "Build Stairs Test");
            }

            tread.transform.position = position;
            tread.transform.localScale = scale;
        }
    }
}
