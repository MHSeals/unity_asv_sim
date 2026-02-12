using System;
using System.Reflection;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

namespace Sim.Utils.ROS {
    [AddComponentMenu("")]
    public class ROSPublisher : MonoBehaviour {
        public string topicName { get; set; }
        public string frameId { get; set; }
        public float Hz;
        public bool manualPublish;

        private ROSConnection ros;
        private float time;

        private Func<object> createMessage;
        private Action<string, object> publishTyped;

        private static void PublishWrapper<T>(ROSConnection ros, string topic, object msg)
        where T : Unity.Robotics.ROSTCPConnector.MessageGeneration.Message {
            ros.Publish(topic, (T)msg);
        }

        private void OnEnable() { hideFlags = HideFlags.HideInInspector; }

        public void Initialize<T>(string topicName, string frameId, Func<T> createMessage, float Hz=10f, bool manualPublish=false)
        where T : Unity.Robotics.ROSTCPConnector.MessageGeneration.Message {
            this.topicName = topicName;
            this.frameId = frameId;
            this.Hz = Hz;
            this.manualPublish = manualPublish;

            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<T>(topicName);

            this.createMessage = () => createMessage();

            var wrapperMethod = typeof(ROSPublisher)
                .GetMethod(nameof(PublishWrapper), BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(typeof(T));

            publishTyped = (Action<string, object>)
                Delegate.CreateDelegate(
                    typeof(Action<string, object>),
                    ros,
                    wrapperMethod
                );
        }

        private void FixedUpdate() {
            if (manualPublish) return;
            if (createMessage == null) return;

            time += Time.fixedDeltaTime;
            if (time >= 1f / Hz) {
                Publish();
                time = 0f;
            }
        }

        public void Publish() {
            publishTyped(topicName, createMessage());
        }

        public HeaderMsg CreateHeader() {
            var header = new HeaderMsg { frame_id=frameId };
            var publishTime = Clock.Now;
            var sec = publishTime;
            var nanosec = (publishTime - Math.Floor(publishTime)) * Clock.k_NanoSecondsInSeconds;
            header.stamp.sec = (int)sec;
            header.stamp.nanosec = (uint)nanosec;
            return header;
        }
    }
}
