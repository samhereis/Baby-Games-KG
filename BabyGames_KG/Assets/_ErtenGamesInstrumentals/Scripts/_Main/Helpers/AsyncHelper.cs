using Loggers;
using System;
using System.Threading;
using UnityEngine;

namespace Helpers
{
    public static class AsyncHelper
    {
        public static async Awaitable WaitWhile(Func<bool> condition, CancellationToken cancellationToken, float skipsTime = 0)
        {
            try
            {
                while (cancellationToken.IsCancellationRequested == false && condition.Invoke())
                {
                    if (skipsTime > 0) { await DelayFloat(skipsTime); } else { await NextFrame(); ; }

                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, "Error while wating for condition");
            }
        }

        public static async Awaitable WaitUntill(Func<bool> condition, CancellationToken cancellationToken, float skipsTime = 0)
        {
            try
            {
                while (cancellationToken.IsCancellationRequested == false && condition.Invoke() == false)
                {
                    if (skipsTime > 0) { await DelayFloat(skipsTime); } else { await NextFrame(); ; }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, "Error while wating for condition");
            }
        }

        public static async Awaitable Skip()
        {
            await Awaitable.WaitForSecondsAsync(0);
        }

        public static async Awaitable NextFrame()
        {
            await Awaitable.NextFrameAsync();
        }

        public static async Awaitable DelayFloat(float delay, CancellationToken cancellationToken = default)
        {
            await Awaitable.WaitForSecondsAsync(delay, cancellationToken);
        }

        public static async Awaitable DelayInt(int delay, CancellationToken cancellationToken = default)
        {
            int duration = (int)Mathf.Max(delay / 1000, 0);

            await Awaitable.WaitForSecondsAsync(duration, cancellationToken);
        }

        public static async Awaitable FromAsyncOperation(AsyncOperation asyncOperation)
        {
            await Awaitable.FromAsyncOperation(asyncOperation);
        }

        public static async void DoDelayed(Action action, float delay, CancellationToken cancellationToken = default)
        {
            await Awaitable.WaitForSecondsAsync(delay, cancellationToken);
            action?.Invoke();
        }
    }
}