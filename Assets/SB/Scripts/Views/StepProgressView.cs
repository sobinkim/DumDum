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
                stepLabel.text = GetLabel(step);

            if (progressSlider != null)
                progressSlider.value = GetProgress(step);
        }

        private static float GetProgress(WorryFlowStep step)
        {
            switch (step)
            {
                case WorryFlowStep.WorryInput:
                    return 1f / 6f;
                case WorryFlowStep.FactCheck:
                    return 2f / 6f;
                case WorryFlowStep.ThoughtCheck:
                    return 3f / 6f;
                case WorryFlowStep.ActionPlan:
                    return 4f / 6f;
                case WorryFlowStep.TakeawaySummary:
                    return 5f / 6f;
                case WorryFlowStep.EmotionCheck:
                    return 1f;
                default:
                    return 0f;
            }
        }

        private static string GetLabel(WorryFlowStep step)
        {
            switch (step)
            {
                case WorryFlowStep.WorryInput:
                    return "1/6 고민";
                case WorryFlowStep.FactCheck:
                    return "2/6 사실";
                case WorryFlowStep.ThoughtCheck:
                    return "3/6 점검";
                case WorryFlowStep.ActionPlan:
                    return "4/6 행동";
                case WorryFlowStep.TakeawaySummary:
                    return "5/6 정리";
                case WorryFlowStep.EmotionCheck:
                    return "6/6 감정";
                default:
                    return "사고 훈련";
            }
        }
    }
}
