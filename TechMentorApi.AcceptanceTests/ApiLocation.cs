namespace TechMentorApi.AcceptanceTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using TechMentorApi.Model;

    public static class ApiLocation
    {
        public static Uri Category(NewCategory category)
        {
            return new Uri(Config.WebsiteAddress,
                "/categories/" + category.Group + "/" + UrlEncode(category.Name));
        }

        public static Uri Category(Category category)
        {
            return new Uri(Config.WebsiteAddress,
                "/categories/" + category.Group + "/" + UrlEncode(category.Name));
        }

        public static Uri PhotoFor(PhotoDetails details)
        {
            var photoUri = new Uri(Config.WebsiteAddress, "/profiles/" + details.ProfileId + "/photos/" + details.Id);

            var location = photoUri.ToString();

            if (string.IsNullOrEmpty(details.Hash) == false)
            {
                location += "?hash=" + details.Hash;
            }

            return new Uri(location);
        }

        public static Uri ProfileFor(Guid profileId)
        {
            return new Uri(Config.WebsiteAddress, "/profiles/" + profileId);
        }

        public static Uri ProfilesMatching(IEnumerable<ProfileFilter> filters)
        {
            var criteria = new List<string>();

            foreach (var filter in filters)
            {
                criteria.Add(filter.CategoryGroup.ToString().ToLowerInvariant() + "=" +
                             UrlEncode(filter.CategoryName));
            }

            var query = criteria.Aggregate((x, y) => x + "&" + y);

            return new Uri(Config.WebsiteAddress, "/profiles?" + query);
        }

        private static string UrlEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return WebUtility.UrlEncode(value).Replace("+", "%20");
        }

        public static Uri AccountProfile => new Uri(Config.WebsiteAddress, "/profile/");

        public static Uri AccountProfilePhotos => new Uri(Config.WebsiteAddress, "/profile/photos/");

        public static Uri Categories => new Uri(Config.WebsiteAddress, "/categories/");

        public static Uri Ping => new Uri(Config.WebsiteAddress, "/ping/");

        public static Uri Profiles => new Uri(Config.WebsiteAddress, "/profiles/");
    }
}