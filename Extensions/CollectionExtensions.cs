using System;
using System.Collections.Generic;

namespace RemoteController.Extensions
{
    public static class CollectionExtensions
    {
        public static T[] Map<TSource, T>(this ICollection<TSource> self, Func<TSource, T> selector)
        {
            var res = new T[self.Count];
            int index = 0;
            foreach (var item in self)
            {
                res[index++] = selector(item);
            }
            return res;
        }
    }
}
