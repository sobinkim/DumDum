using System;

namespace SB.App.Domain
{
    [Serializable]
    public sealed class WorryCard
    {
        public string Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string WorryText { get; private set; }
        public string FactsText { get; private set; }
        public string AssumptionsText { get; private set; }
        public string EvidenceText { get; private set; }
        public string CounterEvidenceText { get; private set; }
        public string AlternativeThoughtText { get; private set; }
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
            string factsText,
            string assumptionsText,
            string evidenceText,
            string counterEvidenceText,
            string alternativeThoughtText,
            string actionPlan,
            string takeaway,
            WorryEmotionState emotionState,
            WorryOutcomeTag outcomeTag = WorryOutcomeTag.Untagged,
            DateTime? outcomeTaggedAt = null,
            int probabilityPercent = 0,
            string[] copingActions = null)
        {
            Id = id;
            CreatedAt = createdAt;
            WorryText = Normalize(worryText);
            FactsText = Normalize(factsText);
            AssumptionsText = Normalize(assumptionsText);
            EvidenceText = Normalize(evidenceText);
            CounterEvidenceText = Normalize(counterEvidenceText);
            AlternativeThoughtText = Normalize(alternativeThoughtText);
            ProbabilityPercent = probabilityPercent;
            CopingActions = copingActions ?? Array.Empty<string>();
            ActionPlan = Normalize(actionPlan);
            Takeaway = Normalize(takeaway);
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
                draft.FactsText,
                draft.AssumptionsText,
                draft.EvidenceText,
                draft.CounterEvidenceText,
                draft.AlternativeThoughtText,
                draft.ActionPlan,
                draft.Takeaway,
                draft.EmotionState);
        }

        public static WorryCard Restore(
            string id,
            DateTime createdAt,
            string worryText,
            string factsText,
            string assumptionsText,
            string evidenceText,
            string counterEvidenceText,
            string alternativeThoughtText,
            string actionPlan,
            string takeaway,
            WorryEmotionState emotionState,
            WorryOutcomeTag outcomeTag,
            DateTime? outcomeTaggedAt)
        {
            return Restore(
                id,
                createdAt,
                worryText,
                factsText,
                assumptionsText,
                evidenceText,
                counterEvidenceText,
                alternativeThoughtText,
                actionPlan,
                takeaway,
                emotionState,
                outcomeTag,
                outcomeTaggedAt,
                0,
                Array.Empty<string>());
        }

        public static WorryCard Restore(
            string id,
            DateTime createdAt,
            string worryText,
            string factsText,
            string assumptionsText,
            string evidenceText,
            string counterEvidenceText,
            string alternativeThoughtText,
            string actionPlan,
            string takeaway,
            WorryEmotionState emotionState,
            WorryOutcomeTag outcomeTag,
            DateTime? outcomeTaggedAt,
            int probabilityPercent,
            string[] copingActions)
        {
            return new WorryCard(
                id,
                createdAt,
                worryText,
                factsText,
                assumptionsText,
                evidenceText,
                counterEvidenceText,
                alternativeThoughtText,
                actionPlan,
                takeaway,
                emotionState,
                outcomeTag,
                outcomeTaggedAt,
                probabilityPercent,
                copingActions);
        }

        public void TagOutcome(WorryOutcomeTag tag)
        {
            OutcomeTag = tag;
            OutcomeTaggedAt = DateTime.Now;
        }

        public void UpdateTakeaway(string takeaway)
        {
            Takeaway = Normalize(takeaway);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
