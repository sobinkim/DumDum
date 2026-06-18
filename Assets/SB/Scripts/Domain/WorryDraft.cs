using System;

namespace SB.App.Domain
{
    [Serializable]
    public sealed class WorryDraft
    {
        public string WorryText { get; private set; } = string.Empty;
        public string FactsText { get; private set; } = string.Empty;
        public string AssumptionsText { get; private set; } = string.Empty;
        public string EvidenceText { get; private set; } = string.Empty;
        public string CounterEvidenceText { get; private set; } = string.Empty;
        public string ActionPlan { get; private set; } = string.Empty;
        public string Takeaway { get; private set; } = string.Empty;
        public WorryEmotionState EmotionState { get; private set; } = WorryEmotionState.Unset;

        public void Clear()
        {
            WorryText = string.Empty;
            FactsText = string.Empty;
            AssumptionsText = string.Empty;
            EvidenceText = string.Empty;
            CounterEvidenceText = string.Empty;
            ActionPlan = string.Empty;
            Takeaway = string.Empty;
            EmotionState = WorryEmotionState.Unset;
        }

        public void SetWorry(string value)
        {
            WorryText = Normalize(value);
        }

        public void SetFactCheck(string facts, string assumptions)
        {
            FactsText = Normalize(facts);
            AssumptionsText = Normalize(assumptions);
        }

        public void SetThoughtCheck(string evidence, string counterEvidence)
        {
            EvidenceText = Normalize(evidence);
            CounterEvidenceText = Normalize(counterEvidence);
        }

        public void SetActionPlan(string value)
        {
            ActionPlan = Normalize(value);
        }

        public void SetTakeaway(string value)
        {
            Takeaway = Normalize(value);
        }

        public void SetEmotionState(WorryEmotionState value)
        {
            EmotionState = value;
        }

        public bool HasWorry => !string.IsNullOrWhiteSpace(WorryText);
        public bool HasFactCheck => !string.IsNullOrWhiteSpace(FactsText) && !string.IsNullOrWhiteSpace(AssumptionsText);
        public bool HasThoughtCheck =>
            !string.IsNullOrWhiteSpace(EvidenceText) ||
            !string.IsNullOrWhiteSpace(CounterEvidenceText);
        public bool HasActionPlan => !string.IsNullOrWhiteSpace(ActionPlan);
        public bool HasTakeaway => !string.IsNullOrWhiteSpace(Takeaway);
        public bool HasEmotionState => EmotionState != WorryEmotionState.Unset;

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
