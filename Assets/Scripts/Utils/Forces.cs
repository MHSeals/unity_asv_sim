using UnityEngine;

namespace Sim.Utils {
    public static class Forces {
        public static Vector3 BuoyancyForce(float height, Vector3 normal, float area) {
            Vector3 F = Constants.waterDensity * Constants.gravity * height * normal * area;
            Vector3 FVertical = new Vector3(0.0f, F.y, 0.0f);
            return FVertical;
        }
    }
}
