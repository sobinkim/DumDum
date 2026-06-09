using SB.App.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class StepProgressView : UIScreenView
    {
        [SerializeField] private TMP_Text stepLabel;
        [SerializeField] private Slider progressSlider;

        public void SetControls(TMP_Text label, Slider slider)
        {
            stepLabel = label;
            progressSlider = slider;
        }

        public void ShowStep(WorryFlowStep step)
        {
            Show();

            if (stepLabel != null)
                stepLabel.text = step.ToString();

            if (progressSlider != null)
                progressSlider.value = GetProgress(step);
        }

        private static float GetProgress(WorryFlowStep step)
        {
            switch (step)
            {
                case WorryFlowStep.WorryInput:
                    return 0.2f;
                case WorryFlowStep.ProbabilityEstimate:
                    return 0.4f;
                case WorryFlowStep.CopingPlan:
                    return 0.6f;
                case WorryFlowStep.ActionPlan:
                    return 0.8f;
                case WorryFlowStep.EmotionCheck:
                    return 1f;
                default:
                    return 0f;
            }
        }
    }
}
