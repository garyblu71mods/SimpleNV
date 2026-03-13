using System;
using System.IO;
using UnityEngine;

namespace KerbVisionIR.PostProcessing
{
    public sealed class PostProcessResources : ScriptableObject
    {
        private static PostProcessResources s_Instance;

        public static PostProcessResources instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = CreateInstance<PostProcessResources>();
                    s_Instance.Init();
                }
                return s_Instance;
            }
        }

        public Shader uberShader;
        public Shader copyShader;

        private void Init()
        {
            string shaderPath = Path.Combine(KSPUtil.ApplicationRootPath, "GameData/KerbVisionIR/Shaders/kerbvision-pp.ssf");

            if (!File.Exists(shaderPath))
            {
                Debug.LogWarning("[KerbVisionIR] Shader bundle not found at: " + shaderPath);
                Debug.LogWarning("[KerbVisionIR] Will run in FALLBACK MODE (lighting only)");
                uberShader = null;
                copyShader = null;
                return;
            }

            try
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(shaderPath);
                
                if (bundle == null)
                {
                    Debug.LogError("[KerbVisionIR] Failed to load shader bundle");
                    uberShader = null;
                    copyShader = null;
                    return;
                }

                // DEBUG: List all shaders in bundle
                Debug.Log("[KerbVisionIR] Listing all shaders in bundle:");
                Shader[] allShaders = bundle.LoadAllAssets<Shader>();
                Debug.Log($"[KerbVisionIR] Found {allShaders.Length} shaders in bundle:");
                foreach (Shader shader in allShaders)
                {
                    Debug.Log($"[KerbVisionIR]   Shader: {shader.name}");
                }

                // Try to load from bundle - use LoadAllAssets and find by name
                uberShader = null;
                copyShader = null;
                
                foreach (Shader shader in allShaders)
                {
                    if (shader.name == "Hidden/KerbVision/Uber")
                    {
                        uberShader = shader;
                        Debug.Log($"[KerbVisionIR] Found Uber shader: {shader.name}");
                    }
                    else if (shader.name == "Hidden/KerbVision/Copy")
                    {
                        copyShader = shader;
                        Debug.Log($"[KerbVisionIR] Found Copy shader: {shader.name}");
                    }
                }

                // If not found, try old PostProcessing names (backwards compatibility)
                if (uberShader == null)
                {
                    Debug.LogWarning("[KerbVisionIR] KerbVision shaders not found, trying PostProcessing shaders...");
                    foreach (Shader shader in allShaders)
                    {
                        if (shader.name == "Hidden/PostProcessing/Uber")
                            uberShader = shader;
                        else if (shader.name == "Hidden/PostProcessing/Copy")
                            copyShader = shader;
                    }
                }

                // If not found in bundle, try Shader.Find (built-in)
                if (uberShader == null)
                {
                    Debug.LogWarning("[KerbVisionIR] Uber shader not in bundle, trying Shader.Find...");
                    uberShader = Shader.Find("Hidden/PostProcessing/Uber");
                }

                if (copyShader == null)
                {
                    Debug.LogWarning("[KerbVisionIR] Copy shader not in bundle, trying Shader.Find...");
                    copyShader = Shader.Find("Hidden/PostProcessing/Copy");
                }

                // Final fallback: use simple Unity shaders
                if (uberShader == null)
                {
                    Debug.LogWarning("[KerbVisionIR] PostProcessing shaders not found, trying fallback shaders...");
                    // Try to use any shader we can find from bundle
                    if (allShaders.Length > 0)
                    {
                        uberShader = allShaders[0];
                        Debug.LogWarning($"[KerbVisionIR] Using fallback shader: {uberShader.name}");
                    }
                }

                if (uberShader != null)
                {
                    Debug.Log($"[KerbVisionIR] Uber shader loaded: {uberShader.name}, Supported={uberShader.isSupported}");
                }
                else
                {
                    Debug.LogWarning("[KerbVisionIR] Failed to load Uber shader - no suitable shader found");
                }

                if (copyShader != null)
                {
                    Debug.Log($"[KerbVisionIR] Copy shader loaded: {copyShader.name}, Supported={copyShader.isSupported}");
                }
                else
                {
                    Debug.LogWarning("[KerbVisionIR] Failed to load Copy shader");
                }

                bundle.Unload(false);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[KerbVisionIR] Exception loading shader: " + ex.Message);
                uberShader = null;
                copyShader = null;
            }
        }
    }
}
