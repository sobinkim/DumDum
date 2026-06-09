using SB.App.Application;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class SoundSettingsPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button muteButton;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TMP_Text volumeValueLabel;
        [SerializeField] private TMP_Text muteButtonLabel;

        private SoundManager _soundManager;
        private bool _isListening;
        private bool _isSyncing;

        private void Awake()
        {
            _soundManager = SoundManager.GetOrCreate();
            RegisterListeners();
            Hide();
            Refresh(_soundManager.MasterVolume, _soundManager.IsMuted);
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        public void SetControls(
            GameObject root,
            Button open,
            Button close,
            Button mute,
            Slider slider,
            TMP_Text valueLabel,
            TMP_Text muteLabel)
        {
            UnregisterListeners();

            panelRoot = root;
            openButton = open;
            closeButton = close;
            muteButton = mute;
            volumeSlider = slider;
            volumeValueLabel = valueLabel;
            muteButtonLabel = muteLabel;

            _soundManager = SoundManager.GetOrCreate();
            RegisterListeners();
            Hide();
            Refresh(_soundManager.MasterVolume, _soundManager.IsMuted);
        }

        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (_soundManager != null)
                Refresh(_soundManager.MasterVolume, _soundManager.IsMuted);
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void RegisterListeners()
        {
            if (_isListening)
                return;

            if (openButton != null)
                openButton.onClick.AddListener(Show);

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (muteButton != null)
                muteButton.onClick.AddListener(ToggleMuted);

            if (volumeSlider != null)
                volumeSlider.onValueChanged.AddListener(SetVolume);

            if (_soundManager != null)
                _soundManager.SettingsChanged += Refresh;

            _isListening = true;
        }

        private void UnregisterListeners()
        {
            if (!_isListening)
                return;

            if (openButton != null)
                openButton.onClick.RemoveListener(Show);

            if (closeButton != null)
                closeButton.onClick.RemoveListener(Hide);

            if (muteButton != null)
                muteButton.onClick.RemoveListener(ToggleMuted);

            if (volumeSlider != null)
                volumeSlider.onValueChanged.RemoveListener(SetVolume);

            if (_soundManager != null)
                _soundManager.SettingsChanged -= Refresh;

            _isListening = false;
        }

        private void SetVolume(float value)
        {
            if (_isSyncing || _soundManager == null)
                return;

            _soundManager.SetMasterVolume(value);
            if (value > 0f && _soundManager.IsMuted)
                _soundManager.SetMuted(false);
        }

        private void ToggleMuted()
        {
            if (_soundManager != null)
                _soundManager.ToggleMuted();
        }

        private void Refresh(float volume, bool isMuted)
        {
            _isSyncing = true;

            if (volumeSlider != null)
            {
                volumeSlider.minValue = 0f;
                volumeSlider.maxValue = 1f;
                volumeSlider.wholeNumbers = false;
                volumeSlider.SetValueWithoutNotify(volume);
            }

            if (volumeValueLabel != null)
                volumeValueLabel.text = Mathf.RoundToInt(volume * 100f) + "%";

            if (muteButtonLabel != null)
                muteButtonLabel.text = isMuted ? "소리 켜기" : "음소거";

            _isSyncing = false;
        }
    }
}
