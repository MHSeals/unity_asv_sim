using System;
using UnityEngine;

namespace Sim.Physics.Water.Dynamics {
    [RequireComponent(typeof(Rigidbody))]
    public class SimpleDrag : MonoBehaviour {
        public Vector3 linearCoefficients = new(1.0f, 1.0f, 1.0f);
        public Vector3 quadraticCoefficients = new(1.0f, 1.0f, 1.0f);
        public Vector3 cubicCoefficients = new(1.0f, 1.0f, 1.0f);

        public Vector3 angularLinearCoefficients = new(1.0f, 1.0f, 1.0f);
        public Vector3 angularQuadraticCoefficients = new(1.0f, 1.0f, 1.0f);
        public Vector3 angularCubicCoefficients = new(1.0f, 1.0f, 1.0f);

        private Rigidbody rb;

        private void Start() {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate() {
            ApplyDrag();
        }

        private void ApplyDrag() {
            // Translational drag
            Vector3 velocity = transform.InverseTransformDirection(rb.linearVelocity);
            Vector3 dragForce = CalculateDragForce(velocity, linearCoefficients, quadraticCoefficients, cubicCoefficients);
            rb.AddRelativeForce(-dragForce, ForceMode.Force);

            // Rotational drag
            Vector3 angularVelocity = transform.InverseTransformDirection(rb.angularVelocity);
            Vector3 angularDragTorque = CalculateDragForce(angularVelocity, angularLinearCoefficients, angularQuadraticCoefficients, angularCubicCoefficients);
            rb.AddTorque(transform.TransformDirection(-angularDragTorque), ForceMode.Force);
        }

        Vector3 CalculateDragForce(Vector3 velocity, Vector3 linear, Vector3 quadratic, Vector3 cubic) {
            Vector3 force = Vector3.zero;
            force.x = CalculateDragForAxis(velocity.x, linear.x, quadratic.x, cubic.x);
            force.y = CalculateDragForAxis(velocity.y, linear.y, quadratic.y, cubic.y);
            force.z = CalculateDragForAxis(velocity.z, linear.z, quadratic.z, cubic.z);
            return force;
        }

        float CalculateDragForAxis(float speed, float linear, float quadratic, float cubic) {
            return linear * speed + quadratic * speed * Math.Abs(speed) + cubic * Mathf.Pow(speed, 3);
        }
    }
}
