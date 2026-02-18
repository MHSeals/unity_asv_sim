using RosMessageTypes.Vision;
using RosMessageTypes.Geometry;
using Sim.Utils.ROS;
using UnityEngine;

namespace Sim.Sensors.Vision {
    public class BoundingBox3D : MonoBehaviour {
        [SerializeField, Range(0.01f, 0.3f)] float visualRadius = 0.05f;
        [SerializeField] bool drawGizmo = false;
        [SerializeField] private string topicName = "/detections";
        [SerializeField] private string frameId = "front_camera_link";
        [SerializeField] private Camera sensorCamera;
        [SerializeField] private float Hz = 10.0f;

        public ROSPublisher publisher { get; set; }
        private float timeSincePublish = 0.0f;

        private Vector3 ComputeCentroid() {
            Vector3 totalPosition = Vector3.zero;
            Transform[] children = GetComponentsInChildren<Transform>();

            int childCount = children.Length;

            foreach (Transform child in children) {
                totalPosition += child.position;
            }

            return totalPosition / childCount;
        }

        public PoseMsg CreateMessage() {
            Vector3 centroid = ComputeCentroid();
            Quaternion orientation = transform.rotation;
            return new PoseMsg(
                new PointMsg(centroid.x, centroid.y, centroid.z),
                new QuaternionMsg(orientation.x, orientation.y, orientation.z, orientation.w)
            );
        }

        private void Awake() {
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
                Vector3 screenPoint = sensorCamera.WorldToViewportPoint(ComputeCentroid());
                bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
                if (onScreen) publisher.Publish();
                timeSincePublish = 0.0f;
            }
        }

        private void OnDrawGizmos() {
            if (!drawGizmo) return;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(ComputeCentroid(), visualRadius);
        }

        // private void OnDrawGizmos() {
        //     if (!drawGizmo) return;

        //     Renderer[] renderers = GetComponentsInChildren<Renderer>();
        //     if (renderers.Length == 0) return;

        //     Transform root = transform;

        //     bool initialized = false;
        //     Vector3 min = Vector3.zero;
        //     Vector3 max = Vector3.zero;

        //     foreach (var r in renderers) {
        //         Bounds b = r.bounds;

        //         // Get 8 world corners of this renderer's bounds
        //         Vector3 c = b.center;
        //         Vector3 e = b.extents;

        //         Vector3[] corners = new Vector3[8]
        //         {
        //             c + new Vector3(-e.x,-e.y,-e.z),
        //             c + new Vector3(e.x,-e.y,-e.z),
        //             c + new Vector3(-e.x,e.y,-e.z),
        //             c + new Vector3(e.x,e.y,-e.z),
        //             c + new Vector3(-e.x,-e.y,e.z),
        //             c + new Vector3(e.x,-e.y,e.z),
        //             c + new Vector3(-e.x,e.y,e.z),
        //             c + new Vector3(e.x,e.y,e.z)
        //         };

        //         // Convert to parent local space
        //         foreach (var corner in corners) {
        //             Vector3 local = root.InverseTransformPoint(corner);

        //             if (!initialized) {
        //                 min = max = local;
        //                 initialized = true;
        //             } else {
        //                 min = Vector3.Min(min, local);
        //                 max = Vector3.Max(max, local);
        //             }
        //         }
        //     }

        //     DrawBoundingBox(min, max);
        // }

        // void DrawBoundingBox(Vector3 min, Vector3 max) {
        //     Vector3 c = (min + max) * 0.5f;
        //     Vector3 e = (max - min) * 0.5f;

        //     Vector3[] localCorners = new Vector3[8]
        //     {
        //         c + new Vector3(-e.x,-e.y,-e.z),
        //         c + new Vector3(e.x,-e.y,-e.z),
        //         c + new Vector3(-e.x,e.y,-e.z),
        //         c + new Vector3(e.x,e.y,-e.z),
        //         c + new Vector3(-e.x,-e.y,e.z),
        //         c + new Vector3(e.x,-e.y,e.z),
        //         c + new Vector3(-e.x,e.y,e.z),
        //         c + new Vector3(e.x,e.y,e.z)
        //     };

        //     Vector3[] worldCorners = new Vector3[8];
        //     for (int i = 0; i < 8; i++)
        //         worldCorners[i] = transform.TransformPoint(localCorners[i]);

        //     Gizmos.color = Color.red;

        //     foreach (var corner in worldCorners)
        //         Gizmos.DrawSphere(corner, visualRadius);

        //     int[,] edges = {
        //         {0,1},{1,3},{3,2},{2,0},
        //         {4,5},{5,7},{7,6},{6,4},
        //         {0,4},{1,5},{2,6},{3,7}
        //     };

        //     for (int i = 0; i < edges.GetLength(0); i++)
        //         Gizmos.DrawLine(
        //             worldCorners[edges[i, 0]],
        //             worldCorners[edges[i, 1]]
        //         );
        // }
    }
}
