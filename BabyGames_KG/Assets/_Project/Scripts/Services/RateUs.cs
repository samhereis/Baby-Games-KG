using System;
#if UNITY_ANDROID
//using Google.Play.Review;
#endif
using UnityEngine;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Services
{
    public class RateUs : MonoBehaviour
    {
        [SerializeField] private bool _autoShow = false;

        private const string _lastShownKey = "RateUs_LastTimeShown";
        private float _startTime;

        private void Start()
        {
            _startTime = Time.realtimeSinceStartup;

            if (_autoShow)
            {
                ShowRateUsPopup(true);
            }
        }

        public void ShowRateUsPopup(bool waitAfterGameStartTime = false)
        {
            if (waitAfterGameStartTime)
            {
                if (Time.realtimeSinceStartup - _startTime < 600f)
                {
                    Debug.Log("Popup not shown. App not running for 10 minutes yet.");
                    return;
                }
            }

            long lastTimeShown = 0;
            if (PlayerPrefs.HasKey(_lastShownKey))
            {
                string lastTimeShownStr = PlayerPrefs.GetString(_lastShownKey);
                lastTimeShown = long.Parse(lastTimeShownStr);
            }

            long currentTimeInSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();
            float dif = Mathf.Abs(currentTimeInSeconds - lastTimeShown);
            if (dif < 3 * 24 * 60 * 60)
            {
                Debug.Log("Short time Show Popup not shown");
                return;
            }

            PlayerPrefs.SetString(_lastShownKey, currentTimeInSeconds.ToString());
            PlayerPrefs.Save();

#if UNITY_IOS
        Device.RequestStoreReview();
#elif UNITY_ANDROID
            RequestReviewAndroid();
#endif
        }

#if UNITY_ANDROID
        public void RequestReviewAndroid()
        {
            //var reviewManager = new ReviewManager();
            //
            //var playReviewInfoAsyncOperation = reviewManager.RequestReviewFlow();
            //
            //playReviewInfoAsyncOperation.Completed += playReviewInfoAsync =>
            //{
            //    if (playReviewInfoAsync.Error == ReviewErrorCode.NoError)
            //    {
            //        // display the review prompt
            //        var playReviewInfo = playReviewInfoAsync.GetResult();
            //        reviewManager.LaunchReviewFlow(playReviewInfo);
            //    }
            //    else
            //    {
            //
            //    }
            //};
        }
#endif
    }
}