using UnityEngine;

namespace Sim.Utils {
    public class LimitFPS : MonoBehaviour {
        [SerializeField] private int targetFPS = 60;

        private void Start() {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFPS;
        }

        private void Update() {
            if (Application.targetFrameRate != targetFPS)
                Application.targetFrameRate = targetFPS;
        }
    }
}
