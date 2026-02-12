using UnityEngine;

namespace Sim.Utils {
    public class VisualizeCenterOfMass : MonoBehaviour {
        [SerializeField, Range(0.01f, 1.0f)] float visualRadius = 0.05f;
        [SerializeField] bool drawGizmo = true;
        private IPhysicsBody body;

        private void OnDrawGizmos() {
            if (!drawGizmo) return;

            if (body == null) {
                var rb = GetComponent<Rigidbody>();
                var ab = GetComponent<ArticulationBody>();

                if (rb != null) body = new RigidbodyAdapter(rb);
                else if (ab != null) body = new ArticulationBodyAdapter(ab);
                else throw new MissingComponentException($"{name} requires a Rigidbody or ArticulationBody!");
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.TransformPoint(body.centerOfMass), visualRadius);
        }
    }
}
