using System.Collections.Generic;

namespace Rejuvena.Collate.Extensions
{
    public static class ListExtensions
    {
        public static List<T> With<T>(this List<T> list, T obj) {
            list.AddOnce(obj);
            return list;
        }

        public static void AddOnce<T>(this List<T> list, T obj) {
            if (!list.Contains(obj)) list.Add(obj);
        }
    }
}