namespace DevMentoryApi.Business.UnitTests
{
    using DevMentorApi.Model;
    using Newtonsoft.Json;

    internal static class Extensions
    {
        public static Profile Clone(this Profile profile)
        {
            var data = JsonConvert.SerializeObject(profile);

            return JsonConvert.DeserializeObject<Profile>(data);
        }
    }
}