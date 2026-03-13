using UnityEngine;

namespace KerbVisionIR
{
    public class VisionSettingsWindow
    {
        private bool isVisible = false;
        private Rect windowRect = new Rect(100, 100, 400, 500);
        private VisionSettings settings;
        private System.Action onSettingsChanged;

        private readonly int windowId = UnityEngine.Random.Range(1000, 10000);

        public bool IsVisible => isVisible;

        public VisionSettingsWindow(VisionSettings settings, System.Action onSettingsChanged)
        {
            this.settings = settings;
            this.onSettingsChanged = onSettingsChanged;
        }

        public void Toggle()
        {
            isVisible = !isVisible;
        }

        public void Show()
        {
            isVisible = true;
        }

        public void Hide()
        {
            isVisible = false;
        }

        public void OnGUI()
        {
            if (!isVisible)
                return;

            GUI.skin = HighLogic.Skin;
            windowRect = GUILayout.Window(windowId, windowRect, DrawWindow, "KerbVision IR Settings", GUILayout.MinWidth(400));
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginVertical();

            // Check if running in fallback mode (must be before first use)
            bool isFallbackMode = false;
            try
            {
                isFallbackMode = (PostProcessing.PostProcessResources.instance.uberShader == null);
            }
            catch { isFallbackMode = true; }

            GUILayout.Label("KerbVision IR - Night Vision", HighLogic.Skin.label);
            GUILayout.Label($"Version 1.0.0 - {(isFallbackMode ? "FALLBACK" : "FULL")} MODE", HighLogic.Skin.label);
            GUILayout.Space(5);

            if (isFallbackMode)
            {
                GUILayout.Label("⚠ FALLBACK MODE - Lighting Only", HighLogic.Skin.label);
                GUILayout.Label("Shader bundle not loaded - check installation", HighLogic.Skin.label);
                GUILayout.Space(5);
            }
            
            bool wasEnabled = settings.IsEnabled;
            settings.IsEnabled = GUILayout.Toggle(settings.IsEnabled, $"Effect Enabled: {(settings.IsEnabled ? "ON" : "OFF")}");
            if (wasEnabled != settings.IsEnabled)
            {
                Debug.Log($"[KerbVisionIR] GUI: Effect toggled to {settings.IsEnabled}");
                onSettingsChanged?.Invoke();
            }

            GUILayout.Space(10);

            GUILayout.Label("Vision Mode:", HighLogic.Skin.label);
            VisionMode oldMode = settings.Mode;
            if (GUILayout.Button(settings.Mode.ToString()))
            {
                settings.Mode = (VisionMode)(((int)settings.Mode + 1) % 3);
                onSettingsChanged?.Invoke();
            }

            GUILayout.Space(10);

            GUILayout.Label($"Brightness: {settings.Brightness:F2}", HighLogic.Skin.label);
            float newBrightness = GUILayout.HorizontalSlider(settings.Brightness, 0f, 2.0f);
            if (!Mathf.Approximately(newBrightness, settings.Brightness))
            {
                settings.Brightness = newBrightness;
                onSettingsChanged?.Invoke();
            }

            GUILayout.Space(10);

            // Show warning for disabled features in fallback mode
            if (isFallbackMode)
            {
                GUILayout.Label("━━━━━━━━━━━━━━━━━━━━━━━━━━━", HighLogic.Skin.label);
                GUILayout.Label("⚠ Following require shader bundle:", HighLogic.Skin.label);
                GUILayout.Label("(Copy kerbvision-pp.ssf to GameData/KerbVisionIR/Shaders/)", HighLogic.Skin.label);
                GUILayout.Space(5);
            }

            // Vignette (disabled in fallback)
            GUI.enabled = !isFallbackMode;
            GUILayout.Label($"Vignette Intensity: {settings.VignetteIntensity:F2} {(isFallbackMode ? "(DISABLED)" : "")}", HighLogic.Skin.label);
            float newVignetteIntensity = GUILayout.HorizontalSlider(settings.VignetteIntensity, 0f, 1.0f);
            if (!Mathf.Approximately(newVignetteIntensity, settings.VignetteIntensity) && !isFallbackMode)
            {
                settings.VignetteIntensity = newVignetteIntensity;
                onSettingsChanged?.Invoke();
            }

            GUILayout.Label($"Vignette Smoothness: {settings.VignetteSmoothness:F2} {(isFallbackMode ? "(DISABLED)" : "")}", HighLogic.Skin.label);
            float newVignetteSmoothness = GUILayout.HorizontalSlider(settings.VignetteSmoothness, 0.01f, 1.0f);
            if (!Mathf.Approximately(newVignetteSmoothness, settings.VignetteSmoothness) && !isFallbackMode)
            {
                settings.VignetteSmoothness = newVignetteSmoothness;
                onSettingsChanged?.Invoke();
            }

            GUILayout.Space(10);

            GUILayout.Label($"Saturation: {settings.ColorSaturation:F2} {(isFallbackMode ? "(DISABLED)" : "")}", HighLogic.Skin.label);
            float newSaturation = GUILayout.HorizontalSlider(settings.ColorSaturation, -1f, 1f);
            if (!Mathf.Approximately(newSaturation, settings.ColorSaturation) && !isFallbackMode)
            {
                settings.ColorSaturation = newSaturation;
                onSettingsChanged?.Invoke();
            }

            GUILayout.Label($"Contrast: {settings.ColorContrast:F2} {(isFallbackMode ? "(DISABLED)" : "")}", HighLogic.Skin.label);
            float newContrast = GUILayout.HorizontalSlider(settings.ColorContrast, -1f, 1f);
            if (!Mathf.Approximately(newContrast, settings.ColorContrast) && !isFallbackMode)
            {
                settings.ColorContrast = newContrast;
                onSettingsChanged?.Invoke();
            }

            GUILayout.Space(10);

            bool wasGrainEnabled = settings.GrainEnabled;
            settings.GrainEnabled = GUILayout.Toggle(settings.GrainEnabled && !isFallbackMode, $"Grain/Noise Effect {(isFallbackMode ? "(DISABLED)" : "")}");
            if (wasGrainEnabled != settings.GrainEnabled && !isFallbackMode)
                onSettingsChanged?.Invoke();

            if (settings.GrainEnabled && !isFallbackMode)
            {
                GUILayout.Label($"Grain Intensity: {settings.GrainIntensity:F2}", HighLogic.Skin.label);
                float newGrainIntensity = GUILayout.HorizontalSlider(settings.GrainIntensity, 0f, 1f);
                if (!Mathf.Approximately(newGrainIntensity, settings.GrainIntensity))
                {
                    settings.GrainIntensity = newGrainIntensity;
                    onSettingsChanged?.Invoke();
                }
            }
            
            GUI.enabled = true; // Re-enable GUI

            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset to Defaults"))
            {
                var defaults = VisionSettings.CreateDefault();
                settings.VignetteIntensity = defaults.VignetteIntensity;
                settings.VignetteSmoothness = defaults.VignetteSmoothness;
                settings.ColorSaturation = defaults.ColorSaturation;
                settings.ColorContrast = defaults.ColorContrast;
                settings.GrainEnabled = defaults.GrainEnabled;
                settings.GrainIntensity = defaults.GrainIntensity;
                settings.Brightness = defaults.Brightness;
                onSettingsChanged?.Invoke();
            }

            if (GUILayout.Button("Save & Close"))
            {
                VisionConfig.Save(settings);
                Hide();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.Label($"Hotkey: {(settings.RequireAlt ? "Alt + " : "")}{settings.ToggleKey}", HighLogic.Skin.label);

            GUILayout.EndVertical();

            GUI.DragWindow();
        }
    }
}
