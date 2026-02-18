using RosMessageTypes.Vision;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Sim.Utils.ROS;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Sim.Sensors.Vision {
    [System.Serializable]
    public class ObjectEntry
    {
        public GameObject obj;
        public string id;
    }

    public class BoundingBox3D : MonoBehaviour {
        [SerializeField, Range(0.01f, 0.3f)] private float visualRadius = 0.05f;
        [SerializeField] private bool drawGizmo = false;
        [SerializeField] private string topicName = "/detections";
        [SerializeField] private string frameId = "front_camera_link";
        [SerializeField] private Camera sensorCamera;
        [SerializeField] private float Hz = 10.0f;

        public ROSPublisher publisher { get; set; }
        private float timeSincePublish = 0.0f;
        
        [SerializeField] private List<ObjectEntry> objects = new();
        private Dictionary<GameObject, string> objectDict = new();

        private void Awake() {
            objectDict.Clear();
            foreach (var entry in objects) {
                if (entry.obj != null && !string.IsNullOrEmpty(entry.id))
                    objectDict[entry.obj] = entry.id;
            }
        
            if (sensorCamera == null) {
                Debug.LogError("Missing a camera reference.");
                enabled = false;
                return;
            }
        
            publisher = gameObject.AddComponent<ROSPublisher>();
        }

        private void Start() {
            publisher.Initialize(topicName, frameId, CreateMessage, Hz, true);
        }

        private void FixedUpdate() {
            timeSincePublish += Time.fixedDeltaTime;
            if (timeSincePublish > 1.0f / Hz) {
                publisher.Publish();
                timeSincePublish = 0.0f;
            }
        }

        public Detection3DArrayMsg CreateMessage() {
            List<Detection3DMsg> detections = new();
        
            foreach (var kvp in objectDict) {
                GameObject obj = kvp.Key;
                string id = kvp.Value;
        
                Vector3[] corners = ComputeCorners(obj);
                Vector3 centroid = ComputeCentroid(corners);
        
                Vector3 screenPoint = sensorCamera.WorldToViewportPoint(centroid);
                bool onScreen =
                    screenPoint.z > 0 &&
                    screenPoint.x > 0 && screenPoint.x < 1 &&
                    screenPoint.y > 0 && screenPoint.y < 1;
        
                if (onScreen)
                    detections.Add(GenerateDetection(corners, id));
            }
        
            HeaderMsg header = publisher.CreateHeader();
            return new Detection3DArrayMsg(header, detections.ToArray());
        }

        private Detection3DMsg GenerateDetection(Vector3[] corners, string id) {
        
            Vector3 centroid = ComputeCentroid(corners);
            Vector3 dimensions = ComputeDimensions(corners);
        
            Quaternion q = Quaternion.identity;
        
            PoseMsg pose = new PoseMsg(
                new PointMsg(centroid.x, centroid.y, centroid.z),
                new QuaternionMsg(q.x, q.y, q.z, q.w)
            );
        
            // Zero covariance (36 values)
            double[] covariance = new double[36];
        
            ObjectHypothesisWithPoseMsg hypothesis =
                new(
                    new ObjectHypothesisMsg(id, 1.0f),
                    new PoseWithCovarianceMsg(pose, covariance)
                );
        
            BoundingBox3DMsg bbox = new(
                new PoseMsg(
                    new PointMsg(centroid.x, centroid.y, centroid.z),
                    new QuaternionMsg(q.x, q.y, q.z, q.w)
                ),
                new Vector3Msg(dimensions.x, dimensions.y, dimensions.z)
            );
        
            HeaderMsg header = publisher.CreateHeader();
        
            return new Detection3DMsg(
                header,
                new ObjectHypothesisWithPoseMsg[] { hypothesis },
                bbox,
                id
            );
        }

        private Vector3 ComputeDimensions(Vector3[] corners) {
            Vector3 min = corners[0];
            Vector3 max = corners[0];
        
            foreach (var c in corners) {
                min = Vector3.Min(min, c);
                max = Vector3.Max(max, c);
            }
        
            return max - min;
        }
        
        private Vector3 ComputeCentroid(Vector3[] corners) {
            Vector3 total = Vector3.zero;
            foreach (Vector3 corner in corners) total += corner;
            return total / corners.Length;
        }

        private Vector3[] ComputeCorners(GameObject obj) {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0) return Enumerable.Repeat(obj.transform.position, 8).ToArray();

            Transform root = transform;

            bool initialized = false;
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;
            Vector3 c, e; 

            foreach (var r in renderers) {
                Bounds b = r.bounds;

                // Get 8 world corners of this renderer's bounds
                c = b.center;
                e = b.extents;

                Vector3[] corners = new Vector3[8]
                {
                    c + new Vector3(-e.x,-e.y,-e.z),
                    c + new Vector3(e.x,-e.y,-e.z),
                    c + new Vector3(-e.x,e.y,-e.z),
                    c + new Vector3(e.x,e.y,-e.z),
                    c + new Vector3(-e.x,-e.y,e.z),
                    c + new Vector3(e.x,-e.y,e.z),
                    c + new Vector3(-e.x,e.y,e.z),
                    c + new Vector3(e.x,e.y,e.z)
                };

                // Convert to parent local space
                foreach (var corner in corners) {
                    Vector3 local = root.InverseTransformPoint(corner);

                    if (!initialized) {
                        min = max = local;
                        initialized = true;
                    } else {
                        min = Vector3.Min(min, local);
                        max = Vector3.Max(max, local);
                    }
                }
            }

            c = (min + max) * 0.5f;
            e = (max - min) * 0.5f;

            Vector3[] localCorners = new Vector3[8] {
                c + new Vector3(-e.x,-e.y,-e.z),
                c + new Vector3(e.x,-e.y,-e.z),
                c + new Vector3(-e.x,e.y,-e.z),
                c + new Vector3(e.x,e.y,-e.z),
                c + new Vector3(-e.x,-e.y,e.z),
                c + new Vector3(e.x,-e.y,e.z),
                c + new Vector3(-e.x,e.y,e.z),
                c + new Vector3(e.x,e.y,e.z)
            };

            Vector3[] worldCorners = new Vector3[8];
            for (int i = 0; i < 8; i++)
                worldCorners[i] = transform.TransformPoint(localCorners[i]);

            return worldCorners;
        }

        private void OnDrawGizmos() {
            if (!drawGizmo) return;
            foreach(var kvp in objectDict) DrawBoundingBox(ComputeCorners(kvp.Key));
        }

        void DrawBoundingBox(Vector3[] corners) {
            Gizmos.color = Color.red;

            foreach (var corner in corners)
                Gizmos.DrawSphere(corner, visualRadius);

            int[,] edges = {
                {0,1},{1,3},{3,2},{2,0},
                {4,5},{5,7},{7,6},{6,4},
                {0,4},{1,5},{2,6},{3,7}
            };

            for (int i = 0; i < edges.GetLength(0); i++)
                Gizmos.DrawLine(
                    corners[edges[i, 0]],
                    corners[edges[i, 1]]
                );
        }
    }
}
