using UnityEngine;
using Sim.Actuators.Motors;
using UnityEngine.InputSystem;

namespace Sim.Controllers {
    public class FullOmniController : MonoBehaviour, IControllerBase {

        [Header("Input Actions")]
        [SerializeField] private InputActionReference translationAction; // Vector3
        [SerializeField] private InputActionReference rotationAction;    // Vector2 (roll, yaw)

        private Vector3 translationInput;
        private Vector2 rotationInput;

        public ThrusterConfig config;

        [Header("Horizontal Corner Thrusters")]
        public Thruster frontLeft;
        public Thruster frontRight;
        public Thruster rearLeft;
        public Thruster rearRight;

        [Header("Vertical Thrusters")]
        public Thruster verticalLeft;
        public Thruster verticalRight;

        public bool movementOverride = false;

        private void OnEnable() {
            translationAction.action.performed += OnTranslationPerformed;
            translationAction.action.canceled += OnTranslationCanceled;

            rotationAction.action.performed += OnRotationPerformed;
            rotationAction.action.canceled += OnRotationCanceled;

            translationAction.action.Enable();
            rotationAction.action.Enable();
        }

        private void OnDisable() {
            translationAction.action.performed -= OnTranslationPerformed;
            translationAction.action.canceled -= OnTranslationCanceled;

            rotationAction.action.performed -= OnRotationPerformed;
            rotationAction.action.canceled -= OnRotationCanceled;

            translationAction.action.Disable();
            rotationAction.action.Disable();
        }

        private void OnTranslationPerformed(InputAction.CallbackContext ctx) {
            movementOverride = true;
            translationInput = ctx.ReadValue<Vector3>();
            Move();
        }

        private void OnTranslationCanceled(InputAction.CallbackContext ctx) {
            translationInput = Vector3.zero;
            CheckMovementOverride();
            Move();
        }

        private void OnRotationPerformed(InputAction.CallbackContext ctx) {
            movementOverride = true;
            rotationInput = ctx.ReadValue<Vector2>();
            Move();
        }

        private void OnRotationCanceled(InputAction.CallbackContext ctx) {
            rotationInput = Vector2.zero;
            CheckMovementOverride();
            Move();
        }

        private void CheckMovementOverride() {
            movementOverride =
                translationInput != Vector3.zero ||
                rotationInput != Vector2.zero;
        }

        private void Move() {
            SetMotion(
                translationInput,
                new Vector2(rotationInput.x, rotationInput.y)
            );
        }

        public void SetMotion(Vector3 linear, Vector3 angular) {

            float max = config.GetMaxCommand();

            float x = linear.x * max;
            float y = linear.y * max;
            float z = linear.z * max;

            float roll = angular.x * max;
            float yaw = angular.z * max;

            float fl = 0f;
            float fr = 0f;
            float rl = 0f;
            float rr = 0f;

            // Translation
            fl += -x - y;
            fr += x - y;
            rl += -x + y;
            rr += x + y;

            // Yaw
            fl += -yaw;
            fr += yaw;
            rl += yaw;
            rr += -yaw;

            float vl = 0f;
            float vr = 0f;

            // Vertical movement
            vl += z;
            vr += z;

            // Roll
            vl += -roll;
            vr += roll;

            frontLeft.SetCommand(fl);
            frontRight.SetCommand(fr);
            rearLeft.SetCommand(rl);
            rearRight.SetCommand(rr);

            verticalLeft.SetCommand(vl);
            verticalRight.SetCommand(vr);
        }
    }
}
