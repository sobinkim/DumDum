using System;
using SB.App.Domain;
using UnityEngine;

namespace SB.App.Application
{
    [Serializable]
    public sealed class NotificationPayload
    {
        public string ruleId;
        public string cardId;
        public NotificationRouteType routeType;

        public static string Create(string ruleId, NotificationRouteType route, string cardId = null)
        {
            return JsonUtility.ToJson(new NotificationPayload
            {
                ruleId = ruleId,
                routeType = route,
                cardId = cardId ?? string.Empty
            });
        }
    }
}
