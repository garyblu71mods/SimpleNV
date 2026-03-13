using System;
using System.IO;
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
        private PostProcessProfile profile;
        private Camera targetCamera;
        private bool postProcessingAvailable = false;
        
        // Effect references
        private Vignette vignette;
        private ColorGrading colorGrading;
        private Grain grain;

        // State variables
        private bool isEffectActive = false;
        private VisionMode currentMode = VisionMode.GreenNV;
        private float brightnessMultiplier = 1.8f;
        private const float MinBrightnessMultiplier = 1.0f;
        private const float MaxBrightnessMultiplier = 4.0f;
        private const float FallbackTintStrength = 0.85f;
        private bool vignetteEnabled = true;
        private bool grainEnabled = true;
        private float vignetteIntensity = 0.45f;
        private float grainIntensity = 0.22f;
        private const float MinVignetteIntensity = 0f;
        private const float MaxVignetteIntensity = 0.8f;
        private const float MinGrainIntensity = 0f;
        private const float MaxGrainIntensity = 0.5f;
        private float vignetteBoost = 1f;
        private float grainBoost = 1f;
        private const float MinEffectBoost = 1f;
        private const float MaxEffectBoost = 4f;
        private float lastToggleTime = -10f;
        private const float ToggleCooldownSeconds = 0.25f;
        private Texture2D fallbackWhiteTexture;
        private Texture2D fallbackGrainTexture;
        private Texture2D fallbackVignetteTexture;
        private int fallbackGrainFrame;
        // GUI / binding
        private Rect guiWindowRect = new Rect(10, 10, 300, 280);
        private bool capturingKey = false;
        private KeyCode boundToggleKey = KeyCode.BackQuote;
        private bool requireAltForBoundKey = true;
        
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
                currentMode = (VisionMode)PlayerPrefs.GetInt("KerbVisionIR_Mode", (int)VisionMode.GreenNV);
                vignetteEnabled = PlayerPrefs.GetInt("KerbVisionIR_VignetteEnabled", 1) == 1;
                grainEnabled = PlayerPrefs.GetInt("KerbVisionIR_GrainEnabled", 1) == 1;
                vignetteIntensity = Mathf.Clamp(PlayerPrefs.GetFloat("KerbVisionIR_VignetteIntensity", 0.45f), MinVignetteIntensity, MaxVignetteIntensity);
                grainIntensity = Mathf.Clamp(PlayerPrefs.GetFloat("KerbVisionIR_GrainIntensity", 0.22f), MinGrainIntensity, MaxGrainIntensity);
                vignetteBoost = Mathf.Clamp(PlayerPrefs.GetFloat("KerbVisionIR_VignetteBoost", 1f), MinEffectBoost, MaxEffectBoost);
                grainBoost = Mathf.Clamp(PlayerPrefs.GetFloat("KerbVisionIR_GrainBoost", 1f), MinEffectBoost, MaxEffectBoost);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[KerbVisionIR] Initialization failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        void Update()
        {
            if (postProcessingAvailable)
            {
                EnsureActiveCameraLayer();
            }

            // Hotkey detection: user-configurable
            if (!capturingKey && boundToggleKey != KeyCode.None && Input.GetKeyDown(boundToggleKey))
            {
                if (!requireAltForBoundKey || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    TryToggleNightVision();
                }
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
                else
                {
                    ApplyBrightnessBoost();
                    if (grainEnabled && Time.frameCount - fallbackGrainFrame > 3)
                    {
                        CreateFallbackGrainTexture();
                    }
                }
            }
        }

        void OnGUI()
        {
            if (isEffectActive && !postProcessingAvailable)
            {
                DrawFallbackEffectOverlay();
            }

            guiWindowRect = GUILayout.Window(10101, guiWindowRect, GuiWindow, "KerbVisionIR");
        }

        void GuiWindow(int id)
        {
            GUILayout.BeginVertical();

            GUILayout.Label($"Mod: KerbVisionIR");

            // Last modification date of this assembly
            try
            {
                var asmPath = Assembly.GetExecutingAssembly().Location;
                var lastWrite = File.GetLastWriteTime(asmPath);
                GUILayout.Label($"Last modified: {lastWrite}");
            }
            catch
            {
                GUILayout.Label("Last modified: unknown");
            }

            GUILayout.Space(8);

            // Toggle button
            if (GUILayout.Button(isEffectActive ? "Disable Night Vision" : "Enable Night Vision", GUILayout.Height(30)))
            {
                TryToggleNightVision();
            }

            GUILayout.Space(6);

            GUILayout.Label($"Mode: {currentMode}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Prev Mode", GUILayout.Height(24)))
            {
                CycleMode(-1);
            }
            if (GUILayout.Button("Next Mode", GUILayout.Height(24)))
            {
                CycleMode(1);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label($"Brightness: {brightnessMultiplier:0.00}x");
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
            GUILayout.BeginHorizontal();
            bool newVignetteEnabled = GUILayout.Toggle(vignetteEnabled, "Vignette", GUILayout.Height(24));
            bool newGrainEnabled = GUILayout.Toggle(grainEnabled, "Grain", GUILayout.Height(24));
            GUILayout.EndHorizontal();

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

            GUILayout.Label($"Vignette Boost: x{vignetteBoost:0.00}");
            float newVignetteBoost = GUILayout.HorizontalSlider(vignetteBoost, MinEffectBoost, MaxEffectBoost);
            if (Mathf.Abs(newVignetteBoost - vignetteBoost) > 0.001f)
            {
                vignetteBoost = newVignetteBoost;
                PlayerPrefs.SetFloat("KerbVisionIR_VignetteBoost", vignetteBoost);
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

            GUILayout.Label($"Grain Boost: x{grainBoost:0.00}");
            float newGrainBoost = GUILayout.HorizontalSlider(grainBoost, MinEffectBoost, MaxEffectBoost);
            if (Mathf.Abs(newGrainBoost - grainBoost) > 0.001f)
            {
                grainBoost = newGrainBoost;
                PlayerPrefs.SetFloat("KerbVisionIR_GrainBoost", grainBoost);
                PlayerPrefs.Save();
                if (isEffectActive)
                    EnforceActiveEffectState();
            }

            GUILayout.Space(6);

            // Binding area
            GUILayout.Label($"Toggle key: {boundToggleKey} {(requireAltForBoundKey ? "(requires Alt)" : "")}");
            GUILayout.BeginHorizontal();
            if (capturingKey)
            {
                if (GUILayout.Button("Press any key...", GUILayout.Height(24))) { /* noop while capturing */ }
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

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        void OnDestroy()
        {
            Debug.Log("[KerbVisionIR] Cleanup started");

            // Disable effect before cleanup
            if (isEffectActive)
                DisableEffect();

            // Destroy PostProcessing objects
            if (volume != null)
                Destroy(volume.gameObject);

            if (profile != null)
                Destroy(profile);

            if (fallbackWhiteTexture != null)
                Destroy(fallbackWhiteTexture);

            if (fallbackGrainTexture != null)
                Destroy(fallbackGrainTexture);

            if (fallbackVignetteTexture != null)
                Destroy(fallbackVignetteTexture);

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

            // Store original lighting
            StoreLighting();

            if (!resourcesLoaded)
            {
                postProcessingAvailable = false;
                Debug.LogWarning("[KerbVisionIR] PostProcessing resources unavailable - running in lighting-only fallback mode.");
                return;
            }

            // Get or create PostProcessLayer
            layer = mainCamera.GetComponent<PostProcessLayer>();
            if (layer == null)
            {
                layer = mainCamera.gameObject.AddComponent<PostProcessLayer>();
                layer.Init(null); // TUFX will auto-initialize resources
                layer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                layer.stopNaNPropagation = true;
                Debug.Log("[KerbVisionIR] Created PostProcessLayer");
            }
            else
            {
                Debug.Log("[KerbVisionIR] Using existing PostProcessLayer");
            }

            layer.enabled = true;
            layer.volumeLayer = -1; // Ensure our global volume is always considered

            // Create global PostProcessVolume
            GameObject volumeGO = new GameObject("KerbVisionIR_PostProcessVolume");
            volume = volumeGO.AddComponent<PostProcessVolume>();
            volume.isGlobal = true;
            volume.priority = 100f; // High priority
            volume.weight = 1f;
            volume.profile = profile;

            postProcessingAvailable = true;
            Debug.Log("[KerbVisionIR] PostProcessing setup complete");
        }

        bool LoadShaderResources()
        {
            // Try to load shader bundle
            string[] possiblePaths = new string[]
            {
                Path.Combine(KSPUtil.ApplicationRootPath, "GameData/SimpleNV/Shaders/simplenv-pp.ssf"),
                Path.Combine(KSPUtil.ApplicationRootPath, "GameData/SimpleNV/Shaders/kerbvision-pp.ssf")
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
                            Debug.Log($"[KerbVisionIR] Loaded shader bundle from: {path}");
                            bundle.Unload(false);
                            return true;
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

        #endregion

        #region Effect Control

        void ToggleNightVision()
        {
            isEffectActive = !isEffectActive;

            if (isEffectActive)
            {
                EnableEffect();
                ScreenMessages.PostScreenMessage(
                    $"<color=lime>[Night Vision] ON - Mode: {currentMode}</color>", 
                    3f, 
                    ScreenMessageStyle.UPPER_CENTER
                );
                Debug.Log($"[KerbVisionIR] Effect ENABLED - Mode: {currentMode}, Brightness: {brightnessMultiplier}x");
            }
            else
            {
                DisableEffect();
                ScreenMessages.PostScreenMessage(
                    "<color=red>[Night Vision] OFF</color>", 
                    2f, 
                    ScreenMessageStyle.UPPER_CENTER
                );
                Debug.Log("[KerbVisionIR] Effect DISABLED");
            }
        }

        void EnableEffect()
        {
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

            // Apply brightness boost via lighting
            ApplyBrightnessBoost();
        }

        void EnforceActiveEffectState()
        {
            if (!postProcessingAvailable || vignette == null || colorGrading == null)
                return;

            if (volume != null)
            {
                volume.isGlobal = true;
                volume.weight = 1f;
                volume.enabled = true;
                if (volume.profile != profile)
                    volume.profile = profile;
            }

            // Apply Vignette (dark corners)
            float effectiveVignette = Mathf.Clamp01(vignetteIntensity * vignetteBoost);
            vignette.intensity.Override(vignetteEnabled ? effectiveVignette : 0f);
            vignette.smoothness.Override(0.35f);

            // Apply Color Grading
            colorGrading.saturation.Override(-60f); // Strong desaturation
            colorGrading.contrast.Override(25f); // Increase contrast
            colorGrading.brightness.Override(0f); // Neutral brightness in post
            colorGrading.colorFilter.Override(GetModeColor(currentMode));

            if (grain != null)
            {
                float effectiveGrain = Mathf.Clamp(grainIntensity * grainBoost, MinGrainIntensity, MaxGrainIntensity);
                grain.intensity.Override(grainEnabled ? effectiveGrain : 0f);
                grain.size.Override(0.55f);
                grain.lumContrib.Override(0.8f);
            }
        }

        void DisableEffect()
        {
            if (postProcessingAvailable && vignette != null && colorGrading != null)
            {
                // Reset Vignette
                vignette.intensity.Override(0f);

                // Reset Color Grading
                colorGrading.saturation.Override(0f);
                colorGrading.contrast.Override(0f);
                colorGrading.brightness.Override(0f);
                colorGrading.colorFilter.Override(Color.white);

                if (grain != null)
                {
                    grain.intensity.Override(0f);
                }
            }

            // Restore original lighting
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
                    return new Color(0.9f, 0.9f, 1f, 1f); // Slight blue tint
                
                case VisionMode.GreenNV:
                    return new Color(0.1f, 1f, 0.3f, 1f); // Classic green
                
                case VisionMode.AmberWarm:
                    return new Color(1f, 0.65f, 0.2f, 1f); // Amber/orange
                
                default:
                    return Color.white;
            }
        }

        void TryToggleNightVision()
        {
            float now = Time.unscaledTime;
            if (now - lastToggleTime < ToggleCooldownSeconds)
                return;

            lastToggleTime = now;
            ToggleNightVision();
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

        void DrawFallbackEffectOverlay()
        {
            if (fallbackWhiteTexture == null)
                return;

            Color mode = GetModeColor(currentMode);
            float tintAlpha = Mathf.Lerp(0.08f, 0.22f, Mathf.InverseLerp(MinBrightnessMultiplier, MaxBrightnessMultiplier, brightnessMultiplier));
            GUI.color = new Color(mode.r, mode.g, mode.b, tintAlpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), fallbackWhiteTexture, ScaleMode.StretchToFill, true);

            if (vignetteEnabled)
            {
                float effectiveVignette = Mathf.Clamp01(vignetteIntensity * vignetteBoost);
                float vignetteAlpha = Mathf.Clamp01(effectiveVignette * 1.35f);
                if (fallbackVignetteTexture != null && vignetteAlpha > 0.001f)
                {
                    GUI.color = new Color(1f, 1f, 1f, vignetteAlpha);
                    GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), fallbackVignetteTexture, ScaleMode.StretchToFill, true);
                }
            }

            if (grainEnabled && fallbackGrainTexture != null)
            {
                float effectiveGrain = Mathf.Clamp(grainIntensity * grainBoost, MinGrainIntensity, MaxGrainIntensity);
                GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(effectiveGrain * 0.9f));
                float u = Screen.width / 96f;
                float v = Screen.height / 96f;
                GUI.DrawTextureWithTexCoords(new Rect(0f, 0f, Screen.width, Screen.height), fallbackGrainTexture, new Rect(0f, 0f, u, v), true);
            }

            GUI.color = Color.white;
        }

        #endregion

        #region Lighting Control

        void StoreLighting()
        {
            if (!lightingStored)
            {
                storedAmbientLight = RenderSettings.ambientLight;
                storedAmbientIntensity = RenderSettings.ambientIntensity;
                lightingStored = true;
                Debug.Log($"[KerbVisionIR] Stored lighting: {storedAmbientLight}, intensity: {storedAmbientIntensity}");
            }
        }

        void ApplyBrightnessBoost()
        {
            if (lightingStored)
            {
                if (postProcessingAvailable)
                {
                    RenderSettings.ambientLight = storedAmbientLight * brightnessMultiplier;
                }
                else
                {
                    Color boosted = storedAmbientLight * brightnessMultiplier;
                    Color tint = GetModeColor(currentMode);
                    RenderSettings.ambientLight = Color.Lerp(boosted, new Color(boosted.r * tint.r, boosted.g * tint.g, boosted.b * tint.b, boosted.a), FallbackTintStrength);
                }

                RenderSettings.ambientIntensity = storedAmbientIntensity * brightnessMultiplier;
                Debug.Log($"[KerbVisionIR] Applied brightness boost: {brightnessMultiplier}x");
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
                layer.volumeLayer = -1;
                return;
            }

            targetCamera = activeCamera;
            layer = activeCamera.GetComponent<PostProcessLayer>();
            if (layer == null)
            {
                layer = activeCamera.gameObject.AddComponent<PostProcessLayer>();
                layer.Init(null);
                layer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                layer.stopNaNPropagation = true;
                Debug.Log($"[KerbVisionIR] Added PostProcessLayer to active camera: {activeCamera.name}");
            }

            layer.enabled = true;
            layer.volumeLayer = -1;
        }

        #endregion
    }
}