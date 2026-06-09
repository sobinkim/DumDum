using System;
using System.Reflection;
using UnityEngine;

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
            const string postNotifications = "android.permission.POST_NOTIFICATIONS";
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(postNotifications))
                UnityEngine.Android.Permission.RequestUserPermission(postNotifications);
#elif UNITY_IOS
            RequestIosAuthorization();
#endif
        }

#if UNITY_ANDROID
        private bool ScheduleAndroid(NotificationScheduleRequest request)
        {
            Type centerType = FindType("Unity.Notifications.Android.AndroidNotificationCenter, Unity.Notifications.Android");
            Type notificationType = FindType("Unity.Notifications.Android.AndroidNotification, Unity.Notifications.Android");
            if (centerType == null || notificationType == null)
                return LogMissingPackage();

            RegisterAndroidChannel(centerType);

            object notification = Activator.CreateInstance(notificationType);
            SetValue(notification, "Title", request.Title);
            SetValue(notification, "Text", request.Body);
            SetValue(notification, "FireTime", request.FireTime);
            SetValue(notification, "IntentData", request.Payload);

            if (request.Repeat)
                SetValue(notification, "RepeatInterval", request.RepeatInterval);

            int explicitId = GetStableId(request.Id);
            MethodInfo explicitMethod = centerType.GetMethod("SendNotificationWithExplicitID", BindingFlags.Public | BindingFlags.Static);
            if (explicitMethod != null)
            {
                explicitMethod.Invoke(null, new[] { notification, AndroidChannelId, explicitId });
                return true;
            }

            MethodInfo sendMethod = centerType.GetMethod("SendNotification", BindingFlags.Public | BindingFlags.Static);
            if (sendMethod == null)
                return false;

            sendMethod.Invoke(null, new[] { notification, AndroidChannelId });
            return true;
        }

        private void RegisterAndroidChannel(Type centerType)
        {
            if (_androidChannelRegistered)
                return;

            Type channelType = FindType("Unity.Notifications.Android.AndroidNotificationChannel, Unity.Notifications.Android");
            Type importanceType = FindType("Unity.Notifications.Android.Importance, Unity.Notifications.Android");
            if (channelType == null || importanceType == null)
                return;

            object channel = Activator.CreateInstance(channelType);
            SetValue(channel, "Id", AndroidChannelId);
            SetValue(channel, "Name", AndroidChannelName);
            SetValue(channel, "Description", "고민 정리와 하루 마무리 리마인더");
            SetValue(channel, "Importance", Enum.Parse(importanceType, "Default"));

            MethodInfo registerMethod = centerType.GetMethod("RegisterNotificationChannel", BindingFlags.Public | BindingFlags.Static);
            registerMethod?.Invoke(null, new[] { channel });
            _androidChannelRegistered = true;
        }
#endif

#if UNITY_IOS
        private bool ScheduleIos(NotificationScheduleRequest request)
        {
            Type centerType = FindType("Unity.Notifications.iOS.iOSNotificationCenter, Unity.Notifications.iOS");
            Type notificationType = FindType("Unity.Notifications.iOS.iOSNotification, Unity.Notifications.iOS");
            Type triggerType = FindType("Unity.Notifications.iOS.iOSNotificationTimeIntervalTrigger, Unity.Notifications.iOS");
            if (centerType == null || notificationType == null || triggerType == null)
                return LogMissingPackage();

            object trigger = Activator.CreateInstance(triggerType);
            TimeSpan interval = request.FireTime - DateTime.Now;
            if (interval.TotalSeconds < 1)
                interval = TimeSpan.FromSeconds(1);

            SetValue(trigger, "TimeInterval", interval);
            SetValue(trigger, "Repeats", request.Repeat);

            object notification = Activator.CreateInstance(notificationType);
            SetValue(notification, "Identifier", request.Id);
            SetValue(notification, "Title", request.Title);
            SetValue(notification, "Body", request.Body);
            SetValue(notification, "Data", request.Payload);
            SetValue(notification, "ShowInForeground", true);
            SetValue(notification, "Trigger", trigger);

            MethodInfo scheduleMethod = centerType.GetMethod("ScheduleNotification", BindingFlags.Public | BindingFlags.Static);
            if (scheduleMethod == null)
                return false;

            scheduleMethod.Invoke(null, new[] { notification });
            return true;
        }

        private void RequestIosAuthorization()
        {
            Type centerType = FindType("Unity.Notifications.iOS.iOSNotificationCenter, Unity.Notifications.iOS");
            Type authorizationType = FindType("Unity.Notifications.iOS.AuthorizationOption, Unity.Notifications.iOS");
            if (centerType == null || authorizationType == null)
                return;

            object alert = Enum.Parse(authorizationType, "Alert");
            object badge = Enum.Parse(authorizationType, "Badge");
            object sound = Enum.Parse(authorizationType, "Sound");
            int optionsValue = Convert.ToInt32(alert) | Convert.ToInt32(badge) | Convert.ToInt32(sound);
            object options = Enum.ToObject(authorizationType, optionsValue);

            MethodInfo requestMethod = centerType.GetMethod("RequestAuthorization", BindingFlags.Public | BindingFlags.Static);
            requestMethod?.Invoke(null, new[] { options, true });
        }
#endif

        private static Type FindType(string assemblyQualifiedName)
        {
            return Type.GetType(assemblyQualifiedName, false);
        }

        private static void SetValue(object target, string name, object value)
        {
            if (target == null)
                return;

            Type type = target.GetType();
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanWrite)
            {
                property.SetValue(target, value);
                return;
            }

            FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            field?.SetValue(target, value);
        }

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

        private static bool LogMissingPackage()
        {
            Debug.LogWarning("[DumDum] Mobile Notifications 패키지가 없어서 실제 알림을 예약하지 못했어요. Package Manager에서 com.unity.mobile.notifications를 설치해주세요.");
            return false;
        }
    }
}
