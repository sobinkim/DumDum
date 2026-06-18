namespace SB.App.Domain
{
    public enum NotificationConditionType
    {
        Always,
        UnreviewedCardCountAtLeast,
        TodayWorryCountAtLeast,
        LatestEmotionIs,
        TaggedCardCountAtLeast,
        DidNotHappenPercentAtLeast
    }
}
