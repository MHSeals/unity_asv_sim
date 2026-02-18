using RosMessageTypes.Sensor;
using UnityEngine;
using Sim.Utils.ROS;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace Sim.Sensors.Vision {
    public class CameraInfo : MonoBehaviour, IROSSensor<CameraInfoMsg> {
        [SerializeField] private string topicName = "camera/camera_info";
        [SerializeField] private string frameId = "front_camera_link";
        [SerializeField] private float Hz = 5.0f;
        [SerializeField] private Camera sensorCamera;
        public ROSPublisher publisher { get; set; }

        private void Awake() {
            if (sensorCamera == null) {
                Debug.LogError("Missing a camera reference.");
                enabled = false;
                return;
            }

            publisher = gameObject.AddComponent<ROSPublisher>();
        }

        private void Start() {
            publisher.Initialize(topicName, frameId, CreateMessage, Hz);
        }

        public CameraInfoMsg CreateMessage() {
            return CameraInfoGenerator.ConstructCameraInfoMessage(
                sensorCamera,
                publisher.CreateHeader(),
                0f,
                1.0f
            );
        }
    }
}
