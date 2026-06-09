using System;
using SB.App.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class EmotionCheckView : PromptedStepView
    {
        [SerializeField] private Button stillDistressedButton;
        [SerializeField] private Button slightlyRelievedButton;
        [SerializeField] private Button calmButton;
        [SerializeField] private Button backButton;

        public event Action<WorryEmotionState> EmotionSelected;
        public event Action BackRequested;
        private bool _isListening;

        private void Awake()
        {
            RegisterListeners();
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        public void SetControls(Button stillDistressed, Button slightlyRelieved, Button calm, Button back)
        {
            UnregisterListeners();
            stillDistressedButton = stillDistressed;
            slightlyRelievedButton = slightlyRelieved;
            calmButton = calm;
            backButton = back;
            RegisterListeners();
        }

        private void RegisterListeners()
        {
            if (_isListening)
                return;

            if (stillDistressedButton != null)
                stillDistressedButton.onClick.AddListener(SelectStillDistressed);

            if (slightlyRelievedButton != null)
                slightlyRelievedButton.onClick.AddListener(SelectSlightlyRelieved);

            if (calmButton != null)
                calmButton.onClick.AddListener(SelectCalm);

            if (backButton != null)
                backButton.onClick.AddListener(RequestBack);

            _isListening = true;
        }

        private void UnregisterListeners()
        {
            if (!_isListening)
                return;

            if (stillDistressedButton != null)
                stillDistressedButton.onClick.RemoveListener(SelectStillDistressed);

            if (slightlyRelievedButton != null)
                slightlyRelievedButton.onClick.RemoveListener(SelectSlightlyRelieved);

            if (calmButton != null)
                calmButton.onClick.RemoveListener(SelectCalm);

            if (backButton != null)
                backButton.onClick.RemoveListener(RequestBack);

            _isListening = false;
        }

        public void SelectStillDistressed()
        {
            EmotionSelected?.Invoke(WorryEmotionState.StillDistressed);
        }

        public void SelectSlightlyRelieved()
        {
            EmotionSelected?.Invoke(WorryEmotionState.SlightlyRelieved);
        }

        public void SelectCalm()
        {
            EmotionSelected?.Invoke(WorryEmotionState.Calm);
        }

        public void RequestBack()
        {
            BackRequested?.Invoke();
        }
    }
}
