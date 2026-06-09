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

        public void ScheduleWorryReview(WorryCard card)
        {
            if (card == null)
                return;

            ScheduleRules(NotificationTriggerType.WorryCardSaved, card.CreatedAt, card.Id);
        }

        public void ScheduleDailyClosure(DailyClosureState closureState)
        {
            ScheduleRules(NotificationTriggerType.DailyClosureCompleted, closureState.ClosedAt, null);
        }

        public void ScheduleManualTest()
        {
            ScheduleRules(NotificationTriggerType.ManualTest, DateTime.Now, null);
        }

        private void ScheduleRules(NotificationTriggerType triggerType, DateTime sourceTime, string cardId)
        {
            if (_catalog == null)
            {
                Debug.LogWarning("[DumDum] NotificationCatalog가 연결되지 않았어요.");
                return;
            }

            foreach (NotificationRule rule in _catalog.FindRules(triggerType))
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
