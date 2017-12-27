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

            var sut = new EventTrigger(newCategoryQueue);

            Func<Task> action = async () => await sut.NewCategory(null, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task NewCategoryWritesCategoryToQueueTest()
        {
            var expected = Model.Create<Category>();

            var newCategoryQueue = Substitute.For<INewCategoryQueue>();

            var sut = new EventTrigger(newCategoryQueue);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.NewCategory(expected, tokenSource.Token).ConfigureAwait(false);

                await newCategoryQueue.Received().WriteCategory(expected, tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullNewCategoryQueueTest()
        {
            Action action = () => new EventTrigger(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}