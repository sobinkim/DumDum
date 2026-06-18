using SB.App.Application;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class SoundSettingsPanelView : MonoBehaviour
    {
        private const string SoundOnIconPath = "Sound/soundPlayIcon";
        private const string MutedIconPath = "Sound/muteIcon";

        [SerializeField] private Button toggleButton;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Sprite soundOnSprite;
        [SerializeField] private Sprite mutedSprite;

        [SerializeField, HideInInspector] private Button openButton;

        private SoundManager _soundManager;
        private bool _isListening;

        private void Awake()
        {
            UpgradeLegacyReferences();
            LoadIcons();
            _soundManager = SoundManager.GetOrCreate();
            RegisterListeners();
            Refresh(_soundManager.MasterVolume, _soundManager.IsMuted);
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        public void SetControls(Button button)
        {
            UnregisterListeners();

            toggleButton = button;
            buttonImage = button != null ? button.targetGraphic as Image : null;

            LoadIcons();
            CleanButtonVisuals();
            _soundManager = SoundManager.GetOrCreate();
            RegisterListeners();
            Refresh(_soundManager.MasterVolume, _soundManager.IsMuted);
        }

        private void RegisterListeners()
        {
            if (_isListening)
                return;

            if (toggleButton != null)
                toggleButton.onClick.AddListener(ToggleMuted);

            if (_soundManager != null)
                _soundManager.SettingsChanged += Refresh;

            _isListening = true;
        }

        private void UnregisterListeners()
        {
            if (!_isListening)
                return;

            if (toggleButton != null)
                toggleButton.onClick.RemoveListener(ToggleMuted);

            if (_soundManager != null)
                _soundManager.SettingsChanged -= Refresh;

            _isListening = false;
        }

        private void ToggleMuted()
        {
            if (_soundManager == null)
                return;

            _soundManager.PlayButtonClick();
            _soundManager.ToggleMuted();
        }

        private void Refresh(float volume, bool isMuted)
        {
            if (buttonImage != null)
            {
                Sprite targetSprite = isMuted ? mutedSprite : soundOnSprite;
                if (targetSprite != null)
                    buttonImage.sprite = targetSprite;

                buttonImage.color = Color.white;
                buttonImage.preserveAspect = true;
            }

            CleanButtonVisuals();
        }

        private void UpgradeLegacyReferences()
        {
            if (toggleButton == null && openButton != null)
                toggleButton = openButton;

            if (toggleButton == null)
            {
                Button[] buttons = GetComponentsInChildren<Button>(true);
                if (buttons.Length > 0)
                    toggleButton = buttons[0];
            }

            if (buttonImage == null && toggleButton != null)
                buttonImage = toggleButton.targetGraphic as Image;
        }

        private void LoadIcons()
        {
            if (soundOnSprite == null)
                soundOnSprite = Resources.Load<Sprite>(SoundOnIconPath);

            if (mutedSprite == null)
                mutedSprite = Resources.Load<Sprite>(MutedIconPath);
        }

        private void CleanButtonVisuals()
        {
            if (toggleButton == null)
                return;

            TMP_Text[] labels = toggleButton.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] != null)
                    labels[i].gameObject.SetActive(false);
            }

            Outline[] outlines = toggleButton.GetComponents<Outline>();
            for (int i = 0; i < outlines.Length; i++)
            {
                if (outlines[i] != null)
                    outlines[i].enabled = false;
            }

            Image targetImage = buttonImage != null ? buttonImage : toggleButton.targetGraphic as Image;
            if (targetImage != null)
            {
                buttonImage = targetImage;
                buttonImage.color = Color.white;
                buttonImage.preserveAspect = true;
            }
        }
    }
}
