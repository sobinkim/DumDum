using System;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class MainMenuView : UIScreenView
    {
        [SerializeField] private Button startButton;

        public event Action StartRequested;
        private bool _isListening;

        private void Awake()
        {
            RegisterListeners();
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        public void SetControls(Button start)
        {
            UnregisterListeners();
            startButton = start;
            RegisterListeners();
        }

        private void RegisterListeners()
        {
            if (_isListening)
                return;

            if (startButton != null)
                startButton.onClick.AddListener(RequestStart);

            _isListening = true;
        }

        private void UnregisterListeners()
        {
            if (!_isListening)
                return;

            if (startButton != null)
                startButton.onClick.RemoveListener(RequestStart);

            _isListening = false;
        }

        public void RequestStart()
        {
            StartRequested?.Invoke();
        }
    }
}
