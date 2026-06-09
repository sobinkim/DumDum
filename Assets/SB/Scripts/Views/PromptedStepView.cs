using SB.App.Application;
using TMPro;
using UnityEngine;

namespace SB.App.Views
{
    public abstract class PromptedStepView : UIScreenView
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text companionText;

        public void SetPromptTexts(TMP_Text title, TMP_Text companion)
        {
            titleText = title;
            companionText = companion;
        }

        public virtual void ApplyContent(WorryStepContent content)
        {
            if (content == null)
                return;

            if (titleText != null)
                titleText.text = content.title;

            if (companionText != null)
                companionText.text = content.companionMessage;
        }
    }
}
