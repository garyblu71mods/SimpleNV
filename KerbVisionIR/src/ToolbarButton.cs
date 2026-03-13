using UnityEngine;
using KSP.UI.Screens;

namespace KerbVisionIR
{
    public class ToolbarButton
    {
        private ApplicationLauncherButton button;
        private Texture2D buttonTexture;
        private System.Action onToggle;
        private System.Action onSettings;

        public void Initialize(System.Action onToggle, System.Action onSettings)
        {
            this.onToggle = onToggle;
            this.onSettings = onSettings;

            CreateTexture();
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
        }

        private void CreateTexture()
        {
            // Create simple green/white icon
            buttonTexture = new Texture2D(38, 38);
            Color[] pixels = new Color[38 * 38];
            
            for (int y = 0; y < 38; y++)
            {
                for (int x = 0; x < 38; x++)
                {
                    // Simple circle with NV green color
                    float dx = (x - 19f) / 19f;
                    float dy = (y - 19f) / 19f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    
                    if (dist < 0.9f)
                    {
                        // Green circle
                        pixels[y * 38 + x] = new Color(0.2f, 1.0f, 0.2f, 1f);
                    }
                    else
                    {
                        // Transparent
                        pixels[y * 38 + x] = new Color(0, 0, 0, 0);
                    }
                }
            }
            
            buttonTexture.SetPixels(pixels);
            buttonTexture.Apply();
        }

        private void OnGUIAppLauncherReady()
        {
            if (ApplicationLauncher.Ready && button == null)
            {
                button = ApplicationLauncher.Instance.AddModApplication(
                    OnButtonTrue,
                    OnButtonFalse,
                    null, null, null, null,
                    ApplicationLauncher.AppScenes.FLIGHT,
                    buttonTexture
                );
                
                Debug.Log("[KerbVisionIR] Toolbar button added");
            }
        }

        private void OnGUIAppLauncherDestroyed()
        {
            if (button != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(button);
                button = null;
            }
        }

        private void OnButtonTrue()
        {
            // Left click - toggle effect
            onToggle?.Invoke();
        }

        private void OnButtonFalse()
        {
            // Right click or second click - open settings
            onSettings?.Invoke();
        }

        public void Cleanup()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGUIAppLauncherDestroyed);
            
            if (button != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(button);
                button = null;
            }

            if (buttonTexture != null)
            {
                Object.Destroy(buttonTexture);
                buttonTexture = null;
            }
        }
    }
}
