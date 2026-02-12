using UnityEngine.Rendering.HighDefinition;
using UnityEngine;
using Sim.Physics.Processing;
using Sim.Utils;

namespace Sim.Physics.Water.Dynamics {
    [RequireComponent(typeof(Submersion))]
    public class Current : MonoBehaviour {
        [SerializeField] private WaterSurface waterSurface;
        [SerializeField] private bool debugCurrent = false;
        [SerializeField] private float Cd = 1.0f;
        private Submerged submerged;
        private IPhysicsBody body;

        void OnValidate() {
            if (GetComponent<Rigidbody>() == null && GetComponent<ArticulationBody>() == null)
                Debug.LogWarning($"{name} should have either a Rigidbody or an ArticulationBody attached.");
        }

        void Awake() {
            var rb = GetComponent<Rigidbody>();
            var ab = GetComponent<ArticulationBody>();

            if (rb != null) body = new RigidbodyAdapter(rb);
            else if (ab != null) body = new ArticulationBodyAdapter(ab);
            else throw new MissingComponentException($"{name} requires a Rigidbody or ArticulationBody!");
        }

        private void Start() { submerged = GetComponent<Submersion>().submerged; }

        private void FixedUpdate() { ApplyCurrent(); }

        private void ApplyCurrent() {
            if (submerged.data == null) return;

            Vector3 bodyVel = body.linearVelocity;
            Vector3 bodyOmega = body.angularVelocity;
            Vector3 bodyPos = body.position;

            for (int i = 0; i < submerged.data.maxTriangleIndex / 3; i++) {
                Vector3 faceCenter = submerged.data.faceCentersWorld[i];
                Vector3 pointVel = bodyVel + Vector3.Cross(bodyOmega, faceCenter - bodyPos);
                Vector3 waterVel = GetCurrentAtPoint(faceCenter);
                Vector3 relVel = pointVel - waterVel;

                float rho = Constants.waterDensity;
                float faceArea = submerged.data.triangleAreas[i];

                Vector3 force = -0.5f * rho * Cd * faceArea * relVel.magnitude * relVel;
                force.y = 0.0f;

                body.AddForceAtPosition(force, faceCenter);

                if (debugCurrent) Debug.DrawRay(faceCenter, force, Color.cyan);
            }
        }

        private Vector3 GetCurrentAtPoint(Vector3 point) { return WaterUtils.Search(waterSurface, point).currentDirectionWS * waterSurface.largeCurrentSpeedValue; }
    }
}
