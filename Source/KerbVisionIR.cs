using System;
using System.Collections.Generic;
using System.IO;
using KSP.UI.Screens;
using ToolbarControl_NS;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Reflection;

namespace KerbVisionIR
{
    /// <summary>
    /// Main controller for KerbVisionIR night vision mod
    /// Hotkey: Alt + ` (backtick) to toggle
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbVisionIRController : MonoBehaviour
    {
        // Singleton instance
        public static KerbVisionIRController Instance { get; private set; }

        // PostProcessing components
        private PostProcessLayer layer;
        private PostProcessVolume volume;
        private PostProcessVolume tintVolume;
        private PostProcessProfile profile;
        private PostProcessProfile tintProfile;
        private Camera targetCamera;
        private bool postProcessingAvailable = false;
        private PostProcessResources runtimeResources;

        private readonly Dictionary<string, Shader> loadedShaders = new Dictionary<string, Shader>();
        private readonly Dictionary<string, ComputeShader> loadedComputeShaders = new Dictionary<string, ComputeShader>();

        // Effect references
        private Vignette vignette;
        private ColorGrading colorGrading;
        private ColorGrading tintColorGrading;
        private Grain grain;

        // Audio
        private AudioSource audioSource;
        private AudioClip nvOnClip;

        // State variables
        private bool isEffectActive = false;
        private VisionMode currentMode = VisionMode.GreenNV;
        private float brightnessMultiplier = 1.8f;
        private const float MinBrightnessMultiplier = 1.0f;
        private const float MaxBrightnessMultiplier = 4.0f;
        private const float FallbackTintStrength = 0.85f;
        private bool vignetteEnabled = true;
        private bool grainEnabled = false;
        private bool scanlinesEnabled = true;
        private float vignetteIntensity = 0.45f;
        private float grainIntensity = 0.12f;
        private float scanlineIntensity = 0.18f;
        private float colorTintStrength = 1.0f;
        private const float MinVignetteIntensity = 0f;
        private const float MaxVignetteIntensity = 0.8f;
        private const float MinGrainIntensity = 0f;
        private const float MaxGrainIntensity = 1.0f;
        private const float MinScanlineIntensity = 0f;
        private const float MaxScanlineIntensity = 0.45f;
        private const float MinColorTintStrength = 0f;
        private const float MaxColorTintStrength = 1f;
        private const float MinPersistentColorTint = 0.35f;
        private float lastToggleTime = -10f;
        private const float ToggleCooldownSeconds = 0.8f;
        private bool transitionActive = false;
        private bool transitionEnabling = false;
        private float transitionTimer = 0f;
        private float transitionBlend = 1f;
        private float transitionOverlayAlpha = 0f;
        private float transitionNoiseBoost = 0f;
        private const float NvOnBlackoutDuration = 0.10f;
        private const float NvOnFadeDuration = 0.30f;
        private const float NvOffFadeDuration = 0.18f;
        private const float NvOffBlackoutDuration = 0.08f;
        private Texture2D fallbackWhiteTexture;
        private Texture2D fallbackGrainTexture;
        private Texture2D fallbackVignetteTexture;
        private Texture2D fallbackScanlineTexture;
        private int fallbackGrainFrame;
        private Material cameraTintMaterial;
        private Material cameraScanlineMaterial;
        private int cameraOverlayRenderedFrame = -1;
        private float cameraOverlayLastSeenTime = -100f;
        private bool cameraOverlayLogged = false;
        private bool guiUnderlayLogged = false;

        // Toolbar
        private const string ToolbarModId = "KerbVisionIR";
        private const int EffectLayer = 31;
        private ToolbarControl toolbarControl;
        private bool showWindow = false;
        private bool toolbarReady = false;

        // GUI / binding
        private Rect guiWindowRect = new Rect(10, 10, 300, 280);
        private bool capturingKey = false;
        private KeyCode boundToggleKey = KeyCode.BackQuote;
        private bool requireAltForBoundKey = true;
        private bool toggleKeyLatched = false;

        // Original lighting values (for restoration)
        private Color storedAmbientLight;
        private float storedAmbientIntensity;
        private bool lightingStored = false;

        // Vision modes
        public enum VisionMode
        {
            Monochrome,   // Black & white
            GreenNV,      // Classic green night vision
            AmberWarm     // Amber/orange thermal-like
        }

        #region Unity Lifecycle

        void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            fallbackWhiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            fallbackWhiteTexture.SetPixel(0, 0, Color.white);
            fallbackWhiteTexture.Apply();
            CreateFallbackGrainTexture();
            CreateFallbackVignetteTexture();
            CreateFallbackScanlineTexture();
            InitializeCameraOverlayMaterials();
            
            Debug.Log("[KerbVisionIR] Controller created");
        }

        void Start()
        {
            try
            {
                InitializePostProcessing();
                Debug.Log("[KerbVisionIR] Initialization complete - Press Alt+` to toggle");
                // Load persisted binding
                boundToggleKey = (KeyCode)PlayerPrefs.GetInt("KerbVisionIR_BoundToggleKey", (int)KeyCode.BackQuote);
                requireAltForBoundKey = PlayerPrefs.GetInt("KerbVisionIR_RequireAlt", 1) == 1;
                brightnessMultiplier = PlayerPrefs.GetFloat("KerbVisionIR_Brightness", 1.8f);
                brightnessMultiplier = Mathf.Clamp(brightnessMultiplier, MinBrightnessMultiplier, MaxBrightnessMultiplier);
                currentMode = VisionMode.GreenNV;
                vignetteEnabled = PlayerPrefs.GetInt("KerbVisionIR_VignetteEnabled", 1) == 1;
                grainEnabled = PlayerPrefs.GetInt("KerbVisionIR_GrainEnabled", 0) == 1;
                vignetteIntensity = Mathf.Clamp(PlayerPrefs.GetFloat("KerbVisionIR_VignetteIntensity", 0.45f), MinVignetteIntensity, MaxVignetteIntensity);
                grainIntensity = Mathf.Clamp(PlayerPrefs.GetFloat("KerbVisionIR_GrainIntensity", 0.12f), MinGrainIntensity, MaxGrainIntensity);
                scanlinesEnabled = PlayerPrefs.GetInt("KerbVisionIR_ScanlinesEnabled", 1) == 1;
                scanlineIntensity = Mathf.Clamp(PlayerPrefs.GetFloat("KerbVisionIR_ScanlineIntensity", 0.18f), MinScanlineIntensity, MaxScanlineIntensity);
                colorTintStrength = Mathf.Clamp(PlayerPrefs.GetFloat("KerbVisionIR_ColorTintStrength", 1.0f), MinColorTintStrength, MaxColorTintStrength);

                InitializeAudio();
                InitializeToolbar();
                toolbarReady = true;
                Camera.onPostRender += OnCameraPostRender;
             }
             catch (Exception ex)
             {
                 Debug.LogError($"[KerbVisionIR] Initialization failed: {ex.Message}\n{ex.StackTrace}");
             }
        }

        void Update()
        {
            if (MapView.MapIsEnabled)
            {
                if (isEffectActive || transitionActive)
                {
                    transitionActive = false;
                    transitionBlend = 0f;
                    transitionOverlayAlpha = 0f;
                    transitionNoiseBoost = 0f;
                    isEffectActive = false;
                    DisableEffect();
                }
                return;
            }

            if (transitionActive)
            {
                UpdateTransition(Time.unscaledDeltaTime);
            }

            if (postProcessingAvailable)
            {
                EnsureActiveCameraLayer();
            }

            // Hotkey detection: user-configurable
            bool boundKeyPressed = !capturingKey && boundToggleKey != KeyCode.None && Input.GetKey(boundToggleKey);
            bool altPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            bool comboPressed = boundKeyPressed && (!requireAltForBoundKey || altPressed);

            if (comboPressed)
            {
                if (!toggleKeyLatched)
                {
                    TryToggleNightVision();
                    toggleKeyLatched = true;
                }
            }
            else
            {
                toggleKeyLatched = false;
            }

            // If we are capturing a new binding, detect any key pressed
            if (capturingKey && Input.anyKeyDown)
            {
                foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(kcode))
                    {
                        boundToggleKey = kcode;
                        capturingKey = false;
                        PlayerPrefs.SetInt("KerbVisionIR_BoundToggleKey", (int)boundToggleKey);
                        PlayerPrefs.Save();
                        Debug.Log($"[KerbVisionIR] Bound toggle key to: {boundToggleKey}");
                        break;
                    }
                }
            }

            if (isEffectActive)
            {
                if (postProcessingAvailable)
                {
                    EnforceActiveEffectState();
                }

                if (grainEnabled && Time.frameCount - fallbackGrainFrame > 3)
                {
                    CreateFallbackGrainTexture();
                }

                ApplyBrightnessBoost();
            }
        }

        void OnGUI()
        {
            DrawTransitionOverlay();

            if (isEffectActive && !postProcessingAvailable)
            {
                DrawFallbackEffectOverlay();
            }

            if (showWindow)
            {
                guiWindowRect = GUILayout.Window(10101, guiWindowRect, GuiWindow, "KerbVisionIR");
            }
        }

        void DrawColorUnderlay()
        {
            if (fallbackWhiteTexture == null || !isEffectActive)
                return;

            int prevDepth = GUI.depth;
            Color prevColor = GUI.color;

            GUI.depth = 9999;

            float effectBlend = GetEffectBlend();

            if (grainEnabled && fallbackGrainTexture != null)
            {
                float effectiveGrain = Mathf.Clamp(grainIntensity, MinGrainIntensity, MaxGrainIntensity);
                float grainAlpha = Mathf.Clamp01(effectiveGrain * (0.9f + transitionNoiseBoost * 0.35f) * effectBlend);
                if (grainAlpha > 0.001f)
                {
                    GUI.color = new Color(1f, 1f, 1f, grainAlpha);
                    float u = Screen.width / 96f;
                    float v = Screen.height / 96f;
                    GUI.DrawTextureWithTexCoords(new Rect(0f, 0f, Screen.width, Screen.height), fallbackGrainTexture, new Rect(0f, 0f, u, v), true);
                }
            }

            GUI.color = prevColor;
            GUI.depth = prevDepth;
        }

        void GuiWindow(int id)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Mod: KerbVisionIR");

            GUILayout.Space(8);

            // Toggle button
            if (GUILayout.Button(isEffectActive ? "Disable Night Vision" : "Enable Night Vision", GUILayout.Height(30)))
            {
                TryToggleNightVision();
            }

            GUILayout.Space(6);

            GUILayout.Space(4);
            GUILayout.Label($"Brightness: {brightnessMultiplier:0.00}");
            float newBrightness = GUILayout.HorizontalSlider(brightnessMultiplier, MinBrightnessMultiplier, MaxBrightnessMultiplier);
            if (Mathf.Abs(newBrightness - brightnessMultiplier) > 0.001f)
            {
                brightnessMultiplier = newBrightness;
                PlayerPrefs.SetFloat("KerbVisionIR_Brightness", brightnessMultiplier);
                PlayerPrefs.Save();

                if (isEffectActive)
                {
                    ApplyBrightnessBoost();
                }
            }

            GUILayout.Space(4);
            if (GUILayout.Button($"Vignette: {(vignetteEnabled ? "ON" : "OFF")}", GUILayout.Height(24), GUILayout.ExpandWidth(true)))
            {
                vignetteEnabled = !vignetteEnabled;
                PlayerPrefs.SetInt("KerbVisionIR_VignetteEnabled", vignetteEnabled ? 1 : 0);
                PlayerPrefs.Save();
                if (isEffectActive)
                    EnforceActiveEffectState();
                Debug.Log($"[KerbVisionIR] Vignette toggled: {(vignetteEnabled ? "ON" : "OFF")}");
            }

            if (GUILayout.Button($"Grain: {(grainEnabled ? "ON" : "OFF")}", GUILayout.Height(24), GUILayout.ExpandWidth(true)))
            {
                grainEnabled = !grainEnabled;
                PlayerPrefs.SetInt("KerbVisionIR_GrainEnabled", grainEnabled ? 1 : 0);
                PlayerPrefs.Save();
                if (isEffectActive)
                    EnforceActiveEffectState();
                Debug.Log($"[KerbVisionIR] Grain toggled: {(grainEnabled ? "ON" : "OFF")}");
            }

            if (GUILayout.Button($"Scanlines: {(scanlinesEnabled ? "ON" : "OFF")}", GUILayout.Height(24), GUILayout.ExpandWidth(true)))
            {
                scanlinesEnabled = !scanlinesEnabled;
                PlayerPrefs.SetInt("KerbVisionIR_ScanlinesEnabled", scanlinesEnabled ? 1 : 0);
                PlayerPrefs.Save();
            }

            GUILayout.Label($"Vignette Strength: {vignetteIntensity:0.00}");
            float newVignetteIntensity = GUILayout.HorizontalSlider(vignetteIntensity, MinVignetteIntensity, MaxVignetteIntensity);
            if (Mathf.Abs(newVignetteIntensity - vignetteIntensity) > 0.001f)
            {
                vignetteIntensity = newVignetteIntensity;
                PlayerPrefs.SetFloat("KerbVisionIR_VignetteIntensity", vignetteIntensity);
                PlayerPrefs.Save();
                if (isEffectActive)
                    EnforceActiveEffectState();
            }

            GUILayout.Label($"Grain Strength: {grainIntensity:0.00}");
            float newGrainIntensity = GUILayout.HorizontalSlider(grainIntensity, MinGrainIntensity, MaxGrainIntensity);
            if (Mathf.Abs(newGrainIntensity - grainIntensity) > 0.001f)
            {
                grainIntensity = newGrainIntensity;
                PlayerPrefs.SetFloat("KerbVisionIR_GrainIntensity", grainIntensity);
                PlayerPrefs.Save();
                if (isEffectActive)
                    EnforceActiveEffectState();
            }

            GUILayout.Label($"Scanline Strength: {scanlineIntensity:0.00}");
            float newScanlineIntensity = GUILayout.HorizontalSlider(scanlineIntensity, MinScanlineIntensity, MaxScanlineIntensity);
            if (Mathf.Abs(newScanlineIntensity - scanlineIntensity) > 0.001f)
            {
                scanlineIntensity = newScanlineIntensity;
                PlayerPrefs.SetFloat("KerbVisionIR_ScanlineIntensity", scanlineIntensity);
                PlayerPrefs.Save();
            }

            GUILayout.Label($"Green Tint Strength: {colorTintStrength:0.00}");
            float newColorTintStrength = GUILayout.HorizontalSlider(colorTintStrength, MinColorTintStrength, MaxColorTintStrength);
            if (Mathf.Abs(newColorTintStrength - colorTintStrength) > 0.001f)
            {
                colorTintStrength = newColorTintStrength;
                PlayerPrefs.SetFloat("KerbVisionIR_ColorTintStrength", colorTintStrength);
                PlayerPrefs.Save();
            }

            GUILayout.Space(6);

            // Binding area
            GUILayout.Label($"Toggle key: {boundToggleKey} {(requireAltForBoundKey ? "(requires Alt)" : "")}");
            GUILayout.BeginHorizontal();
            if (capturingKey)
            {
                if (GUILayout.Button("Press any key...", GUILayout.Height(24))){ /* noop while capturing */ }
            }
            else
            {
                if (GUILayout.Button("Bind Toggle Key", GUILayout.Height(24)))
                {
                    capturingKey = true;
                }
            }

            bool newRequireAlt = GUILayout.Toggle(requireAltForBoundKey, "Require Alt", GUILayout.Height(24));
            if (newRequireAlt != requireAltForBoundKey)
            {
                requireAltForBoundKey = newRequireAlt;
                PlayerPrefs.SetInt("KerbVisionIR_RequireAlt", requireAltForBoundKey ? 1 : 0);
                PlayerPrefs.Save();
            }
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            var footerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.LowerRight
            };

            try
            {
                var asm = Assembly.GetExecutingAssembly();
                var version = asm.GetName().Version;
                var asmPath = asm.Location;
                var lastWrite = File.GetLastWriteTime(asmPath);
                GUILayout.Label($"v{version}  {lastWrite:yyyy-MM-dd HH:mm}", footerStyle);
            }
            catch
            {
                GUILayout.Label("v?  date unknown", footerStyle);
            }

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        void OnDestroy()
        {
            Debug.Log("[KerbVisionIR] Cleanup started");

            // Disable effect before cleanup
            if (isEffectActive)
                DisableEffect();

            Camera.onPostRender -= OnCameraPostRender;

            // Destroy PostProcessing objects
            if (volume != null)
                Destroy(volume.gameObject);

            if (tintVolume != null)
                Destroy(tintVolume.gameObject);

            if (profile != null)
                Destroy(profile);

            if (tintProfile != null)
                Destroy(tintProfile);

            if (audioSource != null)
                Destroy(audioSource);

            if (fallbackWhiteTexture != null)
                Destroy(fallbackWhiteTexture);

            if (fallbackGrainTexture != null)
                Destroy(fallbackGrainTexture);

            if (fallbackVignetteTexture != null)
                Destroy(fallbackVignetteTexture);

            if (fallbackScanlineTexture != null)
                Destroy(fallbackScanlineTexture);

            if (cameraTintMaterial != null)
                Destroy(cameraTintMaterial);

            if (cameraScanlineMaterial != null)
                Destroy(cameraScanlineMaterial);

            if (toolbarControl != null)
                Destroy(toolbarControl);

            if (runtimeResources != null)
                Destroy(runtimeResources);

            // Restore original lighting
            RestoreLighting();

            Instance = null;
            Debug.Log("[KerbVisionIR] Cleanup complete");
        }

        #endregion

        #region Initialization

        void InitializePostProcessing()
        {
            // Get main camera
            Camera mainCamera = GetActiveCamera();
            if (mainCamera == null)
            {
                Debug.LogError("[KerbVisionIR] No active flight camera found!");
                return;
            }

            targetCamera = mainCamera;
            Debug.Log($"[KerbVisionIR] Found camera: {mainCamera.name}");

            // Load TUFX shader resources FIRST
            bool resourcesLoaded = LoadShaderResources();

            // Create profile with effects
            CreateEffectProfile();
            CreateTintProfile();

            // Store original lighting
            StoreLighting();

            if (!resourcesLoaded)
            {
                postProcessingAvailable = false;
                Debug.LogWarning("[KerbVisionIR] PostProcessing resources unavailable - running in lighting-only fallback mode.");
                return;
            }

            runtimeResources = BuildRuntimeResources();
            if (runtimeResources == null)
            {
                postProcessingAvailable = false;
                Debug.LogWarning("[KerbVisionIR] Failed to build PostProcessResources - running in lighting-only fallback mode.");
                return;
            }

            // Get or create PostProcessLayer
            layer = mainCamera.GetComponent<PostProcessLayer>();
            if (layer == null)
            {
                layer = mainCamera.gameObject.AddComponent<PostProcessLayer>();
                layer.Init(runtimeResources);
                layer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                layer.stopNaNPropagation = true;
                Debug.Log("[KerbVisionIR] Created PostProcessLayer");
            }
            else
            {
                layer.Init(runtimeResources);
                Debug.Log("[KerbVisionIR] Using existing PostProcessLayer");
            }

            layer.enabled = true;
            layer.volumeLayer = 1 << EffectLayer;

            // Create global PostProcessVolume
            GameObject volumeGO = new GameObject("KerbVisionIR_PostProcessVolume");
            volumeGO.layer = EffectLayer;
            volume = volumeGO.AddComponent<PostProcessVolume>();
            volume.isGlobal = true;
            volume.priority = 100f; // High priority
            volume.weight = 1f;
            volume.profile = profile;

            GameObject tintVolumeGO = new GameObject("KerbVisionIR_TintVolume");
            tintVolumeGO.layer = EffectLayer;
            tintVolume = tintVolumeGO.AddComponent<PostProcessVolume>();
            tintVolume.isGlobal = true;
            tintVolume.priority = 101f;
            tintVolume.weight = 0f;
            tintVolume.profile = tintProfile;
 
            postProcessingAvailable = true;
            Debug.Log("[KerbVisionIR] PostProcessing setup complete");
        }

        bool LoadShaderResources()
        {
            loadedShaders.Clear();
            loadedComputeShaders.Clear();

            // Try to load shader bundle
            string[] possiblePaths = new string[]
            {
                Path.Combine(KSPUtil.ApplicationRootPath, "GameData/KerbVisionIR/Shaders/kerbvision-pp.ssf"),
                Path.Combine(KSPUtil.ApplicationRootPath, "GameData/KerbVisionIR/Shaders/simplenv-pp.ssf")
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        var bundle = AssetBundle.LoadFromFile(path);
                        if (bundle != null)
                        {
                            foreach (var sh in bundle.LoadAllAssets<Shader>())
                            {
                                if (sh != null && !loadedShaders.ContainsKey(sh.name))
                                    loadedShaders.Add(sh.name, sh);
                            }

                            foreach (var csh in bundle.LoadAllAssets<ComputeShader>())
                            {
                                if (csh != null && !loadedComputeShaders.ContainsKey(csh.name))
                                    loadedComputeShaders.Add(csh.name, csh);
                            }

                            Debug.Log($"[KerbVisionIR] Loaded shader bundle from: {path} (Shaders: {loadedShaders.Count}, Compute: {loadedComputeShaders.Count})");
                            bundle.Unload(false);
                            return loadedShaders.Count > 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[KerbVisionIR] Failed to load shader bundle: {ex.Message}");
                    }
                }
                else
                {
                    Debug.Log($"[KerbVisionIR] Shader bundle not found at: {path}");
                }
            }

            Debug.LogWarning("[KerbVisionIR] No shader bundle found - effects may not work!");
            return false;
        }

        Shader GetLoadedShader(string name)
        {
            Shader s;
            if (loadedShaders.TryGetValue(name, out s))
                return s;

            return Shader.Find(name);
        }

        ComputeShader GetLoadedComputeShader(string name)
        {
            ComputeShader s;
            if (loadedComputeShaders.TryGetValue(name, out s))
                return s;

            return null;
        }

        PostProcessResources BuildRuntimeResources()
        {
            var resources = ScriptableObject.CreateInstance<PostProcessResources>();
            resources.shaders = new PostProcessResources.Shaders();
            resources.computeShaders = new PostProcessResources.ComputeShaders();
            resources.blueNoise64 = new Texture2D[64];
            resources.blueNoise256 = new Texture2D[8];
            resources.smaaLuts = new PostProcessResources.SMAALuts();

            resources.shaders.copy = GetLoadedShader("Hidden/PostProcessing/Copy");
            resources.shaders.copyStd = GetLoadedShader("Hidden/PostProcessing/CopyStd");
            resources.shaders.copyStdFromDoubleWide = GetLoadedShader("Hidden/PostProcessing/CopyStdFromDoubleWide");
            resources.shaders.copyStdFromTexArray = GetLoadedShader("Hidden/PostProcessing/CopyStdFromTexArray");
            resources.shaders.uber = GetLoadedShader("Hidden/PostProcessing/Uber");
            resources.shaders.lut2DBaker = GetLoadedShader("Hidden/PostProcessing/Lut2DBaker");
            resources.shaders.grainBaker = GetLoadedShader("Hidden/PostProcessing/GrainBaker");
            resources.shaders.finalPass = GetLoadedShader("Hidden/PostProcessing/FinalPass");
            resources.shaders.texture2dLerp = GetLoadedShader("Hidden/PostProcessing/Texture2DLerp");
            resources.shaders.scalableAO = GetLoadedShader("Hidden/PostProcessing/ScalableAO");
            resources.shaders.multiScaleAO = GetLoadedShader("Hidden/PostProcessing/MultiScaleVO");

            resources.computeShaders.multiScaleAODownsample1 = GetLoadedComputeShader("KMultiScaleVODownsample1");
            resources.computeShaders.multiScaleAODownsample2 = GetLoadedComputeShader("KMultiScaleVODownsample2");
            resources.computeShaders.multiScaleAORender = GetLoadedComputeShader("KMultiScaleVORender");
            resources.computeShaders.multiScaleAOUpsample = GetLoadedComputeShader("KMultiScaleVOUpsample");

            // Fill required runtime textures with safe defaults to prevent null dereferences
            var white = Texture2D.whiteTexture;
            for (int i = 0; i < resources.blueNoise64.Length; i++)
                resources.blueNoise64[i] = white;

            for (int i = 0; i < resources.blueNoise256.Length; i++)
                resources.blueNoise256[i] = white;

            resources.smaaLuts.area = white;
            resources.smaaLuts.search = white;

            if (resources.shaders.copyStd == null || resources.shaders.copy == null || resources.shaders.uber == null || resources.shaders.lut2DBaker == null)
                return null;

            return resources;
        }

        void CreateEffectProfile()
        {
            profile = ScriptableObject.CreateInstance<PostProcessProfile>();
            profile.name = "KerbVisionIR_Profile";

            // === VIGNETTE EFFECT ===
            vignette = profile.AddSettings<Vignette>();
            vignette.active = true;
            vignette.enabled.Override(true);
            vignette.mode.Override(VignetteMode.Classic);
            vignette.color.Override(Color.black);
            vignette.center.Override(new Vector2(0.5f, 0.5f));
            vignette.intensity.Override(0f); // Start at 0 (disabled)
            vignette.smoothness.Override(0.3f);
            vignette.roundness.Override(1f);
            vignette.rounded.Override(false);

            Debug.Log("[KerbVisionIR] Vignette added to profile");

            // === GRAIN EFFECT ===
            grain = profile.AddSettings<Grain>();
            grain.active = true;
            grain.enabled.Override(true);
            grain.colored.Override(false);
            grain.intensity.Override(0f);
            grain.size.Override(0.55f);
            grain.lumContrib.Override(0.8f);

            Debug.Log("[KerbVisionIR] Grain added to profile");

            // === COLOR GRADING EFFECT ===
            colorGrading = profile.AddSettings<ColorGrading>();
            colorGrading.active = true;
            colorGrading.enabled.Override(true);
            colorGrading.gradingMode.Override(GradingMode.LowDefinitionRange); // LDR for compatibility
            
            // Start with neutral values
            colorGrading.temperature.Override(0f);
            colorGrading.tint.Override(0f);
            colorGrading.colorFilter.Override(Color.white);
            colorGrading.hueShift.Override(0f);
            colorGrading.saturation.Override(0f); // Start neutral
            colorGrading.brightness.Override(0f);
            colorGrading.contrast.Override(0f); // Start neutral

            Debug.Log("[KerbVisionIR] ColorGrading added to profile");
        }

        void CreateTintProfile()
        {
            tintProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
            tintProfile.name = "KerbVisionIR_TintProfile";

            tintColorGrading = tintProfile.AddSettings<ColorGrading>();
            tintColorGrading.active = true;
            tintColorGrading.enabled.Override(true);
            tintColorGrading.gradingMode.Override(GradingMode.LowDefinitionRange);

            tintColorGrading.temperature.overrideState = false;
            tintColorGrading.tint.overrideState = false;
            tintColorGrading.hueShift.overrideState = false;
            tintColorGrading.saturation.overrideState = false;
            tintColorGrading.brightness.overrideState = false;
            tintColorGrading.contrast.overrideState = false;
            tintColorGrading.colorFilter.Override(Color.white);
        }
 
        #endregion

        #region Effect Control

        void ToggleNightVision()
        {
            if (!isEffectActive)
            {
                isEffectActive = true;
                cameraOverlayLogged = false;
                guiUnderlayLogged = false;
                EnableEffect();
                PlayEnableSound();
                BeginningEnableTransition();
                ScreenMessages.PostScreenMessage(
                    "Night Vision ON",
                    2f,
                    ScreenMessageStyle.UPPER_CENTER
                );
                Debug.Log($"[KerbVisionIR] Effect ENABLED - Mode: {currentMode}, Brightness: {brightnessMultiplier}x");
            }
            else
            {
                BeginDisableTransition();
                ScreenMessages.PostScreenMessage(
                    "Night Vision OFF",
                    2f,
                    ScreenMessageStyle.UPPER_CENTER
                );
                Debug.Log("[KerbVisionIR] Effect DISABLED");
            }
        }

        void BeginningEnableTransition()
        {
            transitionActive = true;
            transitionEnabling = true;
            transitionTimer = 0f;
            transitionBlend = 0f;
            transitionOverlayAlpha = 1f;
            transitionNoiseBoost = 0.8f;
        }

        void BeginDisableTransition()
        {
            transitionActive = true;
            transitionEnabling = false;
            transitionTimer = 0f;
            transitionBlend = 1f;
            transitionOverlayAlpha = 0f;
            transitionNoiseBoost = 0.25f;
        }

        void UpdateTransition(float dt)
        {
            transitionTimer += dt;

            if (transitionEnabling)
            {
                if (transitionTimer < NvOnBlackoutDuration)
                {
                    transitionBlend = 0f;
                    transitionOverlayAlpha = 1f;
                    transitionNoiseBoost = 1f;
                    return;
                }

                float fadeT = Mathf.Clamp01((transitionTimer - NvOnBlackoutDuration) / NvOnFadeDuration);
                transitionBlend = fadeT;
                transitionOverlayAlpha = 1f - fadeT;
                transitionNoiseBoost = 1f - fadeT;

                if (fadeT >= 1f)
                {
                    transitionActive = false;
                    transitionBlend = 1f;
                    transitionOverlayAlpha = 0f;
                    transitionNoiseBoost = 0f;
                }
            }
            else
            {
                float fadeT = Mathf.Clamp01(transitionTimer / NvOffFadeDuration);
                transitionBlend = 1f - fadeT;
                transitionOverlayAlpha = fadeT * 0.45f;
                transitionNoiseBoost = 0.25f * transitionBlend;

                if (transitionTimer >= NvOffFadeDuration + NvOffBlackoutDuration)
                {
                    transitionActive = false;
                    transitionBlend = 0f;
                    transitionOverlayAlpha = 0f;
                    transitionNoiseBoost = 0f;
                    isEffectActive = false;
                    DisableEffect();
                }
            }
        }

        float GetEffectBlend()
        {
            if (!isEffectActive)
                return 0f;

            return transitionActive ? transitionBlend : 1f;
        }

        void EnableEffect()
        {
            StoreLighting();

            if (!postProcessingAvailable)
            {
                ApplyBrightnessBoost();
                return;
            }

            if (vignette == null || colorGrading == null)
            {
                Debug.LogError("[KerbVisionIR] Effects not initialized!");
                return;
            }

            EnforceActiveEffectState();
            ApplyBrightnessBoost();
        }

        void EnforceActiveEffectState()
        {
             if (!postProcessingAvailable || vignette == null || colorGrading == null)
                 return;

            float effectBlend = GetEffectBlend();
            float tintStrength = Mathf.Clamp01(colorTintStrength);

            if (volume != null)
            {
                volume.isGlobal = true;
                volume.weight = 1f;
                volume.enabled = true;
                if (volume.profile != profile)
                    volume.profile = profile;
            }

            if (tintVolume != null)
            {
                tintVolume.weight = 0f;
                tintVolume.enabled = false;
            }

            float effectiveVignette = Mathf.Clamp01(vignetteIntensity * effectBlend);
            vignette.intensity.Override(vignetteEnabled ? effectiveVignette : 0f);
            vignette.smoothness.Override(0.35f);

            if (grain != null)
            {
                grain.enabled.Override(true);
                grain.colored.Override(false);
                grain.size.Override(0.35f);
                grain.lumContrib.Override(0.2f);

                float grainResponse = Mathf.Pow(Mathf.Clamp01(grainIntensity), 0.85f);
                float effectiveGrain = grainEnabled
                    ? Mathf.Clamp(grainResponse * 0.75f * effectBlend, MinGrainIntensity, MaxGrainIntensity)
                    : 0f;

                grain.intensity.Override(effectiveGrain);
            }

            colorGrading.saturation.Override(0f);
            colorGrading.contrast.Override(22f);
            float targetBrightness = Mathf.Clamp((brightnessMultiplier - 1f) * 60f, 0f, 100f);
            colorGrading.brightness.Override(Mathf.Lerp(0f, targetBrightness, effectBlend));
            colorGrading.colorFilter.Override(Color.white);

            const float monoR = 29.9f;
            const float monoG = 58.7f;
            const float monoB = 11.4f;

            float targetRScale = Mathf.Lerp(1f, 0.18f, tintStrength);
            float targetGScale = Mathf.Lerp(1f, 1.20f, tintStrength);
            float targetBScale = Mathf.Lerp(1f, 0.22f, tintStrength);

            float redOutRedIn = Mathf.Lerp(100f, monoR * targetRScale, effectBlend);
            float redOutGreenIn = Mathf.Lerp(0f, monoG * targetRScale, effectBlend);
            float redOutBlueIn = Mathf.Lerp(0f, monoB * targetRScale, effectBlend);

            float greenOutRedIn = Mathf.Lerp(0f, monoR * targetGScale, effectBlend);
            float greenOutGreenIn = Mathf.Lerp(100f, monoG * targetGScale, effectBlend);
            float greenOutBlueIn = Mathf.Lerp(0f, monoB * targetGScale, effectBlend);

            float blueOutRedIn = Mathf.Lerp(0f, monoR * targetBScale, effectBlend);
            float blueOutGreenIn = Mathf.Lerp(0f, monoG * targetBScale, effectBlend);
            float blueOutBlueIn = Mathf.Lerp(100f, monoB * targetBScale, effectBlend);

            colorGrading.mixerRedOutRedIn.Override(redOutRedIn);
            colorGrading.mixerRedOutGreenIn.Override(redOutGreenIn);
            colorGrading.mixerRedOutBlueIn.Override(redOutBlueIn);
            colorGrading.mixerGreenOutRedIn.Override(greenOutRedIn);
            colorGrading.mixerGreenOutGreenIn.Override(greenOutGreenIn);
            colorGrading.mixerGreenOutBlueIn.Override(greenOutBlueIn);
            colorGrading.mixerBlueOutRedIn.Override(blueOutRedIn);
            colorGrading.mixerBlueOutGreenIn.Override(blueOutGreenIn);
            colorGrading.mixerBlueOutBlueIn.Override(blueOutBlueIn);

            if (tintColorGrading != null)
            {
                tintColorGrading.colorFilter.Override(Color.white);
            }
        }

        void DisableEffect()
        {
            if (postProcessingAvailable && vignette != null && colorGrading != null)
            {
                vignette.intensity.Override(0f);

                colorGrading.saturation.Override(0f);
                colorGrading.contrast.Override(0f);
                colorGrading.brightness.Override(0f);
                colorGrading.colorFilter.Override(Color.white);
                colorGrading.mixerRedOutRedIn.Override(100f);
                colorGrading.mixerRedOutGreenIn.Override(0f);
                colorGrading.mixerRedOutBlueIn.Override(0f);
                colorGrading.mixerGreenOutRedIn.Override(0f);
                colorGrading.mixerGreenOutGreenIn.Override(100f);
                colorGrading.mixerGreenOutBlueIn.Override(0f);
                colorGrading.mixerBlueOutRedIn.Override(0f);
                colorGrading.mixerBlueOutGreenIn.Override(0f);
                colorGrading.mixerBlueOutBlueIn.Override(100f);

                if (tintColorGrading != null)
                {
                    tintColorGrading.colorFilter.Override(Color.white);
                }

                if (tintVolume != null)
                {
                    tintVolume.weight = 0f;
                    tintVolume.enabled = false;
                }
            }

            RestoreLighting();
        }

        void CycleMode()
        {
            CycleMode(1);
        }

        void CycleMode(int direction)
        {
            int count = Enum.GetValues(typeof(VisionMode)).Length;
            int index = (int)currentMode + direction;
            while (index < 0) index += count;
            currentMode = (VisionMode)(index % count);
            PlayerPrefs.SetInt("KerbVisionIR_Mode", (int)currentMode);
            PlayerPrefs.Save();

            Debug.Log($"[KerbVisionIR] Mode changed to: {currentMode}");
            ScreenMessages.PostScreenMessage(
                $"<color=lime>[NV Mode] {currentMode}</color>", 
                2f, 
                ScreenMessageStyle.UPPER_CENTER
            );

            if (isEffectActive)
            {
                if (postProcessingAvailable && colorGrading != null)
                    colorGrading.colorFilter.Override(GetModeColor(currentMode));

                ApplyBrightnessBoost();
            }
        }

        Color GetModeColor(VisionMode mode)
        {
            switch (mode)
            {
                case VisionMode.Monochrome:
                    return Color.white;

                case VisionMode.GreenNV:
                    return new Color(0.03f, 1.55f, 0.15f, 1f);

                case VisionMode.AmberWarm:
                    return new Color(1.35f, 0.80f, 0.12f, 1f);

                default:
                    return Color.white;
            }
        }

        void TryToggleNightVision()
        {
            if (MapView.MapIsEnabled)
            {
                ScreenMessages.PostScreenMessage(
                    "<color=yellow>[Night Vision] Disabled in Map View</color>",
                    2f,
                    ScreenMessageStyle.UPPER_CENTER
                );
                return;
            }

            float now = Time.unscaledTime;
            if (now - lastToggleTime < ToggleCooldownSeconds)
                return;

            lastToggleTime = now;
            ToggleNightVision();
        }

        void InitializeAudio()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.GetComponent<AudioSource>();
                if (audioSource == null)
                    audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.spatialBlend = 0f;
                audioSource.volume = 1f;
            }

            string[] clipPaths =
            {
                "KerbVisionIR/soud/NVon",
                "KerbVisionIR/sound/NVon",
                "KerbVisionIR/Sound/NVon",
                "KerbVisionIR/Sounds/NVon"
            };

            foreach (var clipPath in clipPaths)
            {
                var clip = GameDatabase.Instance.GetAudioClip(clipPath);
                if (clip != null)
                {
                    nvOnClip = clip;
                    Debug.Log($"[KerbVisionIR] Loaded NV sound: {clipPath}");
                    return;
                }
            }

            Debug.LogWarning("[KerbVisionIR] NVon sound not found (expected under GameData/KerbVisionIR)");
        }

        string ResolveToolbarIconPath()
        {
            string[] iconCandidates =
            {
                "KerbVisionIR/Assets/KerbVisionIR-Icon",
                "KerbVisionIR/Assets/SimpleNV-Icon"
            };

            foreach (string iconPath in iconCandidates)
            {
                if (GameDatabase.Instance.GetTexture(iconPath, false) != null)
                    return iconPath;
            }

            return "KerbVisionIR/Assets/SimpleNV-Icon";
        }

        void InitializeToolbar()
        {
            toolbarReady = false;
            ToolbarControl.RegisterMod(ToolbarModId);
            toolbarControl = gameObject.GetComponent<ToolbarControl>();
            if (toolbarControl == null)
                toolbarControl = gameObject.AddComponent<ToolbarControl>();

            string iconPath = ResolveToolbarIconPath();

            toolbarControl.AddToAllToolbars(
                OnToolbarEnable,
                OnToolbarDisable,
                ApplicationLauncher.AppScenes.FLIGHT,
                ToolbarModId,
                ToolbarModId,
                iconPath,
                iconPath,
                ToolbarModId);
        }

        void OnToolbarEnable()
        {
            if (!toolbarReady)
                return;
            showWindow = true;
        }
 
        void OnToolbarDisable()
        {
            if (!toolbarReady)
                return;
            showWindow = false;
        }

        void PlayEnableSound()
        {
            if (audioSource == null || nvOnClip == null)
                return;

            audioSource.PlayOneShot(nvOnClip);
        }

        void CreateFallbackGrainTexture()
        {
            if (fallbackGrainTexture == null)
            {
                fallbackGrainTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                fallbackGrainTexture.wrapMode = TextureWrapMode.Repeat;
                fallbackGrainTexture.filterMode = FilterMode.Bilinear;
            }

            int width = fallbackGrainTexture.width;
            int height = fallbackGrainTexture.height;
            var pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                byte v = (byte)UnityEngine.Random.Range(88, 168);
                pixels[i] = new Color32(v, v, v, 255);
            }

            fallbackGrainTexture.SetPixels32(pixels);
            fallbackGrainTexture.Apply(false, false);
            fallbackGrainFrame = Time.frameCount;
        }

        void CreateFallbackVignetteTexture()
        {
            const int size = 256;
            fallbackVignetteTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            fallbackVignetteTexture.wrapMode = TextureWrapMode.Clamp;
            fallbackVignetteTexture.filterMode = FilterMode.Bilinear;

            var pixels = new Color32[size * size];
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float maxDist = center.magnitude;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                    float edge = Mathf.Clamp01((dist - 0.45f) / 0.55f);
                    float alpha = Mathf.Pow(edge, 1.75f);
                    byte a = (byte)(alpha * 255f);
                    pixels[y * size + x] = new Color32(0, 0, 0, a);
                }
            }

            fallbackVignetteTexture.SetPixels32(pixels);
            fallbackVignetteTexture.Apply(false, false);
        }

        void CreateFallbackScanlineTexture()
        {
            fallbackScanlineTexture = new Texture2D(1, 2, TextureFormat.RGBA32, false);
            fallbackScanlineTexture.wrapMode = TextureWrapMode.Repeat;
            fallbackScanlineTexture.filterMode = FilterMode.Point;
            fallbackScanlineTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.85f));
            fallbackScanlineTexture.SetPixel(0, 1, new Color(0f, 0f, 0f, 0.15f));
            fallbackScanlineTexture.Apply(false, false);
        }

        void InitializeCameraOverlayMaterials()
        {
            if (cameraTintMaterial == null)
            {
                Shader tintShader = Shader.Find("Hidden/Internal-Colored");
                if (tintShader != null)
                {
                    cameraTintMaterial = new Material(tintShader);
                    cameraTintMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
            }

            if (cameraScanlineMaterial == null)
            {
                Shader scanlineShader = Shader.Find("Unlit/Transparent");
                if (scanlineShader == null)
                    scanlineShader = Shader.Find("UI/Default");

                if (scanlineShader != null)
                {
                    cameraScanlineMaterial = new Material(scanlineShader);
                    cameraScanlineMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
            }
        }

        void OnCameraPostRender(Camera cam)
        {
            if (!isEffectActive || !postProcessingAvailable)
                return;

            if (cam == null || targetCamera == null || cam != targetCamera)
                return;

            bool drewOverlay = false;

            if (scanlinesEnabled && cameraTintMaterial != null)
            {
                float lineAlpha = Mathf.Clamp01(scanlineIntensity * 0.85f * GetEffectBlend());
                if (lineAlpha > 0.001f)
                {
                    float step = Mathf.Max(2f, Screen.height / 540f * 2f);
                    float h = 1f / Screen.height;
                    cameraTintMaterial.SetPass(0);
                    GL.PushMatrix();
                    GL.LoadOrtho();
                    GL.Begin(GL.QUADS);
                    GL.Color(new Color(0f, 0f, 0f, lineAlpha));

                    for (float y = 0f; y < Screen.height; y += step)
                    {
                        float y0 = y / Screen.height;
                        float y1 = Mathf.Min(1f, y0 + h);
                        GL.Vertex3(0f, y0, 0f);
                        GL.Vertex3(1f, y0, 0f);
                        GL.Vertex3(1f, y1, 0f);
                        GL.Vertex3(0f, y1, 0f);
                    }

                    GL.End();
                    GL.PopMatrix();
                    drewOverlay = true;
                }
            }

            if (drewOverlay)
            {
                cameraOverlayRenderedFrame = Time.frameCount;
                cameraOverlayLastSeenTime = Time.unscaledTime;
            }
        }

        void DrawFallbackEffectOverlay()
        {
            if (fallbackWhiteTexture == null)
                return;

            float effectBlend = GetEffectBlend();

            float monoAlpha = Mathf.Lerp(0.14f, 0.28f, Mathf.InverseLerp(MinBrightnessMultiplier, MaxBrightnessMultiplier, brightnessMultiplier)) * effectBlend;
            GUI.color = new Color(1f, 1f, 1f, monoAlpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), fallbackWhiteTexture, ScaleMode.StretchToFill, true);

            if (currentMode != VisionMode.Monochrome)
            {
                Color tint = GetModeColor(currentMode);
                float tintAlpha = Mathf.Lerp(0.05f, 0.14f, Mathf.InverseLerp(MinBrightnessMultiplier, MaxBrightnessMultiplier, brightnessMultiplier)) * effectBlend;
                GUI.color = new Color(tint.r, tint.g, tint.b, tintAlpha);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), fallbackWhiteTexture, ScaleMode.StretchToFill, true);
            }

            if (grainEnabled && fallbackGrainTexture != null)
            {
                float effectiveGrain = Mathf.Clamp(grainIntensity, MinGrainIntensity, MaxGrainIntensity);
                GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(effectiveGrain * (0.9f * effectBlend + transitionNoiseBoost * 0.35f)));
                float u = Screen.width / 96f;
                float v = Screen.height / 96f;
                GUI.DrawTextureWithTexCoords(new Rect(0f, 0f, Screen.width, Screen.height), fallbackGrainTexture, new Rect(0f, 0f, u, v), true);
            }

            if (scanlinesEnabled && fallbackScanlineTexture != null)
            {
                float lineAlpha = Mathf.Clamp01(scanlineIntensity * effectBlend);
                if (lineAlpha > 0.001f)
                {
                    GUI.color = new Color(1f, 1f, 1f, lineAlpha);
                    float vRepeat = Screen.height / 2f;
                    GUI.DrawTextureWithTexCoords(new Rect(0f, 0f, Screen.width, Screen.height), fallbackScanlineTexture, new Rect(0f, 0f, 1f, vRepeat), true);
                }
            }

            if (vignetteEnabled)
            {
                float effectiveVignette = Mathf.Clamp01(vignetteIntensity * effectBlend);
                float vignetteAlpha = Mathf.Clamp01(effectiveVignette * 1.35f);
                if (fallbackVignetteTexture != null && vignetteAlpha > 0.001f)
                {
                    GUI.color = new Color(1f, 1f, 1f, vignetteAlpha);
                    GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), fallbackVignetteTexture, ScaleMode.StretchToFill, true);
                }
            }

            GUI.color = Color.white;
        }

        void DrawTransitionOverlay()
        {
            if (!transitionActive || transitionOverlayAlpha <= 0.001f || fallbackWhiteTexture == null)
                return;

            GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01(transitionOverlayAlpha));
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), fallbackWhiteTexture, ScaleMode.StretchToFill, true);
            GUI.color = Color.white;
        }

        #endregion

        #region Lighting Control

        void StoreLighting()
        {
            storedAmbientLight = RenderSettings.ambientLight;
            storedAmbientIntensity = RenderSettings.ambientIntensity;
            lightingStored = true;
            Debug.Log($"[KerbVisionIR] Stored lighting: {storedAmbientLight}, intensity: {storedAmbientIntensity}");
        }

        void ApplyBrightnessBoost()
        {
            if (lightingStored)
            {
                float blend = GetEffectBlend();
                float effectiveBrightness = Mathf.Lerp(1f, brightnessMultiplier, blend);

                Color boosted = storedAmbientLight * effectiveBrightness;
                float gray = boosted.grayscale;
                Color monoBoosted = new Color(gray, gray, gray, boosted.a);
                bool useMonochromeOnly = currentMode == VisionMode.Monochrome || colorTintStrength <= 0.05f;

                if (useMonochromeOnly)
                {
                    RenderSettings.ambientLight = monoBoosted;
                }
                else
                {
                    RenderSettings.ambientLight = monoBoosted;
                }

                RenderSettings.ambientIntensity = storedAmbientIntensity * effectiveBrightness;
            }
        }

        void RestoreLighting()
        {
            if (lightingStored)
            {
                RenderSettings.ambientLight = storedAmbientLight;
                RenderSettings.ambientIntensity = storedAmbientIntensity;
                Debug.Log("[KerbVisionIR] Restored original lighting");
            }
        }

        #endregion

        #region Camera Handling

        Camera GetActiveCamera()
        {
            if (FlightCamera.fetch != null && FlightCamera.fetch.mainCamera != null)
                return FlightCamera.fetch.mainCamera;

            return Camera.main;
        }

        void EnsureActiveCameraLayer()
        {
            if (!postProcessingAvailable)
                return;

            Camera activeCamera = GetActiveCamera();
            if (activeCamera == null)
                return;

            if (targetCamera == activeCamera && layer != null)
            {
                layer.enabled = true;
                layer.volumeLayer = 1 << EffectLayer;
                return;
            }

            targetCamera = activeCamera;
            layer = activeCamera.GetComponent<PostProcessLayer>();
            if (layer == null)
            {
                layer = activeCamera.gameObject.AddComponent<PostProcessLayer>();
                layer.Init(runtimeResources);
                layer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                layer.stopNaNPropagation = true;
                Debug.Log($"[KerbVisionIR] Added PostProcessLayer to active camera: {activeCamera.name}");
            }
            else
            {
                layer.Init(runtimeResources);
            }

            layer.enabled = true;
            layer.volumeLayer = 1 << EffectLayer;
        }

        #endregion
    }
}

