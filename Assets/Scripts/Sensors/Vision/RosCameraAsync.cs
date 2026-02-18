using RosMessageTypes.Sensor;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using Sim.Utils.ROS;

namespace Sim.Sensors.Vision {
    public class ROSCameraAsync : MonoBehaviour, IROSSensor<ImageMsg> {
        [SerializeField] private RenderTexture rgbRenderTexture;
        [SerializeField] private Camera sensorCamera;

        [SerializeField] private string topicName = "camera/image_raw";
        [SerializeField] private string frameId = "front_camera_link";
        [SerializeField] private float Hz = 15.0f;
        public ROSPublisher publisher { get; set; }

        private Texture2D rgbTexture2D;
        private float timeSincePublish = 0.0f;

        private void Awake() {
            if (sensorCamera == null) {
                Debug.LogError("Missing a camera reference.");
                enabled = false;
                return;
            }

            publisher = gameObject.AddComponent<ROSPublisher>();
        }

        private void Start() {
            sensorCamera.targetTexture = rgbRenderTexture;
            publisher.Initialize(topicName, frameId, CreateMessage, Hz, true);
        }

        public ImageMsg CreateMessage() {
            return rgbTexture2D.ToImageMsg(publisher.CreateHeader());
        }

        private void FixedUpdate() {
            timeSincePublish += Time.fixedDeltaTime;
            if (timeSincePublish > 1.0f / Hz) {
                RequestReadback(rgbRenderTexture);
                timeSincePublish = 0.0f;
            }
        }

        private void RequestReadback(RenderTexture targetTexture) {
            AsyncGPUReadback.Request(targetTexture, 0, TextureFormat.RGB24, OnReadbackComplete);
        }

        private void OnReadbackComplete(AsyncGPUReadbackRequest request) {
            if (request.hasError) {
                Debug.LogWarning("Failed to read back texture once");
                return;
            }

            if (rgbTexture2D == null || rgbTexture2D.width != sensorCamera.targetTexture.width || rgbTexture2D.height != sensorCamera.targetTexture.height) {
                rgbTexture2D = new Texture2D(sensorCamera.targetTexture.width, sensorCamera.targetTexture.height, TextureFormat.RGB24, false);
            }

            rgbTexture2D.LoadRawTextureData(request.GetData<byte>());
            rgbTexture2D.Apply();

            if (publisher != null) publisher.Publish();
        }
    }
}
