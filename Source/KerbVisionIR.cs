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
        
        // Effect references
        private Vignette vignette;
        private ColorGrading colorGrading;

        // State variables
        private bool isEffectActive = false;
        private VisionMode currentMode = VisionMode.GreenNV;
        private float brightnessMultiplier = 1.8f;
        // GUI / binding
        private Rect guiWindowRect = new Rect(10, 10, 300, 140);
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
            }
            catch (Exception ex)
            {
                Debug.LogError($"[KerbVisionIR] Initialization failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        void Update()
        {
            // Hotkey detection: user-configurable
            if (!capturingKey && boundToggleKey != KeyCode.None && Input.GetKeyDown(boundToggleKey))
            {
                if (!requireAltForBoundKey || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    ToggleNightVision();
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
        }

        void OnGUI()
        {
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
                ToggleNightVision();
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
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[KerbVisionIR] Camera.main is null!");
                return;
            }

            Debug.Log($"[KerbVisionIR] Found camera: {mainCamera.name}");

            // Load TUFX shader resources FIRST
            LoadShaderResources();

            // Get or create PostProcessLayer
            layer = mainCamera.GetComponent<PostProcessLayer>();
            if (layer == null)
            {
                layer = mainCamera.gameObject.AddComponent<PostProcessLayer>();
                layer.Init(null); // TUFX will auto-initialize resources
                layer.volumeLayer = -1; // All layers
                layer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                layer.stopNaNPropagation = true;
                Debug.Log("[KerbVisionIR] Created PostProcessLayer");
            }
            else
            {
                Debug.Log("[KerbVisionIR] Using existing PostProcessLayer");
            }

            // Create global PostProcessVolume
            GameObject volumeGO = new GameObject("KerbVisionIR_PostProcessVolume");
            volume = volumeGO.AddComponent<PostProcessVolume>();
            volume.isGlobal = true;
            volume.priority = 100f; // High priority
            volume.weight = 1f;

            // Create profile with effects
            CreateEffectProfile();
            volume.profile = profile;

            // Store original lighting
            StoreLighting();

            Debug.Log("[KerbVisionIR] PostProcessing setup complete");
        }

        void LoadShaderResources()
        {
            // Try to load TUFX shader bundle
            string[] possiblePaths = new string[]
            {
                Path.Combine(KSPUtil.ApplicationRootPath, "GameData/TUFX/Shaders/tufx-universal.ssf"),
                Path.Combine(KSPUtil.ApplicationRootPath, "GameData/KerbVisionIR/Shaders/kerbvision-pp.ssf")
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
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[KerbVisionIR] Failed to load shader bundle: {ex.Message}");
                    }
                }
            }

            Debug.LogWarning("[KerbVisionIR] No shader bundle found - effects may not work!");
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
            if (vignette == null || colorGrading == null)
            {
                Debug.LogError("[KerbVisionIR] Effects not initialized!");
                return;
            }

            // Apply Vignette (dark corners)
            vignette.intensity.Override(0.45f);
            vignette.smoothness.Override(0.35f);

            // Apply Color Grading
            colorGrading.saturation.Override(-60f); // Strong desaturation
            colorGrading.contrast.Override(25f); // Increase contrast
            colorGrading.brightness.Override(0f); // Neutral brightness in post
            colorGrading.colorFilter.Override(GetModeColor(currentMode));

            // Apply brightness boost via lighting
            ApplyBrightnessBoost();
        }

        void DisableEffect()
        {
            if (vignette == null || colorGrading == null)
                return;

            // Reset Vignette
            vignette.intensity.Override(0f);

            // Reset Color Grading
            colorGrading.saturation.Override(0f);
            colorGrading.contrast.Override(0f);
            colorGrading.brightness.Override(0f);
            colorGrading.colorFilter.Override(Color.white);

            // Restore original lighting
            RestoreLighting();
        }

        void CycleMode()
        {
            // Cycle through modes
            currentMode = (VisionMode)(((int)currentMode + 1) % Enum.GetValues(typeof(VisionMode)).Length);
            
            Debug.Log($"[KerbVisionIR] Mode changed to: {currentMode}");
            ScreenMessages.PostScreenMessage(
                $"<color=lime>[NV Mode] {currentMode}</color>", 
                2f, 
                ScreenMessageStyle.UPPER_CENTER
            );

            // Re-apply effect if active
            if (isEffectActive)
            {
                colorGrading.colorFilter.Override(GetModeColor(currentMode));
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
                RenderSettings.ambientLight = storedAmbientLight * brightnessMultiplier;
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
    }
}