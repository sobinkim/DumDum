using System;
using UnityEngine;

namespace SB.App.Application
{
    public sealed class SoundManager : MonoBehaviour
    {
        private const string VolumeKey = "DumDum.Sound.MasterVolume";
        private const string MutedKey = "DumDum.Sound.Muted";

        [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.8f;

        private static SoundManager _instance;
        private float _masterVolume;
        private bool _isMuted;

        public static SoundManager Instance => GetOrCreate();
        public float MasterVolume => _masterVolume;
        public bool IsMuted => _isMuted;

        public event Action<float, bool> SettingsChanged;

        public static SoundManager GetOrCreate()
        {
            if (_instance != null)
                return _instance;

            _instance = FindFirstObjectByType<SoundManager>();
            if (_instance != null)
                return _instance;

            GameObject root = new GameObject(nameof(SoundManager));
            _instance = root.AddComponent<SoundManager>();
            return _instance;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
            ApplySettings(false);
        }

        public void SetMasterVolume(float volume)
        {
            float clamped = Mathf.Clamp01(volume);
            if (Mathf.Approximately(_masterVolume, clamped))
                return;

            _masterVolume = clamped;
            SaveSettings();
            ApplySettings(true);
        }

        public void SetMuted(bool muted)
        {
            if (_isMuted == muted)
                return;

            _isMuted = muted;
            SaveSettings();
            ApplySettings(true);
        }

        public void ToggleMuted()
        {
            SetMuted(!_isMuted);
        }

        private void LoadSettings()
        {
            _masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, defaultVolume));
            _isMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat(VolumeKey, _masterVolume);
            PlayerPrefs.SetInt(MutedKey, _isMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void ApplySettings(bool notify)
        {
            AudioListener.volume = _isMuted ? 0f : _masterVolume;

            if (notify)
                SettingsChanged?.Invoke(_masterVolume, _isMuted);
        }
    }
}
