namespace TechMentorApi.Model.UnitTests
{
    using System.IO;
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using Xunit;

    public class PhotoTests
    {
        [Fact]
        public void CopyConstructorCreatesInstanceWithSuppliedValuesTest()
        {
            var originalData = Substitute.For<Stream>();
            var newData = Substitute.For<Stream>();

            var source = Model.Ignoring<Photo>(x => x.Data).Create<Photo>().Set(x => x.Data = originalData);

            using (var sut = new Photo(source, newData))
            {
                sut.Should().BeEquivalentTo(source, opt => opt.Excluding(x => x.Data));
                sut.Data.Should().BeSameAs(newData);
            }
        }

        [Fact]
        public void CreatesWithDefaultValuesTest()
        {
            using (var sut = new Photo())
            {
                sut.ContentType.Should().BeNull();
                sut.Data.Should().BeNull();
                sut.Id.Should().BeEmpty();
                sut.ProfileId.Should().BeEmpty();
                sut.Hash.Should().BeNull();
            }
        }

        [Fact]
        public void DisposeCleansUpDataOnlyOnceTest()
        {
            var data = Substitute.For<Stream>();

            using (var sut = new Photo())
            {
                sut.Data = data;

                sut.Dispose();
                sut.Dispose();

                data.Received(1).Dispose();
            }
        }

        [Fact]
        public void DisposeCleansUpDataTest()
        {
            var data = Substitute.For<Stream>();

            using (var sut = new Photo())
            {
                sut.Data = data;

                sut.Dispose();

                data.Received().Dispose();
            }
        }
    }
}