using System;
using System.Collections.Generic;

namespace Wtf
{
    public static class IListExtensions
    {
        // Enqueue
        public static void Enqueue<T>(this IList<T> list, T item) where T : class
        {
            list.Add(item);
        }

        // Dequeue
        public static T Dequeue<T>(this IList<T> list) where T : class
        {
            var item = list[0];
            list.RemoveAt(0);
            return item;
        }

        public static void AddSorted<T>(this List<T> @this, T item) where T : IComparable<T>
        {
            if (@this.Count == 0)
            {
                @this.Add(item);
                return;
            }
            if (@this[@this.Count - 1].CompareTo(item) <= 0)
            {
                @this.Add(item);
                return;
            }
            if (@this[0].CompareTo(item) >= 0)
            {
                @this.Insert(0, item);
                return;
            }
            int index = @this.BinarySearch(item);
            if (index < 0)
                index = ~index;
            @this.Insert(index, item);
        }

        public static T RandomChoose<T>(this IList<T> list) where T : class
        {
            if (list.Count == 0)
            {
                return null;
            }

            var index = UnityEngine.Random.Range(0, list.Count);
            return list[index];
        }
    }
}