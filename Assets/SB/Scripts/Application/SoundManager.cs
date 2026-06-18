using System;
using UnityEngine;

namespace SB.App.Application
{
    public sealed class SoundManager : MonoBehaviour
    {
        private const string VolumeKey = "DumDum.Sound.MasterVolume";
        private const string MutedKey = "DumDum.Sound.Muted";

        [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.8f;
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip defaultEffectClip;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.55f;
        [SerializeField, Range(0f, 1f)] private float effectVolume = 1f;

        private static SoundManager _instance;
        private AudioSource _bgmSource;
        private AudioSource _effectSource;
        private float _masterVolume;
        private bool _isMuted;

        public static SoundManager Instance => GetOrCreate();
        public float MasterVolume => _masterVolume;
        public bool IsMuted => _isMuted;

        public event Action<float, bool> SettingsChanged;

        public static SoundManager GetOrCreate()
        {
            if (_instance != null)
            {
                EnsureSoundSettingsInstaller(_instance);
                return _instance;
            }

            _instance = FindFirstObjectByType<SoundManager>();
            if (_instance != null)
            {
                EnsureSoundSettingsInstaller(_instance);
                return _instance;
            }

            GameObject root = new GameObject(nameof(SoundManager));
            _instance = root.AddComponent<SoundManager>();
            EnsureSoundSettingsInstaller(_instance);
            return _instance;
        }

        private static void EnsureSoundSettingsInstaller(SoundManager manager)
        {
            if (manager == null || manager.GetComponent<SoundSettingsInstaller>() != null)
                return;

            manager.gameObject.AddComponent<SoundSettingsInstaller>();
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
            EnsureAudioSources();
            LoadSettings();
            ApplySettings(false);
            PlayBgmIfReady();
            EnsureSoundSettingsInstaller(this);
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

        public void PlayButtonClick()
        {
            PlayEffect(buttonClickClip);
        }

        public void PlayDefaultEffect()
        {
            PlayEffect(defaultEffectClip);
        }

        public void PlayEffect(AudioClip clip)
        {
            if (clip == null)
                return;

            EnsureAudioSources();
            _effectSource.PlayOneShot(clip, effectVolume);
        }

        public void SetBgmClip(AudioClip clip, bool playImmediately = true)
        {
            bgmClip = clip;

            if (playImmediately)
                PlayBgmIfReady();
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
            EnsureAudioSources();
            AudioListener.volume = _isMuted ? 0f : _masterVolume;
            _bgmSource.volume = bgmVolume;
            _effectSource.volume = effectVolume;

            if (notify)
                SettingsChanged?.Invoke(_masterVolume, _isMuted);
        }

        private void EnsureAudioSources()
        {
            if (_bgmSource == null)
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();
                _bgmSource.playOnAwake = false;
                _bgmSource.loop = true;
            }

            if (_effectSource == null)
            {
                _effectSource = gameObject.AddComponent<AudioSource>();
                _effectSource.playOnAwake = false;
                _effectSource.loop = false;
            }
        }

        private void PlayBgmIfReady()
        {
            if (bgmClip == null)
                return;

            EnsureAudioSources();
            if (_bgmSource.clip == bgmClip && _bgmSource.isPlaying)
                return;

            _bgmSource.clip = bgmClip;
            _bgmSource.volume = bgmVolume;
            _bgmSource.Play();
        }
    }
}
