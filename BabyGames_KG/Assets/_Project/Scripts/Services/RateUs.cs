using System;
using Loggers;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

#if UNITY_ANDROID
using Google.Play.Review;
#endif

namespace _Project.Scripts.Services
{
    public class RateUs : MonoBehaviour
    {
        [Header("Auto prompt")]
        [SerializeField] private int _mainMenuOpensBeforePrompt = 3;
        [SerializeField] private int _minDaysBetweenAutoPrompts = 7;

        [Header("Store IDs")]
        [SerializeField] private string _iosAppId = "";

        private const string _lastShownKey = "RateUs_LastTimeShown";

        [Button]
        public void TryAutoReview()
        {
            _mainMenuOpensBeforePrompt--;
            if (_mainMenuOpensBeforePrompt != 0) { return; }

            if (IsThrottled()) { return; }

            PlayerPrefs.SetString(_lastShownKey, DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
            PlayerPrefs.Save();

#if UNITY_EDITOR
            OpenStorePage();
#endif

#if UNITY_IOS
            Device.RequestStoreReview();
#endif

#if UNITY_ANDROID
            RequestReviewAndroid();
#endif
        }

        [Button]
        public void OpenStorePage()
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(_iosAppId))
            {
                Application.OpenURL($"https://apps.apple.com/app/id{_iosAppId}?action=write-review");
            }
            else
            {
                Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
            }
            return;
#endif

#if UNITY_ANDROID
            var id = Application.identifier;
            try
            {
                Application.OpenURL("market://details?id=" + id);
            } catch (Exception e)
            {
                CustomLogger.instance?.LogException(e);
                Application.OpenURL("https://play.google.com/store/apps/details?id=" + id);
            }
#endif

#if UNITY_IOS
            try
            {
                if (string.IsNullOrEmpty(_iosAppId)) { return; }
                Application.OpenURL($"itms-apps://apps.apple.com/app/id{_iosAppId}?action=write-review");
            }
            catch (Exception e) { CustomLogger.instance?.LogException(e); }
#endif
        }

        [Button]
        private void RequestReviewAndroid()
        {
#if UNITY_ANDROID
            try
            {
                var reviewManager = new ReviewManager();
                var requestOp = reviewManager.RequestReviewFlow();
                requestOp.Completed += request =>
                {
                    if (request.Error != ReviewErrorCode.NoError) { return; }

                    reviewManager.LaunchReviewFlow(request.GetResult());
                };
            } catch (Exception e) { CustomLogger.instance?.LogException(e); }
#endif
        }

        private bool IsThrottled()
        {
            try
            {
                if (!PlayerPrefs.HasKey(_lastShownKey)) { return false; }
                if (!long.TryParse(PlayerPrefs.GetString(_lastShownKey), out long lastShown)) { return false; }

                long now = DateTimeOffset.Now.ToUnixTimeSeconds();
                long minGap = (long)_minDaysBetweenAutoPrompts * 24 * 60 * 60;
                bool throttled = Math.Abs(now - lastShown) < minGap;

                CustomLogger.instance?.Log("RateUs", $"IsThrottled: now={now}, lastShown={lastShown}, minGap={minGap}, throttled={throttled}", this);
                return throttled;
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e);
                return true;
            }
        }
    }
}
