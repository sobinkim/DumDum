using System.Collections.Generic;
using SB.App.Domain;
using UnityEngine;

namespace SB.App.Application
{
    [CreateAssetMenu(menuName = "DumDum/Notification Catalog", fileName = "NotificationCatalog")]
    public sealed class NotificationCatalog : ScriptableObject
    {
        [SerializeField] private NotificationRule[] rules;

        public IReadOnlyList<NotificationRule> Rules => rules ?? System.Array.Empty<NotificationRule>();

        public IEnumerable<NotificationRule> FindRules(NotificationTriggerType triggerType)
        {
            return FindRules(triggerType, NotificationRuleContext.Empty);
        }

        public IEnumerable<NotificationRule> FindRules(NotificationTriggerType triggerType, NotificationRuleContext context)
        {
            if (rules == null)
                yield break;

            for (int i = 0; i < rules.Length; i++)
            {
                NotificationRule rule = rules[i];
                if (rule != null && rule.Enabled && rule.TriggerType == triggerType && rule.AreConditionsMet(context))
                    yield return rule;
            }
        }

        public void SetRuntimeRules(NotificationRule[] notificationRules)
        {
            rules = notificationRules;
        }

        public static NotificationRule[] CreateDefaultRules()
        {
            return new[]
            {
                NotificationRule.Create(
                    "worry-review-3-days",
                    "고민 결과 확인",
                    NotificationTriggerType.WorryCardSaved,
                    NotificationRouteType.OutcomeReview,
                    "며칠 전 적은 걱정이 있어요",
                    "실제로 어떻게 되었는지 확인해볼까요?",
                    3,
                    0,
                    0,
                    true,
                    20,
                    0,
                    false,
                    1,
                    new[]
                    {
                        NotificationRuleCondition.Create(NotificationConditionType.UnreviewedCardCountAtLeast, 1)
                    }),
                NotificationRule.Create(
                    "daily-closure-next-noon",
                    "다음날 다시 사용 가능",
                    NotificationTriggerType.DailyClosureCompleted,
                    NotificationRouteType.Whiteboard,
                    "오늘 다시 정리할 수 있어요",
                    "필요하면 가볍게 마음을 정리해볼까요?",
                    1,
                    0,
                    0,
                    true,
                    12,
                    0),
                NotificationRule.Create(
                    "manual-test-one-minute",
                    "1분 뒤 테스트",
                    NotificationTriggerType.ManualTest,
                    NotificationRouteType.Whiteboard,
                    "알림 테스트",
                    "DumDum 알림이 정상적으로 예약됐어요.",
                    0,
                    0,
                    1,
                    false,
                    0,
                    0)
            };
        }
    }
}
