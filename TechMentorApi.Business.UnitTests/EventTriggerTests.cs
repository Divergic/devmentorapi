namespace TechMentorApi.Business.UnitTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;
    using Xunit;

    public class EventTriggerTests
    {
        [Fact]
        public void NewCategoryThrowsExceptionWithNullCategoryTest()
        {
            var newCategoryQueue = Substitute.For<INewCategoryQueue>();
            var updatedProfileQueue = Substitute.For<IUpdatedProfileQueue>();

            var sut = new EventTrigger(newCategoryQueue, updatedProfileQueue);

            Func<Task> action = async () => await sut.NewCategory(null, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task NewCategoryWritesCategoryToQueueTest()
        {
            var expected = Model.Create<Category>();

            var newCategoryQueue = Substitute.For<INewCategoryQueue>();
            var updatedProfileQueue = Substitute.For<IUpdatedProfileQueue>();

            var sut = new EventTrigger(newCategoryQueue, updatedProfileQueue);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.NewCategory(expected, tokenSource.Token).ConfigureAwait(false);

                await newCategoryQueue.Received().WriteMessage(expected, null, null, tokenSource.Token)
                    .ConfigureAwait(false);
            }
        }

        [Fact]
        public void ProfileUpdatedThrowsExceptionWithNullCategoryTest()
        {
            var newCategoryQueue = Substitute.For<INewCategoryQueue>();
            var updatedProfileQueue = Substitute.For<IUpdatedProfileQueue>();

            var sut = new EventTrigger(newCategoryQueue, updatedProfileQueue);

            Func<Task> action = async () =>
                await sut.ProfileUpdated(null, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task ProfileUpdatedWritesCategoryToQueueTest()
        {
            var expected = Model.Create<Profile>();

            var newCategoryQueue = Substitute.For<INewCategoryQueue>();
            var updatedProfileQueue = Substitute.For<IUpdatedProfileQueue>();

            var sut = new EventTrigger(newCategoryQueue, updatedProfileQueue);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.ProfileUpdated(expected, tokenSource.Token).ConfigureAwait(false);

                await updatedProfileQueue.Received().WriteMessage(expected, null, null, tokenSource.Token)
                    .ConfigureAwait(false);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullNewCategoryQueueTest()
        {
            var updatedProfileQueue = Substitute.For<IUpdatedProfileQueue>();

            Action action = () => new EventTrigger(null, updatedProfileQueue);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullUpdatedProfileQueueTest()
        {
            var newCategoryQueue = Substitute.For<INewCategoryQueue>();

            Action action = () => new EventTrigger(newCategoryQueue, null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}