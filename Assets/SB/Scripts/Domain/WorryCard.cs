using System;
using System.Linq;

namespace SB.App.Domain
{
    [Serializable]
    public sealed class WorryCard
    {
        public string Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string WorryText { get; private set; }
        public int ProbabilityPercent { get; private set; }
        public string[] CopingActions { get; private set; }
        public string CopingPlan => CopingActions == null ? string.Empty : string.Join("\n", CopingActions);
        public string ActionPlan { get; private set; }
        public string Takeaway { get; private set; }
        public WorryEmotionState EmotionState { get; private set; }
        public WorryOutcomeTag OutcomeTag { get; private set; }
        public DateTime? OutcomeTaggedAt { get; private set; }

        private WorryCard(
            string id,
            DateTime createdAt,
            string worryText,
            int probabilityPercent,
            string[] copingActions,
            string actionPlan,
            string takeaway,
            WorryEmotionState emotionState,
            WorryOutcomeTag outcomeTag = WorryOutcomeTag.Untagged,
            DateTime? outcomeTaggedAt = null)
        {
            Id = id;
            CreatedAt = createdAt;
            WorryText = worryText;
            ProbabilityPercent = probabilityPercent;
            CopingActions = copingActions ?? Array.Empty<string>();
            ActionPlan = actionPlan;
            Takeaway = takeaway;
            EmotionState = emotionState;
            OutcomeTag = outcomeTag;
            OutcomeTaggedAt = outcomeTaggedAt;
        }

        public static WorryCard FromDraft(WorryDraft draft)
        {
            return new WorryCard(
                Guid.NewGuid().ToString("N"),
                DateTime.Now,
                draft.WorryText,
                draft.ProbabilityPercent,
                draft.CopingActions.ToArray(),
                draft.ActionPlan,
                draft.Takeaway,
                draft.EmotionState);
        }

        public static WorryCard Restore(
            string id,
            DateTime createdAt,
            string worryText,
            int probabilityPercent,
            string[] copingActions,
            string actionPlan,
            string takeaway,
            WorryEmotionState emotionState,
            WorryOutcomeTag outcomeTag,
            DateTime? outcomeTaggedAt)
        {
            return new WorryCard(
                id,
                createdAt,
                worryText,
                probabilityPercent,
                copingActions,
                actionPlan,
                takeaway,
                emotionState,
                outcomeTag,
                outcomeTaggedAt);
        }

        public void TagOutcome(WorryOutcomeTag tag)
        {
            OutcomeTag = tag;
            OutcomeTaggedAt = DateTime.Now;
        }

        public void UpdateTakeaway(string takeaway)
        {
            Takeaway = string.IsNullOrWhiteSpace(takeaway) ? string.Empty : takeaway.Trim();
        }
    }
}
