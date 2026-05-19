using Helpers;
using Loggers;
using ScriptableObjects;
using System;
using _Project.Scripts.Services;
using UnityEngine;

namespace Coloring.Notification
{
    public abstract class NotificationBase : MonoBehaviour
    {
        [Serializable]
        public struct Timer
        {
            public int days, hours, minutes, seconds;
            public Timer(int d, int h, int m, int s) { days = d; hours = h; minutes = m; seconds = s; }
        }

        [Header("Notification Timers")]
        public Timer firstNotificationTimer = new Timer(1, 0, 0, 0);
        public Timer repeatNotificationTimer = new Timer(1, 0, 0, 0);

        protected GameSavableSettings _gameSavableSettings;

        public void Initialize(GameSavableSettings gameSettings)
        {
            _gameSavableSettings = gameSettings;

            RegisterForNotifications();
            UpdateNotifications();
        }

        protected abstract void RegisterForNotifications();

        protected abstract void ScheduleNotification(Timer timer, string title, string message, bool repeat);

        public async void UpdateNotifications()
        {
            try
            {
                RemoveAllNotifications();

                if (_gameSavableSettings == null || !_gameSavableSettings.notifications.currentValue) { return; }

                string title = await LocalizationHelper.GetTranslationAsync("Notifications", $"title{new Vector2Int(1, 3).GetRandom()}");
                string message = await LocalizationHelper.GetTranslationAsync("Notifications", $"text{new Vector2Int(1, 3).GetRandom()}");

                if (title == string.Empty) { title = "Come back!"; }
                if (title == string.Empty) { title = "Don't forget to play some fun games!"; }

                ScheduleNotification(repeatNotificationTimer, title, message, true);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, "Notifications");
            }
        }

        protected abstract void RemoveAllNotifications();
    }
}