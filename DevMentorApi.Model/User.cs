namespace DevMentorApi.Model
{
    using EnsureThat;

    public class User
    {
        public User(string username)
        {
            Ensure.That(username, nameof(username)).IsNotNullOrWhiteSpace();

            Username = username;
        }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Username { get; }
    }
}