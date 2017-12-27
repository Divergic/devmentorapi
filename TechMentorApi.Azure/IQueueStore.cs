namespace TechMentorApi.Azure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IQueueStore
    {
        Task WriteMessage(
            string message,
            TimeSpan? timeToLive,
            TimeSpan? visibleIn,
            CancellationToken cancellationToken);
    }
}