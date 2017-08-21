namespace DevMentorApi.AcceptanceTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Model;

    public static class ApiLocation
    {
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
                             WebUtility.UrlEncode(filter.CategoryName));
            }

            var query = criteria.Aggregate((x, y) => x + "&" + y);

            return new Uri(Config.WebsiteAddress, "/profiles?" + query);
        }

        public static Uri Categories => new Uri(Config.WebsiteAddress, "/categories");

        public static Uri Ping => new Uri(Config.WebsiteAddress, "/ping");

        public static Uri Profiles => new Uri(Config.WebsiteAddress, "/profiles");

        public static Uri AccountProfile => new Uri(Config.WebsiteAddress, "/profile");
    }
}