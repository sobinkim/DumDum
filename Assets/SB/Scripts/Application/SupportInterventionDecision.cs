using System;
using SB.App.Domain;

namespace SB.App.Application
{
    public readonly struct SupportInterventionDecision
    {
        public static readonly SupportInterventionDecision None = new SupportInterventionDecision(
            SupportInterventionType.None,
            string.Empty,
            string.Empty,
            string.Empty,
            NotificationRouteType.Whiteboard,
            string.Empty,
            TimeSpan.Zero);

        public SupportInterventionDecision(
            SupportInterventionType type,
            string id,
            string title,
            string body,
            NotificationRouteType routeType,
            string cardId,
            TimeSpan delay)
        {
            Type = type;
            Id = id ?? string.Empty;
            Title = title ?? string.Empty;
            Body = body ?? string.Empty;
            RouteType = routeType;
            CardId = cardId ?? string.Empty;
            Delay = delay;
        }

        public SupportInterventionType Type { get; }
        public string Id { get; }
        public string Title { get; }
        public string Body { get; }
        public NotificationRouteType RouteType { get; }
        public string CardId { get; }
        public TimeSpan Delay { get; }

        public bool HasNotification => Type != SupportInterventionType.None
            && !string.IsNullOrWhiteSpace(Id)
            && !string.IsNullOrWhiteSpace(Title)
            && !string.IsNullOrWhiteSpace(Body);
    }
}
