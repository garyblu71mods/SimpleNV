using System;
using UnityEngine;

namespace KerbVisionIR.PostProcessing
{
    [Serializable]
    public sealed class ColorGrading : PostProcessEffectSettings
    {
        public FloatParameter saturation = new FloatParameter { value = 0f };
        public FloatParameter contrast = new FloatParameter { value = 0f };
        public ColorParameter colorFilter = new ColorParameter { value = Color.white };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    internal sealed class ColorGradingRenderer : PostProcessEffectRenderer<ColorGrading>
    {
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.uberSheet;
            sheet.EnableKeyword("COLOR_GRADING");
            
            sheet.properties.SetFloat(ShaderIDs.ColorGrading_Saturation, settings.saturation.value + 1f);
            sheet.properties.SetFloat(ShaderIDs.ColorGrading_Contrast, settings.contrast.value + 1f);
            sheet.properties.SetColor(ShaderIDs.ColorGrading_ColorFilter, settings.colorFilter.value);
        }
    }
}
