using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public abstract class TextInputStepView : PromptedStepView
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button backButton;

        public event Action<string> Submitted;
        public event Action BackRequested;
        private bool _isListening;

        protected virtual void Awake()
        {
            RegisterListeners();
        }

        protected virtual void OnDestroy()
        {
            UnregisterListeners();
        }

        public void SetControls(TMP_InputField input, Button submit, Button back)
        {
            UnregisterListeners();
            inputField = input;
            submitButton = submit;
            backButton = back;
            RegisterListeners();
        }

        private void RegisterListeners()
        {
            if (_isListening)
                return;

            if (submitButton != null)
                submitButton.onClick.AddListener(SubmitCurrentInput);

            if (backButton != null)
                backButton.onClick.AddListener(RequestBack);

            _isListening = true;
        }

        private void UnregisterListeners()
        {
            if (!_isListening)
                return;

            if (submitButton != null)
                submitButton.onClick.RemoveListener(SubmitCurrentInput);

            if (backButton != null)
                backButton.onClick.RemoveListener(RequestBack);

            _isListening = false;
        }

        public void SetInput(string value)
        {
            if (inputField != null)
                inputField.text = value ?? string.Empty;
        }

        public string GetInput()
        {
            return inputField != null ? inputField.text : string.Empty;
        }

        public void SubmitCurrentInput()
        {
            Submitted?.Invoke(GetInput());
        }

        public void RequestBack()
        {
            BackRequested?.Invoke();
        }
    }
}
