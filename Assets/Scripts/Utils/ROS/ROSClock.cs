using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Rosgraph;

namespace Sim.Utils.ROS {
    public class ROSClock : MonoBehaviour {
        [SerializeField] private Clock.ClockMode clockMode;

        [SerializeField, HideInInspector] private Clock.ClockMode lastSetClockMode;

        [SerializeField] private double publishRateHz = 100f;

        private double lastPublishTimeSeconds;

        ROSConnection ros;

        private double PublishPeriodSeconds => 1.0f / publishRateHz;

        private bool ShouldPublishMessage => Clock.FrameStartTimeInSeconds - PublishPeriodSeconds > lastPublishTimeSeconds;

        private void OnValidate() {
            var clocks = FindObjectsByType<ROSClock>(FindObjectsSortMode.None);
            if (clocks.Length > 1) {
                Debug.LogWarning("Found too many clock publishers in the scene, there should only be one!");
            }

            if (Application.isPlaying && lastSetClockMode != clockMode) {
                Debug.LogWarning("Can't change ClockMode during simulation! Setting it back...");
                clockMode = lastSetClockMode;
            }

            SetClockMode(clockMode);
        }

        private void SetClockMode(Clock.ClockMode mode) {
            Clock.Mode = mode;
            lastSetClockMode = mode;
        }

        // Start is called before the first frame update
        private void Start() {
            SetClockMode(clockMode);
            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<ClockMsg>("clock");
        }

        private void PublishMessage() {
            var publishTime = Clock.time;
            var clockMsg = new TimeMsg {
                sec = (int)publishTime,
                nanosec = (uint)((publishTime - Math.Floor(publishTime)) * Clock.k_NanoSecondsInSeconds)
            };
            lastPublishTimeSeconds = publishTime;
            ros.Publish("clock", clockMsg);
        }

        private void Update() {
            if (ShouldPublishMessage) {
                PublishMessage();
            }
        }
    }
}
