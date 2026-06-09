using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class ActionPlanView : PromptedStepView
    {
        [SerializeField] private TMP_InputField actionInput;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button backButton;

        private bool _isListening;

        public event Action<string> Submitted;
        public event Action BackRequested;

        private void Awake()
        {
            RegisterListeners();
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        public void SetControls(TMP_InputField action, Button submit, Button back)
        {
            UnregisterListeners();
            actionInput = action;
            submitButton = submit;
            backButton = back;
            RegisterListeners();
        }

        public void SetInput(string action)
        {
            if (actionInput != null)
                actionInput.text = action ?? string.Empty;
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
            string action = actionInput != null ? actionInput.text : string.Empty;
            Submitted?.Invoke(action);
        }

        private void RequestBack()
        {
            BackRequested?.Invoke();
        }
    }
}
