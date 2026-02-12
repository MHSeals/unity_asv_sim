using RosMessageTypes.Sensor;
using UnityEngine;
using Sim.Utils.ROS;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace Sim.Sensors.Vision {
    [RequireComponent(typeof(Camera))]
    public class CameraInfo : MonoBehaviour, IROSSensor<CameraInfoMsg> {
        [SerializeField] private string topicName = "camera/camera_info";
        [SerializeField] private string frameId = "camera_link_optical_frame";
        [SerializeField] private float Hz = 5.0f;
        public ROSPublisher publisher { get; set; }

        private Camera sensorCamera;

        private void Awake() {
            publisher = gameObject.AddComponent<ROSPublisher>();
        }

        private void Start() {
            sensorCamera = GetComponent<Camera>();
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
