using FluentAssertions;
using ModelBuilder;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace TechMentorApi.Model.UnitTests
{
    public class ExportPhotoTests
    {
        [Fact]
        public void CopiesAllInformationWhenCreatedFromPhotoTest()
        {
            var photo = ModelBuilder.Model.Ignoring<Photo>(x => x.Data).Create<Photo>().Set(x => x.ContentType = ".jpg");
            var expected = ModelBuilder.Model.Create<byte[]>();

            using (var inputStream = new MemoryStream(expected))
            {
                photo.Data = inputStream;

                var sut = new ExportPhoto(photo);

                sut.Should().BeEquivalentTo(photo, opt => opt.Excluding(x => x.Data));

                sut.Data.SequenceEqual(expected).Should().BeTrue();
            }
        }

        [Fact]
        public void DoesNotDisposeDataStreamTest()
        {
            var photo = ModelBuilder.Model.Ignoring<Photo>(x => x.Data).Create<Photo>().Set(x => x.ContentType = ".jpg");
            var expected = ModelBuilder.Model.Create<byte[]>();

            using (var inputStream = new MemoryStream(expected))
            {
                photo.Data = inputStream;

                var first = new ExportPhoto(photo);

                first.Should().BeEquivalentTo(photo, opt => opt.Excluding(x => x.Data));

                first.Data.SequenceEqual(expected).Should().BeTrue();

                var second = new ExportPhoto(photo);

                second.Should().BeEquivalentTo(photo, opt => opt.Excluding(x => x.Data));

                second.Data.SequenceEqual(expected).Should().BeTrue();
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullPhotoTest()
        {
            Action action = () => new ExportPhoto(null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}