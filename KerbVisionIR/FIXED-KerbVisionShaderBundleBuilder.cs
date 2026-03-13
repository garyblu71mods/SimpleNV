using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class KerbVisionShaderBundleBuilder
{
    [MenuItem("Assets/Build KerbVision Shader Bundle")]
    static void BuildBundle()
    {
        Debug.Log("=== Building KerbVision Shader Bundle ===");
        
        // Find our custom shaders
        List<string> shaderPaths = new List<string>();
        
        // Look for shaders in Assets/Shaders
        string[] allShaders = AssetDatabase.FindAssets("t:Shader", new[] { "Assets/Shaders" });
        
        foreach (string guid in allShaders)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            
            if (shader != null && shader.name.StartsWith("Hidden/KerbVision/"))
            {
                shaderPaths.Add(path);
                Debug.Log($"? Adding shader: {shader.name} ({path})");
            }
        }
        
        if (shaderPaths.Count == 0)
        {
            Debug.LogError("? No KerbVision shaders found!");
            Debug.LogError("Make sure shaders are in Assets/Shaders folder");
            Debug.LogError("And shader names start with 'Hidden/KerbVision/'");
            return;
        }
        
        Debug.Log($"Found {shaderPaths.Count} shaders to bundle");
        
        // Create AssetBundle build
        AssetBundleBuild[] builds = new AssetBundleBuild[1];
        builds[0].assetBundleName = "kerbvision-pp.ssf";
        builds[0].assetNames = shaderPaths.ToArray();
        
        // Create output directory
        string outputPath = Application.dataPath + "/../AssetBundles";
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        
        Debug.Log($"Building to: {outputPath}");
        
        // Build for Windows x64 with CHUNK-BASED compression (KSP compatible!)
        BuildPipeline.BuildAssetBundles(
            outputPath,
            builds,
            BuildAssetBundleOptions.ChunkBasedCompression,  // Changed from Uncompressed!
            BuildTarget.StandaloneWindows64
        );
        
        // Verify output
        string bundlePath = Path.Combine(outputPath, "kerbvision-pp.ssf");
        if (File.Exists(bundlePath))
        {
            FileInfo fi = new FileInfo(bundlePath);
            Debug.Log($"? Bundle built successfully!");
            Debug.Log($"   Location: {bundlePath}");
            Debug.Log($"   Size: {fi.Length / 1024} KB");
            Debug.Log($"   Shaders: {shaderPaths.Count}");
            
            // List what's in the bundle
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle != null)
            {
                Debug.Log($"\n=== Bundle Contents ===");
                Shader[] loadedShaders = bundle.LoadAllAssets<Shader>();
                foreach (Shader shader in loadedShaders)
                {
                    Debug.Log($"  - {shader.name}");
                }
                bundle.Unload(true);
            }
            else
            {
                Debug.LogError("? Bundle verification failed - could not load!");
            }
            
            // Open folder
            EditorUtility.RevealInFinder(bundlePath);
        }
        else
        {
            Debug.LogError($"? Bundle build failed! File not found: {bundlePath}");
        }
        
        Debug.Log("=== Build Complete ===");
    }
    
    [MenuItem("Assets/Test Load KerbVision Bundle")]
    static void TestLoadBundle()
    {
        string bundlePath = Application.dataPath + "/../AssetBundles/kerbvision-pp.ssf";
        
        if (!File.Exists(bundlePath))
        {
            Debug.LogError($"Bundle not found: {bundlePath}");
            return;
        }
        
        Debug.Log("=== Testing Bundle Load ===");
        
        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
        if (bundle == null)
        {
            Debug.LogError("Failed to load bundle!");
            return;
        }
        
        Debug.Log("? Bundle loaded successfully");
        
        // Load ALL shaders from bundle
        Shader[] allShaders = bundle.LoadAllAssets<Shader>();
        Debug.Log($"Found {allShaders.Length} shaders in bundle:");
        foreach (Shader shader in allShaders)
        {
            Debug.Log($"  Shader: {shader.name}");
        }
        
        // Find by shader name
        Shader uberShader = null;
        Shader copyShader = null;
        
        foreach (Shader shader in allShaders)
        {
            if (shader.name == "Hidden/KerbVision/Uber")
                uberShader = shader;
            else if (shader.name == "Hidden/KerbVision/Copy")
                copyShader = shader;
        }
        
        // Report results
        if (uberShader != null)
        {
            Debug.Log($"? Uber shader loaded: {uberShader.name}");
            Debug.Log($"  Supported: {uberShader.isSupported}");
        }
        else
        {
            Debug.LogError("? Failed to load Uber shader");
        }
        
        if (copyShader != null)
        {
            Debug.Log($"? Copy shader loaded: {copyShader.name}");
            Debug.Log($"  Supported: {copyShader.isSupported}");
        }
        else
        {
            Debug.LogError("? Failed to load Copy shader");
        }
        
        // List all assets
        string[] allAssets = bundle.GetAllAssetNames();
        Debug.Log($"\n=== All Assets ({allAssets.Length}) ===");
        foreach (string asset in allAssets)
        {
            Debug.Log($"  - {asset}");
        }
        
        bundle.Unload(true);
        Debug.Log("=== Test Complete ===");
    }
}
