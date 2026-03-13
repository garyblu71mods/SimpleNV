using UnityEngine;

namespace KerbVisionIR
{
    public class VisionLightingController
    {
        private Color storedAmbientLight;
        private float storedAmbientIntensity;
        private bool hasStoredValues = false;

        public void StoreOriginalLighting()
        {
            if (!hasStoredValues)
            {
                storedAmbientLight = RenderSettings.ambientLight;
                storedAmbientIntensity = RenderSettings.ambientIntensity;
                hasStoredValues = true;
                Debug.Log($"[KerbVisionIR] Stored original lighting - Light: {storedAmbientLight}, Intensity: {storedAmbientIntensity}");
            }
        }

        public void ApplyBrightness(float brightness, Color tint)
        {
            if (!hasStoredValues)
            {
                Debug.LogWarning("[KerbVisionIR] Attempted to apply brightness without stored values");
                return;
            }

            RenderSettings.ambientLight = storedAmbientLight * tint * brightness;
            RenderSettings.ambientIntensity = storedAmbientIntensity * brightness;
        }

        public void RestoreOriginalLighting()
        {
            if (hasStoredValues)
            {
                RenderSettings.ambientLight = storedAmbientLight;
                RenderSettings.ambientIntensity = storedAmbientIntensity;
                Debug.Log("[KerbVisionIR] Restored original lighting");
            }
        }
    }
}
