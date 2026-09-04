using UnityEditor;
using UnityEngine;

namespace MouseGame.EditorTools
{
    /// <summary>
    /// One-click test room: a solid floor plus four walls, sized to comfortably enclose where
    /// MvpSceneBuilder/StairsTestBuilder place the Player, Cheese, and stairs (roughly x: -3..3,
    /// z: -2..4). Run any time, independent of the other build tools. Safe to re-run.
    /// </summary>
    public static class RoomTestBuilder
    {
        private const float FloorHalfSize = 4f;
        private const float FloorThickness = 0.2f;
        private const float WallHeight = 3f;
        private const float WallThickness = 0.2f;

        [MenuItem("Mouse Game/Build Test Room")]
        private static void BuildTestRoom()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Exit Play mode before building the test room.");
                return;
            }

            Undo.SetCurrentGroupName("Build Test Room");
            int undoGroup = Undo.GetCurrentGroup();

            BuildBlock("Floor", new Vector3(0f, -FloorThickness * 0.5f, 0f),
                new Vector3(FloorHalfSize * 2f, FloorThickness, FloorHalfSize * 2f));

            BuildBlock("Wall_North", new Vector3(0f, WallHeight * 0.5f, FloorHalfSize),
                new Vector3(FloorHalfSize * 2f, WallHeight, WallThickness));
            BuildBlock("Wall_South", new Vector3(0f, WallHeight * 0.5f, -FloorHalfSize),
                new Vector3(FloorHalfSize * 2f, WallHeight, WallThickness));
            BuildBlock("Wall_East", new Vector3(FloorHalfSize, WallHeight * 0.5f, 0f),
                new Vector3(WallThickness, WallHeight, FloorHalfSize * 2f));
            BuildBlock("Wall_West", new Vector3(-FloorHalfSize, WallHeight * 0.5f, 0f),
                new Vector3(WallThickness, WallHeight, FloorHalfSize * 2f));

            Undo.CollapseUndoOperations(undoGroup);
            MvpSceneBuilder.SaveActiveScene();

            Debug.Log($"Test room built and saved: floor + 4 walls spanning roughly " +
                      $"({-FloorHalfSize}..{FloorHalfSize}, *, {-FloorHalfSize}..{FloorHalfSize}) — " +
                      "comfortably encloses the Player spawn, Cheese, and stairs from the other build tools.");
        }

        private static void BuildBlock(string name, Vector3 position, Vector3 scale)
        {
            GameObject block = GameObject.Find(name);
            if (block == null)
            {
                block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.name = name;
                Undo.RegisterCreatedObjectUndo(block, "Build Test Room");
            }

            block.transform.position = position;
            block.transform.localScale = scale;
        }
    }
}
