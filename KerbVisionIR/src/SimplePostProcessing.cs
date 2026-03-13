using UnityEngine;

namespace KerbVisionIR
{
    /// <summary>
    /// Simple post-processing effects without TUFX dependency.
    /// Uses OnRenderImage with built-in Unity shaders.
    /// </summary>
    public class SimplePostProcessing : MonoBehaviour
    {
        private Material vignetteMataterial;
        private Material grayscaleMaterial;
        private VisionSettings settings;

        public void Initialize(VisionSettings settings)
        {
            this.settings = settings;
            CreateMaterials();
        }

        private void CreateMaterials()
        {
            // Vignette - use built-in shader
            Shader vignetteShader = Shader.Find("Hidden/Vignetting");
            if (vignetteShader != null)
            {
                vignetteMataterial = new Material(vignetteShader);
                Debug.Log("[KerbVisionIR] Vignette shader found and material created");
            }
            else
            {
                Debug.LogWarning("[KerbVisionIR] Vignette shader not found");
            }

            // Grayscale - use built-in shader
            Shader grayscaleShader = Shader.Find("Hidden/Grayscale Effect");
            if (grayscaleShader != null)
            {
                grayscaleMaterial = new Material(grayscaleShader);
                Debug.Log("[KerbVisionIR] Grayscale shader found and material created");
            }
            else
            {
                Debug.LogWarning("[KerbVisionIR] Grayscale shader not found");
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (settings == null || !settings.IsEnabled)
            {
                Graphics.Blit(source, destination);
                return;
            }

            RenderTexture temp1 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
            RenderTexture temp2 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

            // Start with source
            Graphics.Blit(source, temp1);

            // Apply vignette if available
            if (vignetteMataterial != null && settings.VignetteIntensity > 0.01f)
            {
                vignetteMataterial.SetFloat("_Intensity", settings.VignetteIntensity);
                vignetteMataterial.SetFloat("_Smoothness", settings.VignetteSmoothness);
                Graphics.Blit(temp1, temp2, vignetteMataterial);
                
                // Swap temps
                RenderTexture swap = temp1;
                temp1 = temp2;
                temp2 = swap;
            }

            // Apply grayscale/saturation if needed
            if (settings.ColorSaturation < -0.01f && grayscaleMaterial != null)
            {
                float amount = Mathf.Abs(settings.ColorSaturation);
                grayscaleMaterial.SetFloat("_Amount", amount);
                Graphics.Blit(temp1, temp2, grayscaleMaterial);
                
                // Swap temps
                RenderTexture swap = temp1;
                temp1 = temp2;
                temp2 = swap;
            }

            // Final output
            Graphics.Blit(temp1, destination);

            RenderTexture.ReleaseTemporary(temp1);
            RenderTexture.ReleaseTemporary(temp2);
        }

        void OnDestroy()
        {
            if (vignetteMataterial != null)
            {
                Destroy(vignetteMataterial);
            }
            if (grayscaleMaterial != null)
            {
                Destroy(grayscaleMaterial);
            }
        }
    }
}
