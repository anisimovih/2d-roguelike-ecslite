using System.Collections.Generic;

namespace Roguelike.Extensions
{
    public static class ListExtensions
    {
        public static T Random<T>(this IList<T> list, int start=0)
        {
            return list[list.RandomIndex(start)];
        }

        public static int RandomIndex<T>(this IList<T> list, int start=0)
        {
            return UnityEngine.Random.Range(start, list.Count);
        }
    }
}
