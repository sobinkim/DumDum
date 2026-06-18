using System;
using SB.App.Domain;
using UnityEngine;

namespace SB.App.Application
{
    public sealed class MobileNotificationService
    {
        private readonly NotificationCatalog _catalog;
        private readonly MobileNotificationGateway _gateway;

        public MobileNotificationService(NotificationCatalog catalog)
        {
            _catalog = catalog;
            _gateway = new MobileNotificationGateway();
        }

        public void ScheduleWorryReview(WorryCard card, WorryRepository repository)
        {
            if (card == null)
                return;

            ScheduleRules(NotificationTriggerType.WorryCardSaved, card.CreatedAt, card.Id, NotificationRuleContext.From(repository, card));
        }

        public void ScheduleDailyClosure(DailyClosureState closureState, WorryRepository repository = null)
        {
            ScheduleRules(NotificationTriggerType.DailyClosureCompleted, closureState.ClosedAt, null, NotificationRuleContext.From(repository));
        }

        public void ScheduleManualTest()
        {
            ScheduleRules(NotificationTriggerType.ManualTest, DateTime.Now, null, NotificationRuleContext.Empty);
        }

        private void ScheduleRules(NotificationTriggerType triggerType, DateTime sourceTime, string cardId, NotificationRuleContext context)
        {
            if (_catalog == null)
            {
                Debug.LogWarning("[DumDum] NotificationCatalog가 연결되지 않았어요.");
                return;
            }

            foreach (NotificationRule rule in _catalog.FindRules(triggerType, context))
            {
                DateTime fireTime = rule.GetFireTime(sourceTime);
                string payload = NotificationPayload.Create(rule.Id, rule.RouteType, cardId);
                string scheduleId = string.IsNullOrWhiteSpace(cardId)
                    ? rule.Id
                    : $"{rule.Id}-{cardId}";

                _gateway.Schedule(new NotificationScheduleRequest(
                    scheduleId,
                    rule.Title,
                    rule.Body,
                    fireTime,
                    rule.Repeat,
                    rule.RepeatInterval,
                    payload));
            }
        }
    }
}
