namespace TechMentorApi.Azure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IQueueStore<in T>
    {
        Task WriteMessage(
            T message,
            TimeSpan? timeToLive,
            TimeSpan? visibleIn,
            CancellationToken cancellationToken);
    }
}