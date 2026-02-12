using RosMessageTypes.Sensor;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Sim.Utils.ROS;

namespace Sim.Sensors.Lidar {
    public class Lidar2D : MonoBehaviour, IROSSensor<LaserScanMsg> {
        [SerializeField] private float minAngleDegrees = -45.0f;
        [SerializeField] private float maxAngleDegrees = 45.0f;
        [SerializeField] private float angleIncrementDegrees = 1.0f;
        [SerializeField] private float minRange = 0.1f;
        [SerializeField] private float maxRange = 50.0f;
        [SerializeField] private int batchSize = 500;
        [SerializeField] private bool drawRays = false;

        [SerializeField] private string topicName = "scan";
        [SerializeField] private string frameId = "lidar_link";
        [SerializeField] private float Hz = 5.0f;
        public ROSPublisher publisher { get; set; }

        private Vector3[] scanDirVectors;

        private void Awake() {
            publisher = gameObject.AddComponent<ROSPublisher>();
        }

        private void Start() {
            publisher.Initialize(topicName, frameId, CreateMessage, Hz);

            scanDirVectors = GenerateScanVectors();
        }

        public LaserScanMsg CreateMessage() {
            // transformScale = transform.lossyScale;
            scanDirVectors = GenerateScanVectors();
            float[] dists = PerformScan(scanDirVectors);
            return DistancesToLaserscan(dists);
        }

        private Vector3[] GenerateScanVectors() {
            int numBeams = (int)((maxAngleDegrees - minAngleDegrees) / (angleIncrementDegrees));
            Debug.Assert(numBeams >= 0, "Number of beams is negative. Check min/max angle and angle increment.");
            Vector3[] scanVectors = new Vector3[numBeams];
            float minAngleRad = Mathf.Deg2Rad * minAngleDegrees;
            float angleIncrementRad = Mathf.Deg2Rad * angleIncrementDegrees;
            for (int i = 0; i < numBeams; i++) {
                float hRot = minAngleRad + angleIncrementRad * i;
                float x = -Mathf.Sin(hRot);
                float y = 0;
                float z = Mathf.Cos(hRot);
                scanVectors[i] = new Vector3(x, y, z);
            }
            return scanVectors;
        }

        private float[] PerformScan(Vector3[] dirs) {
            int numPoints = dirs.Length;
            var commands = new NativeArray<RaycastCommand>(numPoints, Allocator.TempJob);
            var results = new NativeArray<RaycastHit>(numPoints, Allocator.TempJob);

            for (int i = 0; i < numPoints; i++) {
                Vector3 origin = transform.position;
                Vector3 direction = transform.rotation * dirs[i];
                commands[i] = new RaycastCommand(origin, direction, QueryParameters.Default, maxRange);
            }

            JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, batchSize, 1);
            handle.Complete();

            float[] dists = new float[numPoints + 1];
            for (int i = 0; i < numPoints; i++) {
                var hit = results[i];
                if (hit.collider != null && (transform.position - hit.point).sqrMagnitude > minRange * minRange) {
                    Vector3 beam = transform.InverseTransformPoint(hit.point);
                    dists[i] = hit.distance;
                    if (drawRays) {
                        Debug.DrawLine(transform.position, transform.TransformPoint(beam), Color.red);
                    }
                }
                else {
                    dists[i] = float.NaN;
                }
            }

            results.Dispose();
            commands.Dispose();
            return dists;
        }

        private LaserScanMsg DistancesToLaserscan(float[] dists) {
            LaserScanMsg msg = new() {
                header = publisher.CreateHeader(),

                angle_min = minAngleDegrees * Mathf.Deg2Rad,
                angle_max = maxAngleDegrees * Mathf.Deg2Rad,
                angle_increment = angleIncrementDegrees * Mathf.Deg2Rad,
                scan_time = 1.0f / Hz,
                range_min = minRange,
                range_max = maxRange,
                ranges = dists
            };
            return msg;
        }
    }
}
