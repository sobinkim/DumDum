using System;
using SB.App.Domain;

namespace SB.App.Application
{
    public readonly struct SupportInterventionContext
    {
        public SupportInterventionContext(
            int totalCards,
            int taggedCards,
            int untaggedCards,
            int didNotHappenPercent,
            int happenedPercent,
            int todayCards,
            int takeawayCount,
            int latestReviewDepthPercent,
            WorryEmotionState latestEmotion,
            string latestTakeaway,
            string latestCardId,
            DateTime now)
        {
            TotalCards = totalCards;
            TaggedCards = taggedCards;
            UntaggedCards = untaggedCards;
            DidNotHappenPercent = didNotHappenPercent;
            HappenedPercent = happenedPercent;
            TodayCards = todayCards;
            TakeawayCount = takeawayCount;
            LatestReviewDepthPercent = latestReviewDepthPercent;
            LatestEmotion = latestEmotion;
            LatestTakeaway = latestTakeaway ?? string.Empty;
            LatestCardId = latestCardId ?? string.Empty;
            Now = now;
        }

        public int TotalCards { get; }
        public int TaggedCards { get; }
        public int UntaggedCards { get; }
        public int DidNotHappenPercent { get; }
        public int HappenedPercent { get; }
        public int TodayCards { get; }
        public int TakeawayCount { get; }
        public int LatestReviewDepthPercent { get; }
        public WorryEmotionState LatestEmotion { get; }
        public string LatestTakeaway { get; }
        public string LatestCardId { get; }
        public DateTime Now { get; }

        public bool HasTaggedEvidence => TaggedCards > 0;
        public bool HasTakeaway => !string.IsNullOrWhiteSpace(LatestTakeaway);
        public bool LatestEmotionIsHeavy => LatestEmotion == WorryEmotionState.StillDistressed;
    }
}
