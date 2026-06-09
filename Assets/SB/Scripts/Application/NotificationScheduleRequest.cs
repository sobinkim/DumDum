using System;

namespace SB.App.Application
{
    public readonly struct NotificationScheduleRequest
    {
        public NotificationScheduleRequest(
            string id,
            string title,
            string body,
            DateTime fireTime,
            bool repeat,
            TimeSpan repeatInterval,
            string payload)
        {
            Id = id;
            Title = title;
            Body = body;
            FireTime = fireTime;
            Repeat = repeat;
            RepeatInterval = repeatInterval;
            Payload = payload;
        }

        public string Id { get; }
        public string Title { get; }
        public string Body { get; }
        public DateTime FireTime { get; }
        public bool Repeat { get; }
        public TimeSpan RepeatInterval { get; }
        public string Payload { get; }
    }
}
