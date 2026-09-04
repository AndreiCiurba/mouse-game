using UnityEditor;
using UnityEngine;

namespace MouseGame.EditorTools
{
    /// <summary>
    /// Assembles a simple low-poly cat shape out of Unity primitives (body, head, ears, eyes,
    /// nose, tail), the same "primitives until Blender/gameplay works" approach as
    /// MouseModelBuilder. Proportions assume a NavMeshAgent-style pivot at ground/feet level
    /// (baseOffset 0), not centered like the Player's CharacterController.
    /// </summary>
    public static class CatModelBuilder
    {
        public static void BuildCatModel(Transform catTransform)
        {
            Transform existing = catTransform.Find("CatModel");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject root = new GameObject("CatModel");
            Undo.RegisterCreatedObjectUndo(root, "Build Cat AI");
            root.transform.SetParent(catTransform, false);

            BuildPart(root.transform, "Body", PrimitiveType.Sphere,
                new Vector3(0f, 0.12f, 0f), new Vector3(0.14f, 0.12f, 0.32f), Quaternion.identity);

            BuildPart(root.transform, "Head", PrimitiveType.Sphere,
                new Vector3(0f, 0.20f, 0.17f), new Vector3(0.13f, 0.12f, 0.13f), Quaternion.identity);

            BuildPart(root.transform, "Ear_L", PrimitiveType.Sphere,
                new Vector3(-0.045f, 0.27f, 0.17f), new Vector3(0.04f, 0.05f, 0.02f), Quaternion.identity);
            BuildPart(root.transform, "Ear_R", PrimitiveType.Sphere,
                new Vector3(0.045f, 0.27f, 0.17f), new Vector3(0.04f, 0.05f, 0.02f), Quaternion.identity);

            BuildPart(root.transform, "Nose", PrimitiveType.Sphere,
                new Vector3(0f, 0.18f, 0.235f), Vector3.one * 0.025f, Quaternion.identity);

            BuildPart(root.transform, "Eye_L", PrimitiveType.Sphere,
                new Vector3(-0.05f, 0.21f, 0.22f), Vector3.one * 0.018f, Quaternion.identity);
            BuildPart(root.transform, "Eye_R", PrimitiveType.Sphere,
                new Vector3(0.05f, 0.21f, 0.22f), Vector3.one * 0.018f, Quaternion.identity);

            // Capsule's long axis is local Y by default; rotate it onto Z so it trails backward.
            BuildPart(root.transform, "Tail", PrimitiveType.Capsule,
                new Vector3(0f, 0.14f, -0.20f), new Vector3(0.03f, 0.16f, 0.03f),
                Quaternion.Euler(80f, 0f, 0f));
        }

        private static void BuildPart(Transform parent, string name, PrimitiveType type,
            Vector3 localPosition, Vector3 localScale, Quaternion localRotation)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            Undo.RegisterCreatedObjectUndo(part, "Build Cat AI");
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;

            // Visual-only parts; the NavMeshAgent/CharacterController on the root already owns
            // movement, and these tiny colliders would just add noise to physics queries.
            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }
    }
}
