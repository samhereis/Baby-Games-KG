
using Loggers;
using System;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

namespace Coloring.Notification
{
    public class AndroidNotificationController : NotificationBase
    {
#if UNITY_ANDROID
        protected override void RegisterForNotifications()
        {
            RequestNotificationPermissionAndroid();
        }

        private void RequestNotificationPermissionAndroid()
        {
            var channel = new AndroidNotificationChannel
            {
                Id = "default_channel",
                Name = "Default Channel",
                Importance = Importance.Default,
                Description = "Generic notifications",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }

        protected override void ScheduleNotification(Timer timer, string title, string message, bool repeat)
        {
            var notification = new AndroidNotification
            {
                Title = title,
                Text = message,
                SmallIcon = "icon_small",
                FireTime = DateTime.Now.Add(new TimeSpan(timer.days, timer.hours, timer.minutes, timer.seconds)),
            };

            if (repeat)
            {
                notification.RepeatInterval = new TimeSpan(timer.days, timer.hours, timer.minutes, timer.seconds);
            }

            AndroidNotificationCenter.SendNotification(notification, "default_channel");
            CustomLogger.instance?.Log($"Android Notifications: {title} {message}", $"{timer.days}, {timer.hours}, repeat: {repeat}", this, LogTypes.General);
        }

        protected override void RemoveAllNotifications()
        {
            AndroidNotificationCenter.CancelAllScheduledNotifications();
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