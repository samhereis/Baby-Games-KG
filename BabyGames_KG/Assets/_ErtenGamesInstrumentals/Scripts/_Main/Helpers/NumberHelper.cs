using UnityEngine;

namespace Helpers
{
    public static class NumberHelper
    {
        public static float GetPercentageOf1(float number, float maxNumber)
        {
            return (number / maxNumber);
        }

        public static float GetPercentageOf100(float number, float maxNumber)
        {
            return (number / maxNumber) * 100;
        }

        public static float GetNumberFromPercentage(float theNumber, float percentage)
        {
            return (percentage / 100) * theNumber;
        }

        public static bool IsMoreAbs(float one, float theOther)
        {
            return Mathf.Abs(one) > Mathf.Abs(theOther);
        }

        public static bool IsEqualsAbs(float one, float theOther)
        {
            return Mathf.Abs(one) == Mathf.Abs(theOther);
        }

        public static bool IsLessAbs(float one, float theOther)
        {
            return Mathf.Abs(one) < Mathf.Abs(theOther);
        }

        public static int GetRandom(int maxValue, int minValue = 0)
        {
            return Random.Range(minValue, maxValue);
        }

        public static float GetRandom(float maxValue, float minValue = 0)
        {
            return Random.Range(minValue, maxValue);
        }

        public static float GetRandom(this Vector2 value)
        {
            if (value.x <= value.y) { value.y += 0.1f; }
            return Random.Range(value.x, value.y);
        }

        public static int GetRandom(this Vector2Int value)
        {
            if (value.x <= value.y) { value.y += 1; }
            return Random.Range(value.x, value.y);
        }

        public static int ToInt(this bool value)
        {
            if (value)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}