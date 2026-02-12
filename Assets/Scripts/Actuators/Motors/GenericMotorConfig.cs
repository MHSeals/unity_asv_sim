using UnityEngine;
using System;
using Sim.Utils;

namespace Sim.Actuators.Motors {
    [CreateAssetMenu(menuName = "Motors/GenericMotorConfig")]
    public class GenericMotorConfig : ScriptableObject, IMotorConfig {
        [field: SerializeField] public MotorControlMode controlMode { get; set; } = MotorControlMode.Velocity;
        [field: SerializeField] public Axis rotationAxis { get; set; } = Axis.Y;
        [field: SerializeField, Tooltip("Pid controller for position control")] public Pid pid { get; set; } = new Pid(3.0f, 0.0f, 2.0f);
        [field: SerializeField, Tooltip("Nm")] public float maxTorque { get; set; } = 10.0f;
        [field: SerializeField, Tooltip("rad/s, Caps angular velocity")] public float maxAngularVelocity { get; set; } = 7.0f;
        [field: SerializeField, Tooltip("Torque Responsiveness"), Range(0.0f, 1.0f)] public float torqueK { get; set; } = 1.0f;
        [field: SerializeField, Tooltip("Deg")] public float minAngle { get; set; } = float.NegativeInfinity;
        [field: SerializeField, Tooltip("Deg")] public float maxAngle { get; set; } = float.PositiveInfinity;
    }
}
