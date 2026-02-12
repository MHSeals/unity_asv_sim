using UnityEngine;
using Sim.Utils.ROS;
using RosMessageTypes.Std;

namespace Sim.Actuators.Motors {
    public class ROSThruster : Thruster {
        [SerializeField] private string topicName;

        private ROSSubscriber ros;

        protected override void Awake() {
            base.Awake();
            ros.Initialize<Float32Msg>(topicName, CommandCallback);
        }

        private void CommandCallback(Float32Msg msg) {
            base.SetCommand(msg.data);
        }
    }
}
