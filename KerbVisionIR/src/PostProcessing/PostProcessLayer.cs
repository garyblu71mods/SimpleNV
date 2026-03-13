using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace KerbVisionIR.PostProcessing
{
    [RequireComponent(typeof(Camera))]
    public sealed class PostProcessLayer : MonoBehaviour
    {
        public Camera camera { get; private set; }
        public PostProcessProfile sharedProfile;

        private PostProcessRenderContext m_Context;
        private PropertySheetFactory m_PropertySheetFactory;
        private CommandBuffer m_CommandBuffer;
        
        private Dictionary<Type, PostProcessEffectRenderer> m_Renderers;

        void OnEnable()
        {
            camera = GetComponent<Camera>();

            m_Context = new PostProcessRenderContext();
            m_PropertySheetFactory = new PropertySheetFactory();
            m_CommandBuffer = new CommandBuffer { name = "KerbVision Post-processing" };
            
            m_Renderers = new Dictionary<System.Type, PostProcessEffectRenderer>
            {
                { typeof(Vignette), new VignetteRenderer() },
                { typeof(ColorGrading), new ColorGradingRenderer() },
                { typeof(Grain), new GrainRenderer() }
            };

            foreach (var renderer in m_Renderers.Values)
                renderer.Init();

            camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, m_CommandBuffer);
        }

        void OnDisable()
        {
            if (m_CommandBuffer != null)
            {
                camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, m_CommandBuffer);
                m_CommandBuffer.Release();
            }

            if (m_PropertySheetFactory != null)
                m_PropertySheetFactory.Release();

            foreach (var renderer in m_Renderers.Values)
                renderer.Release();
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (sharedProfile == null || PostProcessResources.instance.uberShader == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            m_Context.Reset();
            m_Context.camera = camera;
            m_Context.source = source;
            m_Context.destination = destination;
            m_Context.sourceFormat = source.format;
            m_Context.command = m_CommandBuffer;
            m_Context.resources = PostProcessResources.instance;
            m_Context.propertySheets = m_PropertySheetFactory;

            m_CommandBuffer.Clear();
            m_CommandBuffer.BeginSample("KerbVision Post-processing");

            var uberSheet = m_PropertySheetFactory.Get(PostProcessResources.instance.uberShader);
            m_Context.uberSheet = uberSheet;
            uberSheet.ClearKeywords();
            uberSheet.properties.Clear();

            RenderTexture tempRT = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
            
            m_CommandBuffer.Blit(source, tempRT);

            foreach (var setting in sharedProfile.settings)
            {
                if (!setting.active || !setting.IsEnabledAndSupported(m_Context))
                    continue;

                var renderer = m_Renderers[setting.GetType()];
                renderer.SetSettings(setting);
                renderer.Render(m_Context);
            }

            m_CommandBuffer.BlitFullscreenTriangle(tempRT, destination, uberSheet, 0);
            m_CommandBuffer.EndSample("KerbVision Post-processing");

            RenderTexture.ReleaseTemporary(tempRT);

            Graphics.ExecuteCommandBuffer(m_CommandBuffer);
        }
    }

    public static class CommandBufferExtensions
    {
        public static void BlitFullscreenTriangle(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, PropertySheet propertySheet, int pass)
        {
            cmd.SetGlobalTexture("_MainTex", source);
            cmd.SetRenderTarget(destination);
            cmd.DrawProcedural(Matrix4x4.identity, propertySheet.material, pass, MeshTopology.Triangles, 3, 1, propertySheet.properties);
        }
    }
}
