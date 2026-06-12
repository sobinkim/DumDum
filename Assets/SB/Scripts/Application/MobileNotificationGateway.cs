using System;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace SB.App.Application
{
    public sealed class MobileNotificationGateway
    {
        private const string AndroidChannelId = "dumdum_reminders";
        private const string AndroidChannelName = "DumDum 리마인더";
        private bool _androidChannelRegistered;
        private bool _permissionRequested;

        public bool Schedule(NotificationScheduleRequest request)
        {
            RequestPermissionIfNeeded();

#if UNITY_ANDROID
            return ScheduleAndroid(request);
#elif UNITY_IOS
            return ScheduleIos(request);
#else
            Debug.Log($"[DumDum] 알림 예약 미리보기: {request.Title} / {request.FireTime:yyyy-MM-dd HH:mm}");
            return true;
#endif
        }

        private void RequestPermissionIfNeeded()
        {
            if (_permissionRequested)
                return;

            _permissionRequested = true;

#if UNITY_ANDROID
            if (AndroidNotificationCenter.UserPermissionToPost != PermissionStatus.Allowed)
                _ = new PermissionRequest();
#elif UNITY_IOS
            iOSNotificationCenter.RequestAuthorization(
                AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound,
                true);
#endif
        }

#if UNITY_ANDROID
        private bool ScheduleAndroid(NotificationScheduleRequest request)
        {
            RegisterAndroidChannel();

            DateTime fireTime = request.FireTime;
            if (fireTime <= DateTime.Now)
                fireTime = DateTime.Now.AddMinutes(1);

            AndroidNotification notification = new AndroidNotification
            {
                Title = request.Title,
                Text = request.Body,
                FireTime = fireTime,
                IntentData = request.Payload
            };

            if (request.Repeat)
                notification.RepeatInterval = request.RepeatInterval;

            int id = GetStableId(request.Id);
            AndroidNotificationCenter.SendNotificationWithExplicitID(notification, AndroidChannelId, id);
            Debug.Log($"[DumDum] Android 알림 예약: {request.Title} / {fireTime:yyyy-MM-dd HH:mm:ss} / id={id}");
            return true;
        }

        private void RegisterAndroidChannel()
        {
            if (_androidChannelRegistered)
                return;

            AndroidNotificationChannel channel = new AndroidNotificationChannel
            {
                Id = AndroidChannelId,
                Name = AndroidChannelName,
                Importance = Importance.Default,
                Description = "고민 정리와 하루 마무리 리마인더"
            };

            AndroidNotificationCenter.RegisterNotificationChannel(channel);
            _androidChannelRegistered = true;
        }
#endif

#if UNITY_IOS
        private static bool ScheduleIos(NotificationScheduleRequest request)
        {
            TimeSpan interval = request.FireTime - DateTime.Now;
            if (interval.TotalSeconds < 1)
                interval = TimeSpan.FromSeconds(1);

            iOSNotificationTimeIntervalTrigger trigger = new iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = interval,
                Repeats = request.Repeat
            };

            iOSNotification notification = new iOSNotification
            {
                Identifier = request.Id,
                Title = request.Title,
                Body = request.Body,
                Data = request.Payload,
                ShowInForeground = true,
                Trigger = trigger
            };

            iOSNotificationCenter.ScheduleNotification(notification);
            return true;
        }
#endif

        private static int GetStableId(string value)
        {
            unchecked
            {
                int hash = 23;
                for (int i = 0; i < value.Length; i++)
                    hash = hash * 31 + value[i];

                return Mathf.Abs(hash);
            }
        }
    }
}
