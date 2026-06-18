using SB.App.Domain;
using UnityEngine;

namespace SB.App.Application
{
    [CreateAssetMenu(menuName = "DumDum/Worry Flow Content", fileName = "WorryFlowContent")]
    public sealed class WorryFlowContent : ScriptableObject
    {
        [SerializeField] private int researchReferencePercent = 8;
        [SerializeField] private WorryStepContent[] steps;

        public int ResearchReferencePercent => researchReferencePercent;

        public void SetRuntimeContent(int referencePercent, WorryStepContent[] stepContents)
        {
            researchReferencePercent = referencePercent;
            steps = stepContents;
        }

        public WorryStepContent GetStep(WorryFlowStep step)
        {
            if (steps == null)
                return CreateFallback(step);

            for (int i = 0; i < steps.Length; i++)
            {
                if (steps[i] != null && steps[i].step == step)
                    return steps[i];
            }

            return CreateFallback(step);
        }

        private static WorryStepContent CreateFallback(WorryFlowStep step)
        {
            return new WorryStepContent
            {
                step = step,
                title = GetFallbackTitle(step),
                companionMessage = string.Empty,
                primaryActionLabel = step == WorryFlowStep.EmotionCheck ? "저장" : "다음",
                secondaryActionLabel = step == WorryFlowStep.WorryInput ? "취소" : "이전"
            };
        }

        public static string GetFallbackTitle(WorryFlowStep step)
        {
            switch (step)
            {
                case WorryFlowStep.Whiteboard:
                    return "증거 화이트보드";
                case WorryFlowStep.WorryInput:
                    return "고민 적기";
                case WorryFlowStep.FactCheck:
                    return "사실과 추측 나누기";
                case WorryFlowStep.ThoughtCheck:
                    return "생각 점검하기";
                case WorryFlowStep.ActionPlan:
                    return "지금 할 수 있는 행동 정하기";
                case WorryFlowStep.TakeawaySummary:
                    return "한 줄로 정리하기";
                case WorryFlowStep.EmotionCheck:
                    return "마음 상태 확인하기";
                case WorryFlowStep.CardSaved:
                    return "저장 완료";
                case WorryFlowStep.CardDetail:
                    return "고민 카드";
                case WorryFlowStep.OutcomeReview:
                    return "실제 결과 확인하기";
                default:
                    return "고민 정리";
            }
        }
    }
}
