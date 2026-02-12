using UnityEngine;

// Utility script for creating an offset child rigidbody force (not useful for articulation bodies which solve based on child position)
namespace Sim.Physics.Misc {
    public class Ballast : MonoBehaviour {
        [SerializeField] private float mass = 30.0f;
        private Rigidbody rb;
        const float g = 9.8067f;

        private void Start() {
            rb = GetComponentInParent<Rigidbody>();
        }

        private void FixedUpdate() {
            rb.AddForceAtPosition(Vector3.down * g * mass, transform.position);
        }
    }
}
