namespace TechMentorApi.Model
{
    using System;

    public class Account
    {
        public Account()
        {
        }

        public Account(string username)
        {
            ParseUsername(username);
        }

        private void ParseUsername(string username)
        {
            // Split the provider and username from the username
            var parts = username.Split(
                new[]
                {
                    '|'
                },
                StringSplitOptions.RemoveEmptyEntries);
            var providerPart = "Unspecified";
            var usernamePart = username;

            if (parts.Length > 1)
            {
                providerPart = parts[0];
                usernamePart = parts[1];
            }

            Provider = providerPart;
            Username = usernamePart;
        }

        public Guid Id { get; set; }

        public string Provider { get; set; }

        public string Username { get; set; }
    }
}