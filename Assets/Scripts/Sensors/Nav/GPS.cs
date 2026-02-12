
using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Sim.Utils;
using Sim.Utils.ROS;

namespace Sim.Sensors.Nav {
    public class GPS : MonoBehaviour, IROSSensor<NavSatFixMsg> {
        [SerializeField] private string topicName = "gps/raw";
        [SerializeField] private string frameId = "gps_link";
        [SerializeField] private float Hz = 20.0f;
        public ROSPublisher publisher { get; set; }

        public NavSatFixMsg CreateMessage() {
            return new NavSatFixMsg {
                header = publisher.CreateHeader()
            };
        }

        private void Awake() {
            publisher = gameObject.AddComponent<ROSPublisher>();
        }

        void Start() {
            publisher.Initialize(topicName, frameId, CreateMessage, Hz);
        }
    }
}
