using UnityEngine;

namespace KerbVisionIR
{
    /// <summary>
    /// Applies color tint to skybox/background for night vision effect.
    /// Uses camera clear flags and background color.
    /// </summary>
    public class SkyboxColorCorrection
    {
        private Camera camera;
        private CameraClearFlags originalClearFlags;
        private Color originalBackgroundColor;
        private Material originalSkybox;
        private bool hasStoredOriginal = false;

        public void Initialize(Camera cam)
        {
            camera = cam;
            StoreOriginalSettings();
        }

        private void StoreOriginalSettings()
        {
            if (camera != null && !hasStoredOriginal)
            {
                originalClearFlags = camera.clearFlags;
                originalBackgroundColor = camera.backgroundColor;
                originalSkybox = RenderSettings.skybox;
                hasStoredOriginal = true;
                
                Debug.Log($"[KerbVisionIR] Stored skybox settings - ClearFlags: {originalClearFlags}, BG: {originalBackgroundColor}");
            }
        }

        public void ApplyTint(Color tintColor, float brightness)
        {
            if (camera == null) return;

            // For space scenes, the original background is usually black (0,0,0)
            // So instead of multiplying (which gives black), we use the tint color directly
            Color adjustedColor;
            
            if (originalBackgroundColor.r < 0.1f && originalBackgroundColor.g < 0.1f && originalBackgroundColor.b < 0.1f)
            {
                // Background is black/dark - use tint color directly with brightness
                adjustedColor = tintColor * brightness * 0.3f; // Scale down to keep space dark but tinted
            }
            else
            {
                // Background has color - blend with tint
                adjustedColor = originalBackgroundColor * tintColor * brightness;
            }
            
            camera.backgroundColor = adjustedColor;

            // Try to tint skybox material if it exists
            if (RenderSettings.skybox != null)
            {
                // Some skyboxes have _Tint property
                if (RenderSettings.skybox.HasProperty("_Tint"))
                {
                    RenderSettings.skybox.SetColor("_Tint", tintColor * brightness);
                }
                
                // Some have _Exposure
                if (RenderSettings.skybox.HasProperty("_Exposure"))
                {
                    RenderSettings.skybox.SetFloat("_Exposure", brightness);
                }
            }

            Debug.Log($"[KerbVisionIR] Applied skybox tint: {adjustedColor}, tintColor: {tintColor}, brightness: {brightness}");
        }

        public void RestoreOriginal()
        {
            if (camera != null && hasStoredOriginal)
            {
                camera.clearFlags = originalClearFlags;
                camera.backgroundColor = originalBackgroundColor;
                
                // Restore skybox
                if (RenderSettings.skybox != null && originalSkybox != null)
                {
                    if (RenderSettings.skybox.HasProperty("_Tint"))
                    {
                        RenderSettings.skybox.SetColor("_Tint", Color.white);
                    }
                    if (RenderSettings.skybox.HasProperty("_Exposure"))
                    {
                        RenderSettings.skybox.SetFloat("_Exposure", 1.0f);
                    }
                }

                Debug.Log("[KerbVisionIR] Restored original skybox settings");
            }
        }
    }
}

