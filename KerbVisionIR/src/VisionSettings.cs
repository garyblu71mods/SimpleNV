using UnityEngine;

namespace KerbVisionIR
{
    public enum VisionMode
    {
        Monochrome,
        GreenNV,
        AmberWarm
    }

    public class VisionSettings
    {
        public bool IsEnabled { get; set; }
        public VisionMode Mode { get; set; }
        public float Brightness { get; set; }
        public KeyCode ToggleKey { get; set; }
        public bool RequireAlt { get; set; }
        
        public float VignetteIntensity { get; set; }
        public float VignetteSmoothness { get; set; }
        public float ColorSaturation { get; set; }
        public float ColorContrast { get; set; }
        public bool GrainEnabled { get; set; }
        public float GrainIntensity { get; set; }

        public static VisionSettings CreateDefault()
        {
            return new VisionSettings
            {
                IsEnabled = false,
                Mode = VisionMode.GreenNV,
                Brightness = 1.5f,
                ToggleKey = KeyCode.BackQuote,
                RequireAlt = true,
                VignetteIntensity = 0.4f,
                VignetteSmoothness = 0.3f,
                ColorSaturation = -0.5f,
                ColorContrast = 0.2f,
                GrainEnabled = true,
                GrainIntensity = 0.3f
            };
        }

        public Color GetTintColor()
        {
            switch (Mode)
            {
                case VisionMode.Monochrome:
                    return Color.white;
                case VisionMode.GreenNV:
                    return new Color(0.2f, 1.0f, 0.2f, 1f);
                case VisionMode.AmberWarm:
                    return new Color(1.0f, 0.7f, 0.3f, 1f);
                default:
                    return Color.white;
            }
        }
    }
}
