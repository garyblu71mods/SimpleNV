using System;
using UnityEngine;

namespace KerbVisionIR.PostProcessing
{
    [Serializable]
    public sealed class Grain : PostProcessEffectSettings
    {
        public BoolParameter colored = new BoolParameter { value = true };
        public FloatParameter intensity = new FloatParameter { value = 0f };
        public FloatParameter size = new FloatParameter { value = 1f };
        public FloatParameter lumContrib = new FloatParameter { value = 0.8f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && intensity.value > 0f;
        }
    }

    internal sealed class GrainRenderer : PostProcessEffectRenderer<Grain>
    {
        RenderTexture m_GrainLookupRT;
        const int k_SampleCount = 1024;
        int m_SampleIndex;

        public override void Render(PostProcessRenderContext context)
        {
            float time = Time.realtimeSinceStartup;
            float rndOffsetX = 0f;
            float rndOffsetY = 0f;

            if (++m_SampleIndex >= k_SampleCount)
                m_SampleIndex = 0;

            if (m_GrainLookupRT == null || !m_GrainLookupRT.IsCreated())
            {
                RuntimeUtilities.Destroy(m_GrainLookupRT);

                m_GrainLookupRT = new RenderTexture(128, 128, 0, RenderTextureFormat.ARGB32)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Repeat,
                    anisoLevel = 0,
                    name = "Grain Lookup Texture"
                };

                m_GrainLookupRT.Create();
            }

            var uberSheet = context.uberSheet;
            uberSheet.EnableKeyword("GRAIN");
            uberSheet.properties.SetTexture(ShaderIDs.GrainTex, m_GrainLookupRT);
            uberSheet.properties.SetVector(ShaderIDs.Grain_Params1, new Vector2(settings.lumContrib.value, settings.intensity.value * 20f));
            uberSheet.properties.SetVector(ShaderIDs.Grain_Params2, new Vector4((float)context.width / (float)m_GrainLookupRT.width / settings.size.value, (float)context.height / (float)m_GrainLookupRT.height / settings.size.value, rndOffsetX, rndOffsetY));
        }

        public override void Release()
        {
            RuntimeUtilities.Destroy(m_GrainLookupRT);
            m_GrainLookupRT = null;
            m_SampleIndex = 0;
        }
    }
}
