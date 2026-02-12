using UnityEngine;
using TMPro;

namespace Sim.UI {
    public class showFPS : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI fpsText;
        private float deltaTime = 0.0f;

        private void Update() {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            fpsText.text = "FPS: " + Mathf.Ceil(fps).ToString();
        }
    }
}
