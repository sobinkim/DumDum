using System;
using SB.App.Domain;

namespace SB.App.Application
{
    public sealed class RuleBasedSupportInterventionPolicy : ISupportInterventionPolicy
    {
        private static readonly TimeSpan GentleDelay = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan TomorrowDelay = TimeSpan.FromMinutes(1);

        public SupportInterventionDecision Choose(SupportInterventionContext context)
        {
            if (ShouldSuggestDailyClosure(context))
                return CreateDailyClosureSuggestion(context);

            if (ShouldSendTakeaway(context))
                return CreateTakeawayReminder(context);

            if (ShouldSendOutcomeStats(context))
                return CreateOutcomeStatsReminder(context);

            return SupportInterventionDecision.None;
        }

        private static bool ShouldSuggestDailyClosure(SupportInterventionContext context)
        {
            return context.TodayCards >= 2 && context.LatestEmotionIsHeavy;
        }

        private static bool ShouldSendTakeaway(SupportInterventionContext context)
        {
            return context.HasTakeaway && context.TakeawayCount >= 1;
        }

        private static bool ShouldSendOutcomeStats(SupportInterventionContext context)
        {
            return context.TaggedCards >= 3 && context.DidNotHappenPercent >= 50;
        }

        private static SupportInterventionDecision CreateDailyClosureSuggestion(SupportInterventionContext context)
        {
            return new SupportInterventionDecision(
                SupportInterventionType.DailyClosureSuggestion,
                "support-daily-closure",
                "오늘은 여기까지 정리해도 괜찮아요",
                "생각이 계속 이어진다면 잠깐 멈추고 내일 다시 봐도 괜찮아요.",
                NotificationRouteType.Whiteboard,
                context.LatestCardId,
                GentleDelay);
        }

        private static SupportInterventionDecision CreateTakeawayReminder(SupportInterventionContext context)
        {
            return new SupportInterventionDecision(
                SupportInterventionType.TakeawayReminder,
                "support-takeaway",
                "예전에 내가 남긴 문장이 있어요",
                $"\"{TrimForNotification(context.LatestTakeaway, 42)}\"",
                NotificationRouteType.Whiteboard,
                context.LatestCardId,
                TomorrowDelay);
        }

        private static SupportInterventionDecision CreateOutcomeStatsReminder(SupportInterventionContext context)
        {
            return new SupportInterventionDecision(
                SupportInterventionType.OutcomeStatsReminder,
                "support-outcome-stats",
                "걱정 기록을 하나 확인해볼까요?",
                $"지금까지 확인한 걱정 중 {context.DidNotHappenPercent}%는 실제로 일어나지 않았어요.",
                NotificationRouteType.Whiteboard,
                context.LatestCardId,
                TomorrowDelay);
        }

        private static string TrimForNotification(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed.Substring(0, maxLength) + "...";
        }
    }
}
