using System;
using SB.App.Application;
using SB.App.Domain;
using UnityEngine;

namespace SB.App.ML
{
    public sealed class DumDumSupportTrainingEnvironment : MonoBehaviour
    {
        private const int MaxCards = 20;
        private const int MaxTodayCards = 5;
        private const int MaxTakeaways = 12;

        [SerializeField] private bool logDecisions;

        public SupportInterventionContext CreateScenario()
        {
            DateTime now = DateTime.Now;
            int scenarioType = UnityEngine.Random.Range(0, 4);

            switch (scenarioType)
            {
                case 0:
                    return CreateDailyClosureScenario(now);
                case 1:
                    return CreateTakeawayScenario(now);
                case 2:
                    return CreateOutcomeStatsScenario(now);
                default:
                    return CreateNoInterventionScenario(now);
            }
        }

        public float Evaluate(SupportInterventionContext context, SupportInterventionType selectedType)
        {
            SupportInterventionType expectedType = GetExpectedType(context);
            float reward = selectedType == expectedType ? 1f : -0.35f;

            if (selectedType != SupportInterventionType.None && selectedType != expectedType)
                reward -= 0.2f;

            if (expectedType == SupportInterventionType.None && selectedType == SupportInterventionType.None)
                reward = 0.65f;

            if (logDecisions)
                Debug.Log($"[DumDumSupportAgent] selected={selectedType}, expected={expectedType}, reward={reward:0.00}");

            return reward;
        }

        public SupportInterventionType GetExpectedType(SupportInterventionContext context)
        {
            if (context.TodayCards >= 2 && context.LatestEmotionIsHeavy)
                return SupportInterventionType.DailyClosureSuggestion;

            if (context.HasTakeaway && context.TakeawayCount >= 1)
                return SupportInterventionType.TakeawayReminder;

            if (context.TaggedCards >= 3 && context.DidNotHappenPercent >= 50)
                return SupportInterventionType.OutcomeStatsReminder;

            return SupportInterventionType.None;
        }

        public static float NormalizeCount(int value, int maxValue)
        {
            if (maxValue <= 0)
                return 0f;

            return Mathf.Clamp01(value / (float)maxValue);
        }

        public static float NormalizePercent(int percent)
        {
            return Mathf.Clamp01(percent / 100f);
        }

        public static float NormalizeEmotion(WorryEmotionState emotion)
        {
            switch (emotion)
            {
                case WorryEmotionState.StillDistressed:
                    return 1f;
                case WorryEmotionState.SlightlyRelieved:
                    return 0.5f;
                case WorryEmotionState.Calm:
                    return 0f;
                default:
                    return 0.25f;
            }
        }

        public static int MaxCardObservationCount => MaxCards;
        public static int MaxTodayCardObservationCount => MaxTodayCards;
        public static int MaxTakeawayObservationCount => MaxTakeaways;

        private static SupportInterventionContext CreateDailyClosureScenario(DateTime now)
        {
            int totalCards = UnityEngine.Random.Range(2, MaxCards + 1);
            int taggedCards = UnityEngine.Random.Range(0, totalCards + 1);
            int todayCards = UnityEngine.Random.Range(2, MaxTodayCards + 1);

            return new SupportInterventionContext(
                totalCards,
                taggedCards,
                totalCards - taggedCards,
                UnityEngine.Random.Range(0, 80),
                UnityEngine.Random.Range(0, 45),
                todayCards,
                UnityEngine.Random.Range(0, 2),
                UnityEngine.Random.Range(55, 101),
                WorryEmotionState.StillDistressed,
                string.Empty,
                "training-daily-closure",
                now);
        }

        private static SupportInterventionContext CreateTakeawayScenario(DateTime now)
        {
            int totalCards = UnityEngine.Random.Range(1, MaxCards + 1);
            int taggedCards = UnityEngine.Random.Range(1, totalCards + 1);

            return new SupportInterventionContext(
                totalCards,
                taggedCards,
                totalCards - taggedCards,
                UnityEngine.Random.Range(0, 100),
                UnityEngine.Random.Range(0, 60),
                UnityEngine.Random.Range(0, 2),
                UnityEngine.Random.Range(1, MaxTakeaways + 1),
                UnityEngine.Random.Range(10, 90),
                RandomNonHeavyEmotion(),
                "생각보다 괜찮게 지나갔다",
                "training-takeaway",
                now);
        }

        private static SupportInterventionContext CreateOutcomeStatsScenario(DateTime now)
        {
            int totalCards = UnityEngine.Random.Range(3, MaxCards + 1);
            int taggedCards = UnityEngine.Random.Range(3, totalCards + 1);

            return new SupportInterventionContext(
                totalCards,
                taggedCards,
                totalCards - taggedCards,
                UnityEngine.Random.Range(50, 101),
                UnityEngine.Random.Range(0, 40),
                UnityEngine.Random.Range(0, 2),
                0,
                UnityEngine.Random.Range(20, 90),
                RandomNonHeavyEmotion(),
                string.Empty,
                "training-outcome-stats",
                now);
        }

        private static SupportInterventionContext CreateNoInterventionScenario(DateTime now)
        {
            int totalCards = UnityEngine.Random.Range(0, 3);
            int taggedCards = totalCards <= 0 ? 0 : UnityEngine.Random.Range(0, totalCards + 1);

            return new SupportInterventionContext(
                totalCards,
                taggedCards,
                totalCards - taggedCards,
                UnityEngine.Random.Range(0, 49),
                UnityEngine.Random.Range(0, 50),
                UnityEngine.Random.Range(0, 2),
                0,
                UnityEngine.Random.Range(0, 60),
                RandomNonHeavyEmotion(),
                string.Empty,
                string.Empty,
                now);
        }

        private static WorryEmotionState RandomNonHeavyEmotion()
        {
            return UnityEngine.Random.value < 0.5f
                ? WorryEmotionState.SlightlyRelieved
                : WorryEmotionState.Calm;
        }
    }
}
