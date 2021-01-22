using System;
using System.Collections.Generic;

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
    }
}