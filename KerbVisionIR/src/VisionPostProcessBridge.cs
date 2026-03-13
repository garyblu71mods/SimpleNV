using UnityEngine;
using KerbVisionIR.PostProcessing;

namespace KerbVisionIR
{
    public class VisionPostProcessBridge
    {
        private PostProcessProfile profile;
        private Vignette vignette;
        private ColorGrading colorGrading;
        private Grain grain;

        public PostProcessProfile CreateProfile(VisionSettings settings)
        {
            profile = ScriptableObject.CreateInstance<PostProcessProfile>();

            vignette = profile.AddSettings<Vignette>();
            vignette.enabled.value = true;
            vignette.mode.value = VignetteMode.Classic;
            vignette.color.value = Color.black;
            vignette.rounded.value = false;

            colorGrading = profile.AddSettings<ColorGrading>();
            colorGrading.enabled.value = true;

            grain = profile.AddSettings<Grain>();

            UpdateProfile(settings);

            return profile;
        }

        public void UpdateProfile(VisionSettings settings)
        {
            if (profile == null)
                return;

            vignette.intensity.value = settings.VignetteIntensity;
            vignette.smoothness.value = settings.VignetteSmoothness;

            colorGrading.saturation.value = settings.ColorSaturation;
            colorGrading.contrast.value = settings.ColorContrast;
            colorGrading.colorFilter.value = settings.GetTintColor();

            grain.enabled.value = settings.GrainEnabled;
            grain.intensity.value = settings.GrainIntensity;
        }
    }
}
