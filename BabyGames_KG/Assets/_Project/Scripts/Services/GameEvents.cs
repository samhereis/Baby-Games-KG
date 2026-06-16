using System;
using System.Collections.Generic;
using Data;
using DataClasses;
using Loggers;
using UnityEngine;
using _Project.Scripts.Services.Purchase;

namespace _Project.Scripts.Services
{
    public static class GameEvents
    {
        private static int gamesOpenedSession;
        private static DateTime _lastGameOpenTime;

        public static void GameOpened(Activity activity)
        {
            try
            {
                GameEventsService.Instance?.SendCustomEvent(Constants_Events.openGame, new Dictionary<string, object>
                {
                    { "gameName", activity.activityName },
                    { "gameCategory", activity.acitvityCategory },
                    { "gamesPlayedInSession", gamesOpenedSession }
                });
                gamesOpenedSession++;
                _lastGameOpenTime = DateTime.Now;
            } catch (Exception e) { CustomLogger.instance?.LogException(e); }
        }

        public static void GameClosed(Activity activity)
        {
            try
            {
                if (gamesOpenedSession > 0 && activity != null)
                {
                    GameEventsService.Instance?.SendCustomEvent(Constants_Events.closeGame, new Dictionary<string, object>
                    {
                        { "gameName", activity.activityName },
                        { "gameCategory", activity.acitvityCategory },
                        { "duration", (int)(DateTime.Now - _lastGameOpenTime).TotalSeconds }
                    });
                }
            } catch (Exception e) { CustomLogger.instance?.LogException(e); }
        }

        public static void AppOpened()
        {
            try
            {
                int openCount = PlayerPrefs.GetInt("app_open_count", 0) + 1;
                PlayerPrefs.SetInt("app_open_count", openCount);
                PlayerPrefs.Save();

                GameEventsService.Instance?.SendCustomEvent(Constants_Events.appOpen, new Dictionary<string, object>
                {
                    { "systemLanguage", Application.systemLanguage.ToString() },
                    { "gameLanguage", LocalizationHelper.currentLanguage },
                    { "openCount", openCount }
                });
            } catch (Exception e) { CustomLogger.instance?.LogException(e); }
        }

        public static void AppClosed()
        {
            try
            {
                int totalSeconds = (int)Time.realtimeSinceStartup;
                if (totalSeconds == 0) { return; }

                GameEventsService.Instance?.SendCustomEvent(Constants_Events.appClose, new Dictionary<string, string>
                {
                    { "durationSeconds", totalSeconds.ToString() }
                });
            } catch (Exception e) { CustomLogger.instance?.LogException(e); }
        }

        public static void PurchaseScreenClose(ISubscriptionChecker subscriptionChecker)
        {
            try
            {
                var isPurchased = subscriptionChecker.IsSubscribed();
                GameEventsService.Instance?.SendCustomEvent(Constants_Events.purchaseScreenClose, new Dictionary<string, object>
                {
                    { "isPurchased", isPurchased },
                    { "systemLanguage", Application.systemLanguage.ToString() },
                });
            } catch (Exception e) { CustomLogger.instance?.LogException(e); }
        }
    }
}
