namespace TechMentorApi.Model
{
    using System;
    using Properties;

    public class NotFoundException : Exception
    {
        public NotFoundException()
            : this(Resources.NotFoundException_Message)
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}