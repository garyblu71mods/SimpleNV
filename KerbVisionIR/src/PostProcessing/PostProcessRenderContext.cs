using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace KerbVisionIR.PostProcessing
{
    public sealed class PostProcessRenderContext
    {
        Camera m_Camera;

        public Camera camera
        {
            get { return m_Camera; }
            set
            {
                m_Camera = value;
                width = m_Camera.pixelWidth;
                height = m_Camera.pixelHeight;
                screenWidth = width;
                screenHeight = height;
                stereoActive = false;
                numberOfEyes = 1;
            }
        }

        public CommandBuffer command { get; set; }
        public RenderTargetIdentifier source { get; set; }
        public RenderTargetIdentifier destination { get; set; }
        public RenderTextureFormat sourceFormat { get; set; }
        public bool flip { get; set; }

        public PostProcessResources resources { get; internal set; }
        public PropertySheetFactory propertySheets { get; internal set; }
        public Dictionary<string, object> userData { get; private set; }

        public int width { get; private set; }
        public int height { get; private set; }
        public bool stereoActive { get; private set; }
        public int xrActiveEye { get; private set; }
        public int numberOfEyes { get; private set; }
        public int screenWidth { get; private set; }
        public int screenHeight { get; private set; }
        public bool isSceneView { get; internal set; }

        internal PropertySheet uberSheet;

        public void Reset()
        {
            m_Camera = null;
            width = 0;
            height = 0;
            stereoActive = false;
            xrActiveEye = 0;
            screenWidth = 0;
            screenHeight = 0;

            command = null;
            source = 0;
            destination = 0;
            sourceFormat = RenderTextureFormat.ARGB32;
            flip = false;

            resources = null;
            propertySheets = null;
            isSceneView = false;

            uberSheet = null;

            if (userData == null)
                userData = new Dictionary<string, object>();

            userData.Clear();
        }
    }
}
