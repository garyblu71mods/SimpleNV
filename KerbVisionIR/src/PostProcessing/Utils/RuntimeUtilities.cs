using UnityEngine;

namespace KerbVisionIR.PostProcessing
{
    public static class RuntimeUtilities
    {
        static Texture2D m_WhiteTexture;

        public static Texture2D whiteTexture
        {
            get
            {
                if (m_WhiteTexture == null)
                {
                    m_WhiteTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false) { name = "White Texture" };
                    m_WhiteTexture.SetPixel(0, 0, Color.white);
                    m_WhiteTexture.Apply();
                }

                return m_WhiteTexture;
            }
        }

        static Texture2D m_BlackTexture;

        public static Texture2D blackTexture
        {
            get
            {
                if (m_BlackTexture == null)
                {
                    m_BlackTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false) { name = "Black Texture" };
                    m_BlackTexture.SetPixel(0, 0, Color.black);
                    m_BlackTexture.Apply();
                }

                return m_BlackTexture;
            }
        }

        public static void Destroy(Object obj)
        {
            if (obj != null)
                Object.DestroyImmediate(obj);
        }

        public static bool isFloatingPointFormat(RenderTextureFormat format)
        {
            return format == RenderTextureFormat.DefaultHDR ||
                   format == RenderTextureFormat.ARGBHalf ||
                   format == RenderTextureFormat.ARGBFloat ||
                   format == RenderTextureFormat.RGFloat ||
                   format == RenderTextureFormat.RGHalf ||
                   format == RenderTextureFormat.RFloat ||
                   format == RenderTextureFormat.RHalf ||
                   format == RenderTextureFormat.RGB111110Float;
        }
    }
}
