using UnityEngine;

namespace Sim.Sensors.Vision {
    public class BoundingBox3D : MonoBehaviour {
        [SerializeField, Range(0.01f, 0.1f)] float visualRadius = 0.025f;
        [SerializeField] bool drawGizmo = true;

        private void OnDrawGizmos() {
            if (!drawGizmo) return;

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Transform root = transform;

            bool initialized = false;
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;

            foreach (var r in renderers) {
                Bounds b = r.bounds;

                // Get 8 world corners of this renderer's bounds
                Vector3 c = b.center;
                Vector3 e = b.extents;

                Vector3[] corners = new Vector3[8]
                {
                    c + new Vector3(-e.x,-e.y,-e.z),
                    c + new Vector3(e.x,-e.y,-e.z),
                    c + new Vector3(-e.x,e.y,-e.z),
                    c + new Vector3(e.x,e.y,-e.z),
                    c + new Vector3(-e.x,-e.y,e.z),
                    c + new Vector3(e.x,-e.y,e.z),
                    c + new Vector3(-e.x,e.y,e.z),
                    c + new Vector3(e.x,e.y,e.z)
                };

                // Convert to parent local space
                foreach (var corner in corners) {
                    Vector3 local = root.InverseTransformPoint(corner);

                    if (!initialized) {
                        min = max = local;
                        initialized = true;
                    } else {
                        min = Vector3.Min(min, local);
                        max = Vector3.Max(max, local);
                    }
                }
            }

            DrawBoundingBox(min, max);
        }

        void DrawBoundingBox(Vector3 min, Vector3 max) {
            Vector3 c = (min + max) * 0.5f;
            Vector3 e = (max - min) * 0.5f;

            Vector3[] localCorners = new Vector3[8]
            {
                c + new Vector3(-e.x,-e.y,-e.z),
                c + new Vector3(e.x,-e.y,-e.z),
                c + new Vector3(-e.x,e.y,-e.z),
                c + new Vector3(e.x,e.y,-e.z),
                c + new Vector3(-e.x,-e.y,e.z),
                c + new Vector3(e.x,-e.y,e.z),
                c + new Vector3(-e.x,e.y,e.z),
                c + new Vector3(e.x,e.y,e.z)
            };

            Vector3[] worldCorners = new Vector3[8];
            for (int i = 0; i < 8; i++)
                worldCorners[i] = transform.TransformPoint(localCorners[i]);

            Gizmos.color = Color.red;

            foreach (var corner in worldCorners)
                Gizmos.DrawSphere(corner, visualRadius);

            int[,] edges = {
                {0,1},{1,3},{3,2},{2,0},
                {4,5},{5,7},{7,6},{6,4},
                {0,4},{1,5},{2,6},{3,7}
            };

            for (int i = 0; i < edges.GetLength(0); i++)
                Gizmos.DrawLine(
                    worldCorners[edges[i, 0]],
                    worldCorners[edges[i, 1]]
                );
        }
    }
}
