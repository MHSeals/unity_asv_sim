using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Sim.Utils;
using Sim.Utils.ROS;

namespace Sim.Sensors.Nav {
    public class Imu : MonoBehaviour, IROSSensor<ImuMsg> {
        [SerializeField] private string topicName = "imu/raw";
        [SerializeField] private string frameId = "imu_link";
        [SerializeField] private float Hz = 50.0f;
        public ROSPublisher publisher { get; set; }

        public IPhysicsBody body;

        private void OnValidate() {
            if (GetComponent<Rigidbody>() == null && GetComponent<ArticulationBody>() == null)
                Debug.LogWarning($"{name} should have either a Rigidbody or an ArticulationBody attached.");
        }

        private void Awake() {
            var rb = GetComponent<Rigidbody>();
            var ab = GetComponent<ArticulationBody>();

            if (rb != null) body = new RigidbodyAdapter(rb);
            else if (ab != null) body = new ArticulationBodyAdapter(ab);
            else throw new MissingComponentException($"{name} requires a Rigidbody or ArticulationBody!");

            publisher = gameObject.AddComponent<ROSPublisher>();
        }

        public ImuMsg CreateMessage() {
            return new ImuMsg {
                orientation = body.transform.rotation.To<FLU>(),
                angular_velocity = body.angularVelocity.To<FLU>(),
                header = publisher.CreateHeader()
            };
        }

        private void Start() {
            publisher.Initialize(topicName, frameId, CreateMessage, Hz);
        }
    }
}
