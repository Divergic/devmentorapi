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
            var subjectPart = username;

            if (parts.Length > 1)
            {
                providerPart = parts[0];
                subjectPart = parts[1];
            }

            Provider = providerPart;
            Subject = subjectPart;
        }

        public Guid Id { get; set; }

        public string Provider { get; set; }

        public string Subject { get; set; }

        public string Username
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Provider) &&
                    string.IsNullOrWhiteSpace(Subject))
                {
                    return null;
                }

                return Provider + "|" + Subject;
            }
        }
    }
}