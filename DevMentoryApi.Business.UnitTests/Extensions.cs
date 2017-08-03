namespace DevMentoryApi.Business.UnitTests
{
    using Newtonsoft.Json;

    internal static class Extensions
    {
        public static T Clone<T>(this T profile)
        {
            var data = JsonConvert.SerializeObject(profile);

            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}