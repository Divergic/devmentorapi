namespace TechMentorApi.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using EnsureThat;
    using TechMentorApi.Model;

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

        internal static void RemoveCategory(this ICacheManager cache, ProfileFilter filter)
        {
            Ensure.Any.IsNotNull(cache, nameof(cache));
            Ensure.Any.IsNotNull(filter, nameof(filter));

            cache.RemoveCategory(filter.CategoryGroup, filter.CategoryName);
        }

        internal static void RemoveCategory(this ICacheManager cache, Category category)
        {
            Ensure.Any.IsNotNull(cache, nameof(cache));
            Ensure.Any.IsNotNull(category, nameof(category));

            cache.RemoveCategory(category.Group, category.Name);
        }
    }
}