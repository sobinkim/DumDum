using System;
using SB.App.Domain;
using UnityEngine;

namespace SB.App.Application
{
    public sealed class SupportInterventionService
    {
        private const string LastScheduledPrefix = "DumDum.SupportIntervention.LastScheduledUtcTicks.";
        private static readonly TimeSpan TypeCooldown = TimeSpan.FromHours(20);

        private readonly MobileNotificationService _notificationService;
        private readonly ISupportInterventionPolicy _policy;

        public SupportInterventionService(
            MobileNotificationService notificationService,
            ISupportInterventionPolicy policy = null)
        {
            _notificationService = notificationService;
            _policy = policy ?? new RuleBasedSupportInterventionPolicy();
        }

        public bool ScheduleBestReminder(WorryRepository repository, WorryCard focusCard)
        {
            if (repository == null || _notificationService == null)
                return false;

            SupportInterventionContext context = CreateContext(repository, focusCard);
            SupportInterventionDecision decision = _policy.Choose(context);
            if (!decision.HasNotification || IsCoolingDown(decision.Type, context.Now))
                return false;

            bool scheduled = _notificationService.ScheduleSupportIntervention(decision, context.Now);
            if (scheduled)
                StoreScheduledTime(decision.Type, context.Now);

            return scheduled;
        }

        private static SupportInterventionContext CreateContext(WorryRepository repository, WorryCard focusCard)
        {
            DateTime now = DateTime.Now;
            WorryOutcomeSummary summary = WorryOutcomeSummary.FromCards(repository.Cards);
            WorryCard latestCard = focusCard ?? FindLatestCard(repository);
            WorryCard takeawayCard = FindLatestTakeawayCard(repository, latestCard);

            int todayCards = 0;
            int takeawayCount = 0;
            for (int i = 0; i < repository.Cards.Count; i++)
            {
                WorryCard card = repository.Cards[i];
                if (card == null)
                    continue;

                if (card.CreatedAt.Date == now.Date)
                    todayCards++;

                if (!string.IsNullOrWhiteSpace(card.Takeaway))
                    takeawayCount++;
            }

            return new SupportInterventionContext(
                summary.TotalCount,
                summary.TaggedCount,
                summary.UntaggedCount,
                summary.DidNotHappenPercent,
                summary.HappenedPercent,
                todayCards,
                takeawayCount,
                latestCard != null ? latestCard.ProbabilityPercent : 0,
                latestCard != null ? latestCard.EmotionState : WorryEmotionState.Unset,
                takeawayCard != null ? takeawayCard.Takeaway : string.Empty,
                latestCard != null ? latestCard.Id : string.Empty,
                now);
        }

        private static WorryCard FindLatestCard(WorryRepository repository)
        {
            WorryCard latest = null;
            for (int i = 0; i < repository.Cards.Count; i++)
            {
                WorryCard card = repository.Cards[i];
                if (card == null)
                    continue;

                if (latest == null || card.CreatedAt > latest.CreatedAt)
                    latest = card;
            }

            return latest;
        }

        private static WorryCard FindLatestTakeawayCard(WorryRepository repository, WorryCard preferredCard)
        {
            if (preferredCard != null && !string.IsNullOrWhiteSpace(preferredCard.Takeaway))
                return preferredCard;

            WorryCard latest = null;
            for (int i = 0; i < repository.Cards.Count; i++)
            {
                WorryCard card = repository.Cards[i];
                if (card == null || string.IsNullOrWhiteSpace(card.Takeaway))
                    continue;

                if (latest == null || card.OutcomeTaggedAt.GetValueOrDefault(card.CreatedAt) > latest.OutcomeTaggedAt.GetValueOrDefault(latest.CreatedAt))
                    latest = card;
            }

            return latest;
        }

        private static bool IsCoolingDown(SupportInterventionType type, DateTime now)
        {
            string key = LastScheduledPrefix + type;
            string rawTicks = PlayerPrefs.GetString(key, string.Empty);
            if (!long.TryParse(rawTicks, out long ticks) || ticks <= 0)
                return false;

            DateTime lastScheduled = new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
            return now - lastScheduled < TypeCooldown;
        }

        private static void StoreScheduledTime(SupportInterventionType type, DateTime now)
        {
            string key = LastScheduledPrefix + type;
            PlayerPrefs.SetString(key, now.ToUniversalTime().Ticks.ToString());
            PlayerPrefs.Save();
        }
    }
}
