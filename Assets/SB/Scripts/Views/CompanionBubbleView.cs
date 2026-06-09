using TMPro;
using UnityEngine;

namespace SB.App.Views
{
    public sealed class CompanionBubbleView : UIScreenView
    {
        [SerializeField] private TMP_Text messageText;

        public void SetMessageText(TMP_Text message)
        {
            messageText = message;
        }

        public void ShowMessage(string message)
        {
            if (messageText != null)
                messageText.text = message ?? string.Empty;

            Show();
        }
    }
}
