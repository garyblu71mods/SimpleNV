using System;
using System.IO;
using UnityEngine;

namespace KerbVisionIR
{
    public static class VisionConfig
    {
        private static string ConfigPath => Path.Combine(KSPUtil.ApplicationRootPath, "GameData/KerbVisionIR/PluginData/settings.cfg");

        public static void Save(VisionSettings settings)
        {
            try
            {
                var configNode = new ConfigNode();
                var mainNode = configNode.AddNode("KERBVISIONIR_SETTINGS");

                mainNode.AddValue("IsEnabled", settings.IsEnabled);
                mainNode.AddValue("Mode", settings.Mode.ToString());
                mainNode.AddValue("Brightness", settings.Brightness);
                mainNode.AddValue("ToggleKey", settings.ToggleKey.ToString());
                mainNode.AddValue("RequireAlt", settings.RequireAlt);
                mainNode.AddValue("VignetteIntensity", settings.VignetteIntensity);
                mainNode.AddValue("VignetteSmoothness", settings.VignetteSmoothness);
                mainNode.AddValue("ColorSaturation", settings.ColorSaturation);
                mainNode.AddValue("ColorContrast", settings.ColorContrast);
                mainNode.AddValue("GrainEnabled", settings.GrainEnabled);
                mainNode.AddValue("GrainIntensity", settings.GrainIntensity);

                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
                configNode.Save(ConfigPath);

                Debug.Log("[KerbVisionIR] Settings saved to " + ConfigPath);
            }
            catch (Exception ex)
            {
                Debug.LogError("[KerbVisionIR] Failed to save settings: " + ex.Message);
            }
        }

        public static VisionSettings Load()
        {
            var settings = VisionSettings.CreateDefault();

            try
            {
                if (!File.Exists(ConfigPath))
                {
                    Debug.Log("[KerbVisionIR] No config file found, using defaults");
                    return settings;
                }

                var configNode = ConfigNode.Load(ConfigPath);
                if (configNode == null)
                {
                    Debug.LogWarning("[KerbVisionIR] Failed to load config file");
                    return settings;
                }

                var mainNode = configNode.GetNode("KERBVISIONIR_SETTINGS");
                if (mainNode == null)
                {
                    Debug.LogWarning("[KerbVisionIR] Config file missing main node");
                    return settings;
                }

                if (mainNode.HasValue("IsEnabled"))
                    settings.IsEnabled = bool.Parse(mainNode.GetValue("IsEnabled"));
                
                if (mainNode.HasValue("Mode"))
                    settings.Mode = (VisionMode)Enum.Parse(typeof(VisionMode), mainNode.GetValue("Mode"));
                
                if (mainNode.HasValue("Brightness"))
                    settings.Brightness = float.Parse(mainNode.GetValue("Brightness"));
                
                if (mainNode.HasValue("ToggleKey"))
                    settings.ToggleKey = (KeyCode)Enum.Parse(typeof(KeyCode), mainNode.GetValue("ToggleKey"));
                
                if (mainNode.HasValue("RequireAlt"))
                    settings.RequireAlt = bool.Parse(mainNode.GetValue("RequireAlt"));
                
                if (mainNode.HasValue("VignetteIntensity"))
                    settings.VignetteIntensity = float.Parse(mainNode.GetValue("VignetteIntensity"));
                
                if (mainNode.HasValue("VignetteSmoothness"))
                    settings.VignetteSmoothness = float.Parse(mainNode.GetValue("VignetteSmoothness"));
                
                if (mainNode.HasValue("ColorSaturation"))
                    settings.ColorSaturation = float.Parse(mainNode.GetValue("ColorSaturation"));
                
                if (mainNode.HasValue("ColorContrast"))
                    settings.ColorContrast = float.Parse(mainNode.GetValue("ColorContrast"));
                
                if (mainNode.HasValue("GrainEnabled"))
                    settings.GrainEnabled = bool.Parse(mainNode.GetValue("GrainEnabled"));
                
                if (mainNode.HasValue("GrainIntensity"))
                    settings.GrainIntensity = float.Parse(mainNode.GetValue("GrainIntensity"));

                Debug.Log("[KerbVisionIR] Settings loaded from " + ConfigPath);
            }
            catch (Exception ex)
            {
                Debug.LogError("[KerbVisionIR] Failed to load settings: " + ex.Message);
            }

            return settings;
        }
    }
}
