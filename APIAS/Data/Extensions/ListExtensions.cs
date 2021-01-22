using System;
using System.Collections.Generic;
using System.Linq;

namespace APIAS.Extensions
{
    public static class ListExtensions
    {
        public static bool AddUnique<T>(this IList<T> List, T newItem)
            where T : IEquatable<T>
        {
            if (List.Contains(newItem))
                return false;

            List.Add(newItem);
            return true;
        }

        public static void AddRangeUnique<T>(this List<T> List, IEnumerable<T> newList)
        {
            List.AddRange(newList);
            List.Distinct();
        }
    }
}