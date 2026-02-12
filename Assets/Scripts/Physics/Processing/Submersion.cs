using UnityEngine.Rendering.HighDefinition;
using Unity.Collections;
using UnityEngine;
using Sim.Utils;

namespace Sim.Physics.Processing {
    public class Submersion : MonoBehaviour {
        [ReadOnly] public Submerged submerged;

        [SerializeField, Tooltip("HDRP water surface used for height querying")]
        private WaterSurface waterSurface = null;
        [SerializeField, Tooltip("A simplified mesh for physics calculations")]
        private Mesh simplifiedMesh;
        [SerializeField, Tooltip("Side length of square water surface approximation patch. Much be large enough to fit entire vessel")]
        private float patchSize = 10;
        [SerializeField, Tooltip("Higher number gives a better approximation of water surface")]
        private int patchResolution = 4;

        private Patch patch;
        //public bool drawWaterLine;

        [SerializeField] private bool drawPatch;
        [SerializeField] private bool drawSubmerged;
        private float displacedVolume;

        private void Awake() {
            Vector3 gridOrigin = new(-patchSize / 2, 0, patchSize / 2);
            patch = new Patch(waterSurface, patchSize, patchResolution, gridOrigin);
            submerged = new Submerged(simplifiedMesh, debug: true); // set up submersion by providing the simplified hull mesh
            patch.Update(transform); // updates the patch to follow the boat and queried water height
        }

        private void FixedUpdate() {
            patch.Update(transform); // updates the patch to follow the boat and queried water height
            submerged.Update(patch, transform);

            displacedVolume = submerged.data.volume;

            if (drawPatch) DebugPatch();
            if (drawSubmerged) DebugSubmerged();
        }


        private void DebugPatch() {
            int[] tris = patch.patchTriangles;
            Vector3[] verts = patch.patchVertices;
            for (var i = 0; i < tris.Length; i += 3) {
                Vector3[] tri = new Vector3[] { verts[tris[i]], verts[tris[i + 1]], verts[tris[i + 2]] };
                CommonUtils.DebugDrawTriangle(tri, Color.red);
            }
        }


        private void DebugSubmerged() {
            int[] tris = submerged.data.triangles;
            Vector3[] verts = submerged.data.vertices;

            for (int i = 0; i < submerged.data.maxTriangleIndex - 2; i += 3) {
                Vector3[] tri = new Vector3[]
                {
                transform.TransformPoint(verts[tris[i]]),
                transform.TransformPoint(verts[tris[i + 1]]),
                transform.TransformPoint(verts[tris[i + 2]])
                };

                CommonUtils.DebugDrawTriangle(tri, Color.green);
            }
        }

        private void OnDestroy() {
            patch.Dispose();
        }
    }
}
