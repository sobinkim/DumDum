namespace SB.App.Domain
{
    public static class WorryDisplayText
    {
        public static string ToLabel(this WorryEmotionState state)
        {
            switch (state)
            {
                case WorryEmotionState.StillDistressed:
                    return "아직 심란함";
                case WorryEmotionState.SlightlyRelieved:
                    return "조금 편해짐";
                case WorryEmotionState.Calm:
                    return "이제 괜찮음";
                default:
                    return "선택 안 함";
            }
        }

        public static string ToLabel(this WorryOutcomeTag tag)
        {
            switch (tag)
            {
                case WorryOutcomeTag.DidNotHappen:
                    return "일어나지 않음";
                case WorryOutcomeTag.PartiallyHappened:
                    return "일부만 일어남";
                case WorryOutcomeTag.Happened:
                    return "실제로 일어남";
                default:
                    return "확인 필요";
            }
        }
    }
}
