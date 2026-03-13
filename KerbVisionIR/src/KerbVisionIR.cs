using UnityEngine;
using KerbVisionIR.PostProcessing;

namespace KerbVisionIR
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbVisionIR : MonoBehaviour
    {
        private static KerbVisionIR instance;

        public VisionSettings Settings { get; private set; }
        private VisionPostProcessBridge postProcessBridge;
        private VisionLightingController lightingController;
        private VisionAudio audioHandler;
        private VisionSettingsWindow settingsWindow;
        private ToolbarButton toolbarButton;
        private SkyboxColorCorrection skyboxCorrection;

        private PostProcessLayer postProcessLayer;
        private Camera mainCamera;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            Debug.Log("[KerbVisionIR] Awake - Initializing...");

            Settings = VisionConfig.Load();
            postProcessBridge = new VisionPostProcessBridge();
            lightingController = new VisionLightingController();
            audioHandler = new VisionAudio();
            audioHandler.Initialize(gameObject);

            settingsWindow = new VisionSettingsWindow(Settings, OnSettingsChanged);
            toolbarButton = new ToolbarButton();
            toolbarButton.Initialize(ToggleEffect, () => settingsWindow.Show());

            Debug.Log("[KerbVisionIR] Awake complete");
        }

        void Start()
        {
            InitializePostProcessing();
            lightingController.StoreOriginalLighting();

            // Initialize skybox correction
            if (mainCamera != null)
            {
                skyboxCorrection = new SkyboxColorCorrection();
                skyboxCorrection.Initialize(mainCamera);
                Debug.Log("[KerbVisionIR] Skybox correction initialized");
            }

            if (Settings.IsEnabled)
            {
                ApplyEffect();
            }

            Debug.Log("[KerbVisionIR] Initialization complete");
        }

        void InitializePostProcessing()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("[KerbVisionIR] Main camera not found - will retry later");
                return;
            }

            // Try to initialize post-processing, but don't fail if shader is missing
            try
            {
                if (PostProcessResources.instance.uberShader == null)
                {
                    Debug.LogWarning("[KerbVisionIR] TUFX shader not found - running in FALLBACK MODE (lighting only)");
                    Debug.LogWarning("[KerbVisionIR] Install TUFX and copy shader for full effects (vignette, grain)");
                    return;
                }

                postProcessLayer = mainCamera.gameObject.GetComponent<PostProcessLayer>();
                if (postProcessLayer == null)
                {
                    postProcessLayer = mainCamera.gameObject.AddComponent<PostProcessLayer>();
                    Debug.Log("[KerbVisionIR] PostProcessLayer added to camera");
                }

                var profile = postProcessBridge.CreateProfile(Settings);
                postProcessLayer.sharedProfile = profile;

                Debug.Log("[KerbVisionIR] Post-processing initialized (FULL MODE with effects)");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[KerbVisionIR] Post-processing initialization failed - FALLBACK MODE");
                Debug.LogWarning("[KerbVisionIR] " + ex.Message);
                postProcessLayer = null;
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(Settings.ToggleKey))
            {
                if (!Settings.RequireAlt || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    ToggleEffect();
                }
            }

            if (Input.GetKeyDown(KeyCode.F8) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                settingsWindow.Toggle();
            }

            if (Settings.IsEnabled)
            {
                // Update lighting continuously
                Color tintColor = Settings.GetTintColor();
                lightingController.ApplyBrightness(Settings.Brightness, tintColor);
                
                // Update skybox tint
                if (skyboxCorrection != null)
                {
                    skyboxCorrection.ApplyTint(tintColor, Settings.Brightness);
                }
            }
        }

        void OnGUI()
        {
            settingsWindow.OnGUI();
        }

        public void ToggleEffect()
        {
            Settings.IsEnabled = !Settings.IsEnabled;

            if (Settings.IsEnabled)
            {
                ApplyEffect();
            }
            else
            {
                RemoveEffect();
            }

            audioHandler.PlayActivationSound();
            VisionConfig.Save(Settings);

            Debug.Log($"[KerbVisionIR] Effect {(Settings.IsEnabled ? "enabled" : "disabled")}");
        }

        private void ApplyEffect()
        {
            // Update post-processing if available
            if (postProcessLayer != null && postProcessLayer.sharedProfile != null)
            {
                postProcessBridge.UpdateProfile(Settings);
                
                foreach (var setting in postProcessLayer.sharedProfile.settings)
                {
                    setting.active = true;
                }

                Debug.Log("[KerbVisionIR] Post-processing effects activated");
            }
            else
            {
                Debug.Log("[KerbVisionIR] Running in FALLBACK MODE - lighting only (no vignette/grain)");
            }

            // Always apply lighting (works without post-processing)
            Color tintColor = Settings.GetTintColor();
            lightingController.ApplyBrightness(Settings.Brightness, tintColor);
            
            // Apply skybox tint
            if (skyboxCorrection != null)
            {
                skyboxCorrection.ApplyTint(tintColor, Settings.Brightness);
            }
        }

        private void RemoveEffect()
        {
            // Disable post-processing if available
            if (postProcessLayer != null && postProcessLayer.sharedProfile != null)
            {
                foreach (var setting in postProcessLayer.sharedProfile.settings)
                {
                    setting.active = false;
                }
            }

            // Always restore lighting
            lightingController.RestoreOriginalLighting();
            
            // Restore skybox
            if (skyboxCorrection != null)
            {
                skyboxCorrection.RestoreOriginal();
            }
        }

        private void OnSettingsChanged()
        {
            if (Settings.IsEnabled)
            {
                // Update post-processing if available
                if (postProcessLayer != null && postProcessLayer.sharedProfile != null)
                {
                    postProcessBridge.UpdateProfile(Settings);
                }

                // Always update lighting and skybox
                Color tintColor = Settings.GetTintColor();
                lightingController.ApplyBrightness(Settings.Brightness, tintColor);
                
                if (skyboxCorrection != null)
                {
                    skyboxCorrection.ApplyTint(tintColor, Settings.Brightness);
                }
            }
        }

        void OnDestroy()
        {
            if (Settings.IsEnabled)
            {
                lightingController.RestoreOriginalLighting();
            }

            audioHandler?.Cleanup();
            toolbarButton?.Cleanup();

            if (postProcessLayer != null)
            {
                Destroy(postProcessLayer);
            }

            VisionConfig.Save(Settings);

            if (instance == this)
            {
                instance = null;
            }

            Debug.Log("[KerbVisionIR] Cleanup complete");
        }
    }
}
