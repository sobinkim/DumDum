using System;
using SB.App.Domain;

namespace SB.App.Application
{
    public readonly struct NotificationRuleContext
    {
        public static readonly NotificationRuleContext Empty = new NotificationRuleContext(
            0,
            0,
            0,
            0,
            0,
            WorryEmotionState.Unset);

        public NotificationRuleContext(
            int totalCardCount,
            int unreviewedCardCount,
            int taggedCardCount,
            int didNotHappenPercent,
            int todayWorryCount,
            WorryEmotionState latestEmotion)
        {
            TotalCardCount = totalCardCount;
            UnreviewedCardCount = unreviewedCardCount;
            TaggedCardCount = taggedCardCount;
            DidNotHappenPercent = didNotHappenPercent;
            TodayWorryCount = todayWorryCount;
            LatestEmotion = latestEmotion;
        }

        public int TotalCardCount { get; }
        public int UnreviewedCardCount { get; }
        public int TaggedCardCount { get; }
        public int DidNotHappenPercent { get; }
        public int TodayWorryCount { get; }
        public WorryEmotionState LatestEmotion { get; }

        public static NotificationRuleContext From(WorryRepository repository, WorryCard focusCard = null)
        {
            if (repository == null)
                return Empty;

            DateTime now = DateTime.Now;
            WorryOutcomeSummary summary = WorryOutcomeSummary.FromCards(repository.Cards);
            WorryCard latestCard = focusCard;
            int todayCount = 0;

            for (int i = 0; i < repository.Cards.Count; i++)
            {
                WorryCard card = repository.Cards[i];
                if (card == null)
                    continue;

                if (card.CreatedAt.Date == now.Date)
                    todayCount++;

                if (latestCard == null || card.CreatedAt > latestCard.CreatedAt)
                    latestCard = card;
            }

            return new NotificationRuleContext(
                summary.TotalCount,
                summary.UntaggedCount,
                summary.TaggedCount,
                summary.DidNotHappenPercent,
                todayCount,
                latestCard != null ? latestCard.EmotionState : WorryEmotionState.Unset);
        }
    }
}
