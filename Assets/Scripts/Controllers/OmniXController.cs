using UnityEngine;
using Sim.Actuators.Motors;
using UnityEngine.InputSystem;

namespace Sim.Controllers {
    public class OmniXController : MonoBehaviour, IControllerBase {
        [SerializeField] private InputActionReference linearAction, angularAction;
        private Vector2 linearInput;
        private float angularInput;
        public ThrusterConfig config; // Assumes all thrusters have this configuration
        public Thruster frontLeft, frontRight, rearLeft, rearRight;
        public bool movementOverride = false;

        private void OnEnable() {
            linearAction.action.performed += OnLinearPerformed;
            linearAction.action.canceled += OnLinearCanceled;

            angularAction.action.performed += OnAngularPerformed;
            angularAction.action.canceled += OnAngularCanceled;

            linearAction.action.Enable();
            angularAction.action.Enable();
        }

        private void OnDisable() {
            linearAction.action.performed -= OnLinearPerformed;
            linearAction.action.canceled -= OnLinearCanceled;

            angularAction.action.performed -= OnAngularPerformed;
            angularAction.action.canceled -= OnAngularCanceled;

            linearAction.action.Disable();
            angularAction.action.Disable();
        }

        private void OnLinearPerformed(InputAction.CallbackContext ctx) {
            movementOverride = true;
            linearInput = ctx.ReadValue<Vector2>();
            Move();
        }

        private void OnLinearCanceled(InputAction.CallbackContext ctx) {
            movementOverride = false;
            linearInput = Vector2.zero;
            Move();
        }

        private void OnAngularPerformed(InputAction.CallbackContext ctx) {
            movementOverride = true;
            angularInput = ctx.ReadValue<float>();
            Move();
        }

        private void OnAngularCanceled(InputAction.CallbackContext ctx) {
            movementOverride = false;
            angularInput = 0;
            Move();
        }

        private void Move() {
            SetMotion(new Vector3(linearInput.x, linearInput.y, 0), new Vector3(0, 0, angularInput));
        }

        // TODO: More accurately model desired linear and angular velocity (not just full forward throttle/backward/angular)
        public void SetMotion(Vector3 linear, Vector3 angular) {
            // Debug.Log(linear.x + " " + linear.y + " " + angular.z);
            if (angular.z != 0) {
                // Only as good at generating torque as the magnitude of the cross product of the radius and force vectors
                float frd = angular.z * config.GetMaxCommand();
                float fld = -frd;

                frontRight.SetCommand(frd);
                rearLeft.SetCommand(frd);

                frontLeft.SetCommand(fld);
                rearRight.SetCommand(fld);
            }
            else {
                float cX = linear.x * config.GetMaxCommand();
                float cY = linear.y * config.GetMaxCommand();

                frontLeft.SetCommand(-cX - cY);
                frontRight.SetCommand(cX - cY);
                rearLeft.SetCommand(-cX + cY);
                rearRight.SetCommand(cX + cY);

                // Outward pointing thrusters
                // frontLeft.SetCommand(cX - cY);
                // frontRight.SetCommand(-cX - cY);
                // rearLeft.SetCommand(cX + cY);
                // rearRight.SetCommand(-cX + cY);
            }
        }
    }
}
