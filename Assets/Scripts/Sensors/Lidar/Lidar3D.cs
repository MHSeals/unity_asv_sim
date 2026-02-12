using System;
using System.Collections.Generic;
using RosMessageTypes.Sensor;
using Unity.Collections;
using Unity.Jobs;
using Sim.Utils.ROS;
using UnityEngine;

namespace Sim.Sensors.Lidar {
    public class Lidar3D : MonoBehaviour, IROSSensor<PointCloud2Msg> {
        [SerializeField, Range(0.1f, 200.0f)] private float maxRange = 100.0f;
        [SerializeField, Range(0, 5000)] private int numHorizontalBeams = 500;
        [SerializeField, Range(0, 16)] private int numVerticalBeams = 16;
        [SerializeField, Range(0.1f, 5.0f)] private float minDistance = 0.3f;
        [SerializeField, Range(0.0f, 2.0f * Mathf.PI)] private float horizontalFOV = 60.0f * Mathf.PI;
        [SerializeField, Range(0.0f, Mathf.PI)] private float verticalFOV = 0.5f * Mathf.PI;
        [SerializeField] private int batchSize = 500;
        [SerializeField] private bool drawRays = true;

        [SerializeField] private string topicName = "points";
        [SerializeField] private string frameId = "lidar_link";
        [SerializeField] private float Hz = 10.0f;
        public ROSPublisher publisher { get; set; }

        private Vector3[] scanDirVectors;
        private Transform transformCache;
        private Vector3 transformScale;

        private float[] scanPatternParams;
        private float[] scanPatternParamsPrev;

        private void Awake() {
            publisher = gameObject.AddComponent<ROSPublisher>();
        }

        private void Start() {
            publisher.Initialize(topicName, frameId, CreateMessage, Hz);

            scanDirVectors = GenerateScanVectors();
            scanPatternParamsPrev = new float[4];
        }

        private void FixedUpdate() {
            // dont re-calculate lidar scan vectors if parameters unchanged
            scanPatternParams = new[] { numHorizontalBeams, numVerticalBeams, horizontalFOV, verticalFOV };
            if (scanPatternParams != scanPatternParamsPrev) {
                scanDirVectors = GenerateScanVectors();
            }
            transformCache = transform;
            // ROSPublisher handles publishing internally, so no extra UpdatePublish() needed
        }

        public PointCloud2Msg CreateMessage() {
            transformScale = transform.lossyScale;
            Vector3[] points = PerformScan(scanDirVectors);
            PointCloud2Msg msg = PointsToPointCloud2(points);
            scanPatternParamsPrev = scanPatternParams;
            return msg;
        }

        private Vector3[] GenerateScanVectors() {
            float fidelityHorizontal = horizontalFOV / numHorizontalBeams;
            float fidelityVertical = verticalFOV / numVerticalBeams;

            Vector3[] scanVectors = new Vector3[numHorizontalBeams * numVerticalBeams];

            for (int i = 0; i < numHorizontalBeams; i++) {
                float hRot = 0.5f * (Mathf.PI - horizontalFOV) + fidelityHorizontal * i;

                for (int j = 0; j < numVerticalBeams; j++) {
                    float vRot = 0.5f * (Mathf.PI - verticalFOV) + (fidelityVertical * j);

                    float x = Mathf.Sin(vRot) * Mathf.Cos(hRot);
                    float y = Mathf.Cos(vRot);
                    float z = Mathf.Sin(vRot) * Mathf.Sin(hRot);

                    scanVectors[i] = new Vector3(x, y, z);
                    i++;
                }
            }
            return scanVectors;
        }

        private Vector3[] PerformScan(Vector3[] dirs) {
            int numPoints = dirs.Length;
            Vector3 nanVec = new Vector3(float.NaN, float.NaN, float.NaN);
            var commands = new NativeArray<RaycastCommand>(numPoints, Allocator.TempJob);
            var results = new NativeArray<RaycastHit>(numPoints, Allocator.TempJob);

            for (int i = 0; i < numPoints; i++) {
                Vector3 origin = transformCache.position;
                Vector3 direction = transformCache.rotation * dirs[i];
                commands[i] = new RaycastCommand(origin, direction, QueryParameters.Default, maxRange);
            }

            JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, batchSize, 1);
            handle.Complete();

            Vector3[] points = new Vector3[numPoints];
            for (int i = 0; i < numPoints; i++) {
                var hit = results[i];
                if (hit.collider != null && (transformCache.position - hit.point).sqrMagnitude > minDistance * minDistance) {
                    Vector3 beam = transformCache.InverseTransformPoint(hit.point);
                    points[i] = beam;

                    if (drawRays) {
                        Debug.DrawLine(transformCache.position, transform.TransformPoint(beam), Color.red);
                    }
                }
                else {
                    points[i] = nanVec;
                }
            }

            results.Dispose();
            commands.Dispose();
            return points;
        }

        private PointCloud2Msg PointsToPointCloud2(Vector3[] points) {
            PointCloud2Msg msg = new PointCloud2Msg();
            msg.header = publisher.CreateHeader();

            // publishing as unordered cloud (height = 1). might reconsider later, idk.
            msg.height = 1;
            // currently the size of each message is non-constant, as the number of scan returns varies.
            // could consider having a constant size pointcloud and filling non-hits with NaN values.
            msg.width = (uint)points.Length;

            PointFieldMsg[] fields = new PointFieldMsg[3];
            fields[0] = new PointFieldMsg("x", 0, 7, 1); // "name", offset, datatype (7 = float), number of elements in field
            fields[1] = new PointFieldMsg("y", 4, 7, 1); // 4 byte offset, since float32 uses 4 bytes
            fields[2] = new PointFieldMsg("z", 8, 7, 1); // another 4 bytes as offset
            // theres an option for this field too, but i dont see a use for it currently
            // fields[3] = new PointFieldMsg("intensity", 12, 7, msg.width);
            msg.fields = fields;

            msg.point_step = (uint)fields.Length * 4; // each point needs 12 bytes (3 float32's, when using fields x, y, z)
            msg.row_step = msg.point_step * msg.width;
            msg.is_dense = true;

            // finally, populate the data field, containing the actual points in bytes
            List<byte> dataList = new List<byte>();
            foreach (Vector3 point in points) {
                dataList.AddRange(BitConverter.GetBytes(point.z * transformScale.z));
                dataList.AddRange(BitConverter.GetBytes(-point.x * transformScale.x));
                dataList.AddRange(BitConverter.GetBytes(point.y * transformScale.y));
            }
            msg.data = dataList.ToArray();
            return msg;
        }
    }
}
