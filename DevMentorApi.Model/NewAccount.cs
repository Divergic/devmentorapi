namespace DevMentorApi.Model
{
    using System;

    public class NewAccount
    {
        public Guid Id
        {
            get;
            set;
        }

        public string Provider
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }
    }
}