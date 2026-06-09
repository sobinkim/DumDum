using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class ProbabilityEstimateView : PromptedStepView
    {
        [SerializeField] private Slider probabilitySlider;
        [SerializeField] private TMP_Text probabilityLabel;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button backButton;

        public event Action<int> Submitted;
        public event Action BackRequested;
        private bool _isListening;

        private void Awake()
        {
            RegisterListeners();
            RefreshLabel();
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        public void SetControls(Slider slider, TMP_Text label, Button submit, Button back)
        {
            UnregisterListeners();
            probabilitySlider = slider;
            probabilityLabel = label;
            submitButton = submit;
            backButton = back;
            RegisterListeners();
            RefreshLabel();
        }

        private void RegisterListeners()
        {
            if (_isListening)
                return;

            if (probabilitySlider != null)
                probabilitySlider.onValueChanged.AddListener(HandleSliderChanged);

            if (submitButton != null)
                submitButton.onClick.AddListener(SubmitCurrentValue);

            if (backButton != null)
                backButton.onClick.AddListener(RequestBack);

            _isListening = true;
        }

        private void UnregisterListeners()
        {
            if (!_isListening)
                return;

            if (probabilitySlider != null)
                probabilitySlider.onValueChanged.RemoveListener(HandleSliderChanged);

            if (submitButton != null)
                submitButton.onClick.RemoveListener(SubmitCurrentValue);

            if (backButton != null)
                backButton.onClick.RemoveListener(RequestBack);

            _isListening = false;
        }

        public void SetProbability(int percent)
        {
            if (probabilitySlider == null)
                return;

            probabilitySlider.value = percent;
            RefreshLabel();
        }

        public int GetProbability()
        {
            return probabilitySlider != null ? Mathf.RoundToInt(probabilitySlider.value) : 0;
        }

        public void SubmitCurrentValue()
        {
            Submitted?.Invoke(GetProbability());
        }

        public void RequestBack()
        {
            BackRequested?.Invoke();
        }

        private void HandleSliderChanged(float value)
        {
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (probabilityLabel != null)
                probabilityLabel.text = $"{GetProbability()}%";
        }
    }
}
