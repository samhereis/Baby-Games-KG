using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Helpers
{
    public static class CollectionsHelper
    {

        #region List

        public static void RemoveNulls<T>(this List<T> list)
        {
            list.RemoveAll(x => x == null);
        }

        public static void SafeAdd<T>(this List<T> list, T item)
        {
            if (list.Contains(item) == false) list.Add(item);
        }

        public static void SafeRemove<T>(this List<T> list, T item)
        {
            if (list.Count == 0) return;
            if (list.Contains(item) == true) list.Remove(item);
        }

        public static List<T> RemoveDuplicates<T>(this List<T> list)
        {
            var listCopy = new HashSet<T>();

            foreach (var item in list.ToHashSet())
            {
                listCopy.Add(item);
            }

            list = listCopy.ToList();
            return list;
        }

        public async static
#if UNITY_2023_2_OR_NEWER
            Awaitable
#else
            System.Threading.Tasks.Task
#endif
            RemoveDuplicatesAsync<T>(this List<T> list)
        {
            var listCopy = new List<T>();
            listCopy.AddRange(list);

            foreach (T itemToCheck in listCopy)
            {
                foreach (T itemToPotentiallyRemove in listCopy)
                {
                    bool isEqual = itemToPotentiallyRemove.Equals(itemToCheck);

                    if (isEqual) list.Remove(itemToPotentiallyRemove);

                    await AsyncHelper.Skip();
                }
            }
        }

        public static T GetRandom<T>(this IEnumerable<T> list)
        {
            if (list == null || list.Count() == 0) return default(T);

            return list.ElementAt(Random.Range(0, list.Count()));
        }

        public static int GetRandomIndex<T>(this IEnumerable<T> list)
        {
            if (list.Count() == 0) return 0;
            return Random.Range(0, list.Count());
        }

        public static bool HasEnoughElementsForIndex<T>(this IEnumerable<T> list, int index)
        {
            return list.Count() >= index + 1;
        }

        public static IList<T> Shuffle_Original<T>(this IList<T> list)
        {
            System.Random rng = new();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static IList<T> Shuffle_Copy<T>(this IList<T> list)
        {
            System.Random rng = new();
            IList<T> copy = new List<T>(list);

            int n = copy.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = copy[k];
                copy[k] = copy[n];
                copy[n] = value;
            }

            return copy;
        }

        #endregion

        #region Array

        public static T GetRandom<T>(this T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }

        #endregion
    }
}