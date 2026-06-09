using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class FeedbackPopupView : UIScreenView
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button closeButton;
        private bool _isListening;

        private void Awake()
        {
            RegisterListeners();
            Hide();
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        public void SetControls(TMP_Text message, Button close)
        {
            UnregisterListeners();
            messageText = message;
            closeButton = close;
            RegisterListeners();
        }

        private void RegisterListeners()
        {
            if (_isListening)
                return;

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            _isListening = true;
        }

        private void UnregisterListeners()
        {
            if (!_isListening)
                return;

            if (closeButton != null)
                closeButton.onClick.RemoveListener(Hide);

            _isListening = false;
        }

        public void ShowMessage(string message)
        {
            if (messageText != null)
                messageText.text = message ?? string.Empty;

            Show();
        }
    }
}
