using MouseGame.Interaction;
using MouseGame.Player;
using UnityEditor;
using UnityEngine;

namespace MouseGame.EditorTools
{
    /// <summary>
    /// One-click wiring for the last unfinished piece of Milestone 6: scatters a few small
    /// "knockable" clutter props around the room and adds PropKnocker to the Player so bumping
    /// into one emits a loud noise (CatHearing already reacts to any noise radius — no changes
    /// needed there). Run after "Mouse Game -> Build MVP Scene". Safe to re-run.
    /// </summary>
    public static class PropsBuilder
    {
        private static readonly Vector3[] Positions =
        {
            new Vector3(1.5f, 0f, -0.5f),
            new Vector3(0f, 0f, -2f),
            new Vector3(-0.5f, 0f, 1f),
        };

        private const float PropRadius = 0.03f;
        private const float PropHeight = 0.08f;

        [MenuItem("Mouse Game/Build Knockable Props")]
        private static void BuildProps()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Exit Play mode before building props.");
                return;
            }

            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("No 'Player' found. Run 'Mouse Game -> Build MVP Scene' first.");
                return;
            }

            Undo.SetCurrentGroupName("Build Knockable Props");
            int undoGroup = Undo.GetCurrentGroup();

            GetOrAddComponent<PropKnocker>(player);

            for (int i = 0; i < Positions.Length; i++)
            {
                BuildProp($"Prop{i + 1:00}", new Vector3(Positions[i].x, PropHeight * 0.5f, Positions[i].z));
            }

            Undo.CollapseUndoOperations(undoGroup);
            MvpSceneBuilder.SaveActiveScene();

            Debug.Log($"{Positions.Length} knockable props built and saved; Player can now knock " +
                      "them for a loud noise pulse the cat can hear (PropKnocker + KnockableProp).");
        }

        private static void BuildProp(string name, Vector3 position)
        {
            GameObject prop = GameObject.Find(name);
            if (prop == null)
            {
                prop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                prop.name = name;
                Undo.RegisterCreatedObjectUndo(prop, "Build Knockable Props");
            }

            prop.transform.position = position;
            prop.transform.localScale = new Vector3(PropRadius * 2f, PropHeight * 0.5f, PropRadius * 2f);

            GetOrAddComponent<KnockableProp>(prop);
        }

        private static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = Undo.AddComponent<T>(go);
            }
            return component;
        }
    }
}
