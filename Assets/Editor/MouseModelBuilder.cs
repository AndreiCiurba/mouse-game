using UnityEditor;
using UnityEngine;

namespace MouseGame.EditorTools
{
    /// <summary>
    /// Assembles a simple low-poly mouse shape out of Unity primitives (body, head, ears, nose,
    /// eyes, tail) as a placeholder for the real Blender-modeled mouse the README calls for.
    /// Nobody here can run Blender — this keeps the "primitives until gameplay works" approach
    /// consistent and gives the real model an obvious, single place to slot in later (replace
    /// this method's contents with an imported prefab's parts, same parent/scale contract).
    ///
    /// All proportions are tuned for a Player with CharacterController height=0.2, radius=0.06,
    /// center=zero (see MvpSceneBuilder) — a genuinely small mouse, not a scaled-down human
    /// capsule. Sits under a "MouseModel" child so it's easy to find/replace as one unit.
    /// </summary>
    public static class MouseModelBuilder
    {
        public static void BuildMouseModel(Transform playerTransform)
        {
            Transform existing = playerTransform.Find("MouseModel");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject root = new GameObject("MouseModel");
            Undo.RegisterCreatedObjectUndo(root, "Build MVP Scene");
            root.transform.SetParent(playerTransform, false);

            BuildPart(root.transform, "Body", PrimitiveType.Sphere,
                new Vector3(0f, -0.02f, 0f), new Vector3(0.1f, 0.08f, 0.16f), Quaternion.identity);

            BuildPart(root.transform, "Head", PrimitiveType.Sphere,
                new Vector3(0f, 0f, 0.09f), new Vector3(0.07f, 0.07f, 0.07f), Quaternion.identity);

            BuildPart(root.transform, "Ear_L", PrimitiveType.Sphere,
                new Vector3(-0.025f, 0.045f, 0.085f), new Vector3(0.025f, 0.03f, 0.012f), Quaternion.identity);
            BuildPart(root.transform, "Ear_R", PrimitiveType.Sphere,
                new Vector3(0.025f, 0.045f, 0.085f), new Vector3(0.025f, 0.03f, 0.012f), Quaternion.identity);

            BuildPart(root.transform, "Nose", PrimitiveType.Sphere,
                new Vector3(0f, -0.01f, 0.125f), Vector3.one * 0.018f, Quaternion.identity);

            BuildPart(root.transform, "Eye_L", PrimitiveType.Sphere,
                new Vector3(-0.028f, 0.01f, 0.115f), Vector3.one * 0.012f, Quaternion.identity);
            BuildPart(root.transform, "Eye_R", PrimitiveType.Sphere,
                new Vector3(0.028f, 0.01f, 0.115f), Vector3.one * 0.012f, Quaternion.identity);

            // Capsule's long axis is local Y by default; rotate it onto Z so it trails backward.
            BuildPart(root.transform, "Tail", PrimitiveType.Capsule,
                new Vector3(0f, -0.02f, -0.12f), new Vector3(0.02f, 0.045f, 0.02f),
                Quaternion.Euler(90f, 0f, 0f));
        }

        private static void BuildPart(Transform parent, string name, PrimitiveType type,
            Vector3 localPosition, Vector3 localScale, Quaternion localRotation)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            Undo.RegisterCreatedObjectUndo(part, "Build MVP Scene");
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;

            // These are visual-only parts; the CharacterController on the Player root already
            // owns collision. Leaving each primitive's auto-added collider in place would stack
            // several small colliders on top of that for no reason.
            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }
    }
}
