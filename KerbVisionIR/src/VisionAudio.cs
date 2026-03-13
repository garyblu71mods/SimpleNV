using System.IO;
using UnityEngine;

namespace KerbVisionIR
{
    public class VisionAudio
    {
        private AudioSource audioSource;
        private AudioClip activationClip;

        public void Initialize(GameObject parent)
        {
            audioSource = parent.AddComponent<AudioSource>();
            audioSource.volume = GameSettings.UI_VOLUME;
            audioSource.spatialBlend = 0f;
            audioSource.playOnAwake = false;

            LoadSound();
        }

        private void LoadSound()
        {
            string soundPath = Path.Combine(KSPUtil.ApplicationRootPath, "GameData/KerbVisionIR/Sounds/NVon");
            
            if (GameDatabase.Instance.ExistsAudioClip(soundPath))
            {
                activationClip = GameDatabase.Instance.GetAudioClip(soundPath);
                Debug.Log("[KerbVisionIR] Activation sound loaded");
            }
            else
            {
                Debug.LogWarning($"[KerbVisionIR] Activation sound not found at {soundPath}");
            }
        }

        public void PlayActivationSound()
        {
            if (audioSource != null && activationClip != null)
            {
                audioSource.PlayOneShot(activationClip);
            }
        }

        public void Cleanup()
        {
            if (audioSource != null)
            {
                Object.Destroy(audioSource);
            }
        }
    }
}
