using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class FactCheckView : PromptedStepView
    {
        [SerializeField] private TMP_InputField factsInput;
        [SerializeField] private TMP_InputField assumptionsInput;
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

        public void SetControls(TMP_InputField facts, TMP_InputField assumptions, Button submit, Button back)
        {
            UnregisterListeners();
            factsInput = facts;
            assumptionsInput = assumptions;
            submitButton = submit;
            backButton = back;
            RegisterListeners();
        }

        public void SetInputs(string facts, string assumptions)
        {
            if (factsInput != null)
                factsInput.text = facts ?? string.Empty;

            if (assumptionsInput != null)
                assumptionsInput.text = assumptions ?? string.Empty;
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
            string facts = factsInput != null ? factsInput.text : string.Empty;
            string assumptions = assumptionsInput != null ? assumptionsInput.text : string.Empty;
            Submitted?.Invoke(facts, assumptions);
        }

        private void RequestBack()
        {
            BackRequested?.Invoke();
        }
    }
}
