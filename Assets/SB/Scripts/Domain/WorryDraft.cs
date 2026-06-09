using System;
using System.Collections.Generic;
using System.Linq;

namespace SB.App.Domain
{
    [Serializable]
    public sealed class WorryDraft
    {
        public string WorryText { get; private set; } = string.Empty;
        public int ProbabilityPercent { get; private set; }
        public IReadOnlyList<string> CopingActions => _copingActions;
        public string CopingPlan => string.Join("\n", _copingActions);
        public string ActionPlan { get; private set; } = string.Empty;
        public string Takeaway { get; private set; } = string.Empty;
        public WorryEmotionState EmotionState { get; private set; } = WorryEmotionState.Unset;

        private readonly List<string> _copingActions = new List<string>();

        public void Clear()
        {
            WorryText = string.Empty;
            ProbabilityPercent = 0;
            _copingActions.Clear();
            ActionPlan = string.Empty;
            Takeaway = string.Empty;
            EmotionState = WorryEmotionState.Unset;
        }

        public void SetWorry(string value)
        {
            WorryText = Normalize(value);
        }

        public void SetProbability(int value)
        {
            ProbabilityPercent = Math.Max(0, Math.Min(100, value));
        }

        public void SetCopingPlan(string value)
        {
            SetCopingActions(new[] { value });
        }

        public void SetCopingActions(IEnumerable<string> values)
        {
            _copingActions.Clear();

            if (values == null)
                return;

            _copingActions.AddRange(values.Select(Normalize).Where(action => !string.IsNullOrWhiteSpace(action)));
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
        public bool HasCopingPlan => _copingActions.Count > 0;
        public bool HasActionPlan => !string.IsNullOrWhiteSpace(ActionPlan);
        public bool HasEmotionState => EmotionState != WorryEmotionState.Unset;

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
