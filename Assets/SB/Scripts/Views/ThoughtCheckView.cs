using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class ThoughtCheckView : PromptedStepView
    {
        [SerializeField] private TMP_InputField evidenceInput;
        [SerializeField] private TMP_InputField counterEvidenceInput;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button backButton;

        private bool _isListening;

        public event Action<string, string> Submitted;
        public event Action BackRequested;

        private void Awake()
        {
            RegisterListeners();
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        public void SetControls(
            TMP_InputField evidence,
            TMP_InputField counterEvidence,
            Button submit,
            Button back)
        {
            UnregisterListeners();
            evidenceInput = evidence;
            counterEvidenceInput = counterEvidence;
            submitButton = submit;
            backButton = back;
            RegisterListeners();
        }

        public void SetInputs(string evidence, string counterEvidence)
        {
            if (evidenceInput != null)
                evidenceInput.text = evidence ?? string.Empty;

            if (counterEvidenceInput != null)
                counterEvidenceInput.text = string.Empty;
        }

        private void RegisterListeners()
        {
            if (_isListening)
                return;

            if (submitButton != null)
                submitButton.onClick.AddListener(SubmitCurrentInputs);

            if (backButton != null)
                backButton.onClick.AddListener(RequestBack);

            _isListening = true;
        }

        private void UnregisterListeners()
        {
            if (!_isListening)
                return;

            if (submitButton != null)
                submitButton.onClick.RemoveListener(SubmitCurrentInputs);

            if (backButton != null)
                backButton.onClick.RemoveListener(RequestBack);

            _isListening = false;
        }

        private void SubmitCurrentInputs()
        {
            string evidence = evidenceInput != null ? evidenceInput.text : string.Empty;
            Submitted?.Invoke(evidence, string.Empty);
        }

        private void RequestBack()
        {
            BackRequested?.Invoke();
        }
    }
}
