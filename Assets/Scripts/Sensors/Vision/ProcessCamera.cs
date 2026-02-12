using UnityEngine;

namespace Sim.Sensors.Vision {
    public enum SensorEnum {
        RGB,
        Depth
    }

    [RequireComponent(typeof(Camera))]
    public class ProcessRenderTexture : MonoBehaviour {
        [SerializeField] private SensorEnum sensorType;
        [SerializeField] private RenderTexture inputRenderTexture;
        [SerializeField] private RenderTexture normalsRenderTexture;
        [SerializeField] private RenderTexture outputRenderTexture;
        [SerializeField] private bool drawFrustum = false;
        private Camera cam;

        [SerializeField, Range(0.0f, 1.0f)] private float flatNoise = 0.0f;
        [SerializeField, Range(0.0f, 1.0f)] private float depthAngleNoiseGain = 0.0f;
        [SerializeField, Range(-1.0f, 1.0f)] private float K1 = 0.0f;
        [SerializeField, Range(-1.0f, 1.0f)] private float K2 = 0.0f;
        [SerializeField, Range(-1.0f, 1.0f)] private float K3 = 0.0f;
        [SerializeField, Range(-1.0f, 1.0f)] private float T1 = 0.0f;
        [SerializeField, Range(-1.0f, 1.0f)] private float T2 = 0.0f;
        private Material mat;
        private Vector3[] frustumCorners = new Vector3[4];
        private Vector3[] normCorners = new Vector3[4];

        private void Start() {
            if (sensorType == SensorEnum.Depth) {
                mat = new Material(Shader.Find("Unlit/NoiseDistortDepth"));
                cam = GetComponent<Camera>();
            }
            else if (sensorType == SensorEnum.RGB) {
                mat = new Material(Shader.Find("Unlit/NoiseDistortRGB"));
            }
        }

        private void Update() {
            if (sensorType == SensorEnum.Depth) {
                mat.SetTexture("_NormalsTex", normalsRenderTexture);
                cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

                for (int i = 0; i < 4; i++) {
                    normCorners[i] = cam.transform.TransformVector(frustumCorners[i]);
                    normCorners[i] = normCorners[i].normalized;
                    if (drawFrustum)
                        Debug.DrawRay(cam.transform.position, normCorners[i], Color.red);
                }

                mat.SetVector("_BL", normCorners[0]);
                mat.SetVector("_TL", normCorners[1]);
                mat.SetVector("_TR", normCorners[2]);
                mat.SetVector("_BR", normCorners[3]);
                mat.SetFloat("_depth_angle_noise_gain", depthAngleNoiseGain);
            }
            mat.SetTexture("_MainTex", inputRenderTexture);
            mat.SetFloat("_K1", K1);
            mat.SetFloat("_K2", K2);
            mat.SetFloat("_K3", K3);
            mat.SetFloat("_T1", T1);
            mat.SetFloat("_T2", T2);
            mat.SetFloat("_flat_noise", flatNoise);
            Graphics.Blit(inputRenderTexture, outputRenderTexture, mat);
        }
    }
}
