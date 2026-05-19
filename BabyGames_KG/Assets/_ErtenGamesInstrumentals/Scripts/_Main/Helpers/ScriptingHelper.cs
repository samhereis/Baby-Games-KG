#if UNITY_EDITOR
#endif
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Helpers
{
    public static class ScriptingHelper
    {
        public static void DoTimes(this object monoBehaviour, Action action, int times)
        {
            for (int i = 0; i < times; i++)
            {
                action?.Invoke();
            }
        }

        public static async Task DoTimesAsync(this object monoBehaviour, Action action, int times, float delay)
        {
            for (int i = 0; i < times; i++)
            {
                action?.Invoke();
                await AsyncHelper.DelayFloat(delay);
            }
        }

        public static async Task DoTimesAsync(this object monoBehaviour, Task action, int times)
        {
            for (int i = 0; i < times; i++)
            {
                await action;
            }
        }

        public static async Awaitable DoTimesAsync(this object monoBehaviour, Awaitable action, int times)
        {
            for (int i = 0; i < times; i++)
            {
                await action;
            }
        }
    }
}