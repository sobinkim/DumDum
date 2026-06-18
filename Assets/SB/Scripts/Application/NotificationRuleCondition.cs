using System;
using SB.App.Domain;
using UnityEngine;

namespace SB.App.Application
{
    [Serializable]
    public sealed class NotificationRuleCondition
    {
        [SerializeField] private bool enabled = true;
        [SerializeField] private NotificationConditionType conditionType;
        [Min(0)]
        [SerializeField] private int intValue = 1;
        [SerializeField] private WorryEmotionState emotionValue = WorryEmotionState.StillDistressed;

        public bool Enabled => enabled;
        public NotificationConditionType ConditionType => conditionType;
        public int IntValue => intValue;
        public WorryEmotionState EmotionValue => emotionValue;

        public bool IsMet(NotificationRuleContext context)
        {
            if (!enabled)
                return true;

            switch (conditionType)
            {
                case NotificationConditionType.UnreviewedCardCountAtLeast:
                    return context.UnreviewedCardCount >= intValue;
                case NotificationConditionType.TodayWorryCountAtLeast:
                    return context.TodayWorryCount >= intValue;
                case NotificationConditionType.LatestEmotionIs:
                    return context.LatestEmotion == emotionValue;
                case NotificationConditionType.TaggedCardCountAtLeast:
                    return context.TaggedCardCount >= intValue;
                case NotificationConditionType.DidNotHappenPercentAtLeast:
                    return context.DidNotHappenPercent >= intValue;
                default:
                    return true;
            }
        }

        public static NotificationRuleCondition Create(NotificationConditionType type, int value = 1)
        {
            return new NotificationRuleCondition
            {
                conditionType = type,
                intValue = Mathf.Max(0, value)
            };
        }
    }
}
