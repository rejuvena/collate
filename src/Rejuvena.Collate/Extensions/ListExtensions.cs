using System.Collections.Generic;

namespace Rejuvena.Collate.Extensions
{
    public static class ListExtensions
    {
        public static List<T> With<T>(this List<T> list, T obj) {
            list.Add(obj);
            return list;
        }
    }
}