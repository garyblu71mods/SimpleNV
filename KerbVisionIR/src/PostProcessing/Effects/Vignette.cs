using System;
using UnityEngine;

namespace KerbVisionIR.PostProcessing
{
    public enum VignetteMode
    {
        Classic,
        Masked
    }

    [Serializable]
    public sealed class VignetteModeParameter : ParameterOverride<VignetteMode> {}

    [Serializable]
    public sealed class TextureParameter : ParameterOverride<Texture> {}

    [Serializable]
    public sealed class Vignette : PostProcessEffectSettings
    {
        public VignetteModeParameter mode = new VignetteModeParameter { value = VignetteMode.Classic };
        public ColorParameter color = new ColorParameter { value = new Color(0f, 0f, 0f, 1f) };
        public Vector2Parameter center = new Vector2Parameter { value = new Vector2(0.5f, 0.5f) };
        public FloatParameter intensity = new FloatParameter { value = 0f };
        public FloatParameter smoothness = new FloatParameter { value = 0.2f };
        public FloatParameter roundness = new FloatParameter { value = 1f };
        public BoolParameter rounded = new BoolParameter { value = false };
        public TextureParameter mask = new TextureParameter { value = null };
        public FloatParameter opacity = new FloatParameter { value = 1f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value
                && ((mode.value == VignetteMode.Classic && intensity.value > 0f)
                    ||  (mode.value == VignetteMode.Masked && opacity.value > 0f && mask.value != null));
        }
    }

    internal sealed class VignetteRenderer : PostProcessEffectRenderer<Vignette>
    {
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.uberSheet;
            sheet.EnableKeyword("VIGNETTE");
            sheet.properties.SetColor(ShaderIDs.Vignette_Color, settings.color.value);

            if (settings.mode == VignetteMode.Classic)
            {
                sheet.properties.SetFloat(ShaderIDs.Vignette_Mode, 0f);
                sheet.properties.SetVector(ShaderIDs.Vignette_Center, settings.center.value);
                float roundness = (1f - settings.roundness.value) * 6f + settings.roundness.value;
                sheet.properties.SetVector(ShaderIDs.Vignette_Settings, new Vector4(settings.intensity.value * 3f, settings.smoothness.value * 5f, roundness, settings.rounded.value ? 1f : 0f));
            }
            else
            {
                sheet.properties.SetFloat(ShaderIDs.Vignette_Mode, 1f);
                sheet.properties.SetTexture(ShaderIDs.Vignette_Mask, settings.mask.value);
                sheet.properties.SetFloat(ShaderIDs.Vignette_Opacity, Mathf.Clamp01(settings.opacity.value));
            }
        }
    }

    internal static class ShaderIDs
    {
        internal static readonly int Vignette_Color = Shader.PropertyToID("_Vignette_Color");
        internal static readonly int Vignette_Mode = Shader.PropertyToID("_Vignette_Mode");
        internal static readonly int Vignette_Center = Shader.PropertyToID("_Vignette_Center");
        internal static readonly int Vignette_Settings = Shader.PropertyToID("_Vignette_Settings");
        internal static readonly int Vignette_Mask = Shader.PropertyToID("_Vignette_Mask");
        internal static readonly int Vignette_Opacity = Shader.PropertyToID("_Vignette_Opacity");
        internal static readonly int ColorGrading_Saturation = Shader.PropertyToID("_ColorGrading_Saturation");
        internal static readonly int ColorGrading_Contrast = Shader.PropertyToID("_ColorGrading_Contrast");
        internal static readonly int ColorGrading_ColorFilter = Shader.PropertyToID("_ColorGrading_ColorFilter");
        internal static readonly int Grain_Intensity = Shader.PropertyToID("_Grain_Intensity");
        internal static readonly int Grain_Response = Shader.PropertyToID("_Grain_Response");
        internal static readonly int GrainTex = Shader.PropertyToID("_GrainTex");
        internal static readonly int Grain_Params1 = Shader.PropertyToID("_Grain_Params1");
        internal static readonly int Grain_Params2 = Shader.PropertyToID("_Grain_Params2");
    }
}
