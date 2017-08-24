namespace TechMentorApi.Business
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Extensions
    {
        internal static List<T> FastToList<T>(this IEnumerable<T> source)
        {
            var items = source as List<T>;

            if (items == null)
            {
                return source.ToList();
            }

            return items;
        }
    }
}