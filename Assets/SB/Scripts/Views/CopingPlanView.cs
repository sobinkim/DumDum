using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class CopingPlanView : PromptedStepView
    {
        [SerializeField] private TMP_InputField[] actionInputs;
        [SerializeField] private Button addActionButton;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button backButton;

        private bool _isListening;
        private int _visibleInputCount = 1;

        public event Action<IReadOnlyList<string>> Submitted;
        public event Action BackRequested;

        private void Awake()
        {
            RegisterListeners();
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        public void SetControls(TMP_InputField[] inputs, Button addAction, Button submit, Button back)
        {
            UnregisterListeners();
            actionInputs = inputs;
            addActionButton = addAction;
            submitButton = submit;
            backButton = back;
            SetVisibleInputCount(1);
            RegisterListeners();
        }

        public void SetActions(IReadOnlyList<string> values)
        {
            if (actionInputs == null)
                return;

            int valueCount = values == null ? 0 : values.Count(value => !string.IsNullOrWhiteSpace(value));
            SetVisibleInputCount(Mathf.Max(1, valueCount));

            for (int i = 0; i < actionInputs.Length; i++)
            {
                if (actionInputs[i] == null)
                    continue;

                actionInputs[i].text = values != null && i < values.Count ? values[i] : string.Empty;
            }
        }

        public IReadOnlyList<string> GetActions()
        {
            if (actionInputs == null)
                return Array.Empty<string>();

            return actionInputs
                .Where(input => input != null)
                .Where(input => input.gameObject.activeSelf)
                .Select(input => input.text)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .ToArray();
        }

        private void RegisterListeners()
        {
            if (_isListening)
                return;

            if (submitButton != null)
                submitButton.onClick.AddListener(SubmitCurrentActions);

            if (addActionButton != null)
                addActionButton.onClick.AddListener(AddActionInput);

            if (backButton != null)
                backButton.onClick.AddListener(RequestBack);

            _isListening = true;
        }

        private void UnregisterListeners()
        {
            if (!_isListening)
                return;

            if (submitButton != null)
                submitButton.onClick.RemoveListener(SubmitCurrentActions);

            if (addActionButton != null)
                addActionButton.onClick.RemoveListener(AddActionInput);

            if (backButton != null)
                backButton.onClick.RemoveListener(RequestBack);

            _isListening = false;
        }

        private void SubmitCurrentActions()
        {
            Submitted?.Invoke(GetActions());
        }

        private void AddActionInput()
        {
            SetVisibleInputCount(_visibleInputCount + 1);
        }

        private void SetVisibleInputCount(int count)
        {
            if (actionInputs == null || actionInputs.Length <= 0)
                return;

            _visibleInputCount = Mathf.Clamp(count, 1, actionInputs.Length);

            for (int i = 0; i < actionInputs.Length; i++)
            {
                if (actionInputs[i] != null)
                    actionInputs[i].gameObject.SetActive(i < _visibleInputCount);
            }

            if (addActionButton != null)
                addActionButton.gameObject.SetActive(_visibleInputCount < actionInputs.Length);
        }

        private void RequestBack()
        {
            BackRequested?.Invoke();
        }
    }
}
