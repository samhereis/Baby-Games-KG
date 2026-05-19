using System;
using System.Collections;
using Loggers;
using UnityEngine;
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace Coloring.Notification
{
    public class iOSNotificationController : NotificationBase
    {
#if UNITY_IOS
        protected override void RegisterForNotifications()
        {
            StartCoroutine(RequestNotificationPermissionIOS());
        }

        private IEnumerator RequestNotificationPermissionIOS()
        {
            var options = AuthorizationOption.Alert | AuthorizationOption.Badge;
            using (var request = new AuthorizationRequest(options, false))
            {
                while (!request.IsFinished)
                {
                    yield return null;
                }

                Debug.Log($"RequestAuthorization finished: {request.IsFinished}, granted: {request.Granted}, error: {request.Error}, deviceToken: {request.DeviceToken}");
            }
        }

        protected override void ScheduleNotification(Timer timer, string title, string message, bool repeat)
        {
            var timeTrigger = new iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = new TimeSpan(timer.days, timer.hours, timer.minutes, timer.seconds),
                Repeats = repeat
            };

            var notification = new iOSNotification
            {
                Body = message,
                ShowInForeground = true,
                ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
                Trigger = timeTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(notification);
            CustomLogger.instance?.Log($"IOS Notifications: {title} {message}", $"{timer.days}, {timer.hours}, repeat: {repeat}", this, LogTypes.General);
        }

        protected override void RemoveAllNotifications()
        {
            iOSNotificationCenter.RemoveAllScheduledNotifications();
            iOSNotificationCenter.RemoveAllDeliveredNotifications();
        }
#else

        protected override void RegisterForNotifications()
        {

        }

        protected override void RemoveAllNotifications()
        {

        }

        protected override void ScheduleNotification(Timer timer, string title, string message, bool repeat)
        {

        }
#endif
    }
}