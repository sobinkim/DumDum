using System;
using SB.App.Domain;
using UnityEngine;

namespace SB.App.Application
{
    [Serializable]
    public sealed class NotificationRule
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private bool enabled = true;
        [SerializeField] private NotificationTriggerType triggerType;
        [SerializeField] private NotificationRouteType routeType;
        [SerializeField] private string title;
        [TextArea(2, 4)]
        [SerializeField] private string body;
        [Min(0)]
        [SerializeField] private int delayDays;
        [Min(0)]
        [SerializeField] private int delayHours;
        [Min(0)]
        [SerializeField] private int delayMinutes;
        [SerializeField] private bool useFixedClockTime;
        [Range(0, 23)]
        [SerializeField] private int fixedHour = 20;
        [Range(0, 59)]
        [SerializeField] private int fixedMinute;
        [SerializeField] private bool repeat;
        [Min(1)]
        [SerializeField] private int repeatIntervalDays = 1;
        [SerializeField] private NotificationRuleCondition[] conditions;

        public string Id => string.IsNullOrWhiteSpace(id) ? displayName : id;
        public string DisplayName => displayName;
        public bool Enabled => enabled;
        public NotificationTriggerType TriggerType => triggerType;
        public NotificationRouteType RouteType => routeType;
        public string Title => title;
        public string Body => body;
        public int DelayDays => delayDays;
        public int DelayHours => delayHours;
        public int DelayMinutes => delayMinutes;
        public bool UseFixedClockTime => useFixedClockTime;
        public int FixedHour => fixedHour;
        public int FixedMinute => fixedMinute;
        public bool Repeat => repeat;
        public int RepeatIntervalDays => repeatIntervalDays;
        public TimeSpan RepeatInterval => TimeSpan.FromDays(Mathf.Max(1, repeatIntervalDays));
        public NotificationRuleCondition[] Conditions => conditions ?? Array.Empty<NotificationRuleCondition>();

        public bool AreConditionsMet(NotificationRuleContext context)
        {
            NotificationRuleCondition[] ruleConditions = Conditions;
            for (int i = 0; i < ruleConditions.Length; i++)
            {
                NotificationRuleCondition condition = ruleConditions[i];
                if (condition != null && !condition.IsMet(context))
                    return false;
            }

            return true;
        }

        public DateTime GetFireTime(DateTime sourceTime)
        {
            DateTime fireTime = sourceTime
                .AddDays(Mathf.Max(0, delayDays))
                .AddHours(Mathf.Max(0, delayHours))
                .AddMinutes(Mathf.Max(0, delayMinutes));

            if (useFixedClockTime)
            {
                fireTime = fireTime.Date
                    .AddHours(Mathf.Clamp(fixedHour, 0, 23))
                    .AddMinutes(Mathf.Clamp(fixedMinute, 0, 59));
            }

            if (fireTime <= DateTime.Now)
                fireTime = DateTime.Now.AddMinutes(1);

            return fireTime;
        }

        public static NotificationRule Create(
            string ruleId,
            string name,
            NotificationTriggerType trigger,
            NotificationRouteType route,
            string notificationTitle,
            string notificationBody,
            int days,
            int hours,
            int minutes,
            bool fixedClock,
            int hour,
            int minute,
            bool shouldRepeat = false,
            int repeatDays = 1,
            NotificationRuleCondition[] ruleConditions = null)
        {
            return new NotificationRule
            {
                id = ruleId,
                displayName = name,
                enabled = true,
                triggerType = trigger,
                routeType = route,
                title = notificationTitle,
                body = notificationBody,
                delayDays = days,
                delayHours = hours,
                delayMinutes = minutes,
                useFixedClockTime = fixedClock,
                fixedHour = hour,
                fixedMinute = minute,
                repeat = shouldRepeat,
                repeatIntervalDays = Mathf.Max(1, repeatDays),
                conditions = ruleConditions ?? Array.Empty<NotificationRuleCondition>()
            };
        }
    }
}
