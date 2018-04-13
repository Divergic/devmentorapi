using FluentAssertions;
using ModelBuilder;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TechMentorApi.Business.Queries;
using TechMentorApi.Model;
using Xunit;

namespace TechMentorApi.Business.UnitTests.Queries
{
    public class ExportQueryTests
    {
        [Fact]
        public async Task GetExportProfileReturnsProfileAndPhotoDataTest()
        {
            var profileId = Guid.NewGuid();
            var profile = ModelBuilder.Model.Create<Profile>();
            var photos = ModelBuilder.Model.Ignoring<Photo>(x => x.Data).Create<List<Photo>>();
            var photoData = ModelBuilder.Model.Create<byte[]>();

            var profileQuery = Substitute.For<IProfileQuery>();
            var photoQuery = Substitute.For<IPhotoQuery>();

            using (var inputStream = new MemoryStream(photoData))
            {
                photos.SetEach(x => x.Data = inputStream);

                using (var tokenSource = new CancellationTokenSource())
                {
                    profileQuery.GetProfile(profileId, tokenSource.Token).Returns(profile);
                    photoQuery.GetPhotos(profileId, tokenSource.Token).Returns(photos);

                    var sut = new ExportQuery(profileQuery, photoQuery);

                    var actual = await sut.GetExportProfile(profileId, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
                    actual.Photos.Should().BeEquivalentTo(photos, opt => opt.Excluding(x => x.Data));
                    actual.Photos.All(x => x.Data.SequenceEqual(photoData)).Should().BeTrue();
                }
            }
        }

        [Fact]
        public void GetExportProfileThrowsExceptionWithEmptyProfileIdTest()
        {
            var profileQuery = Substitute.For<IProfileQuery>();
            var photoQuery = Substitute.For<IPhotoQuery>();

            var sut = new ExportQuery(profileQuery, photoQuery);

            Func<Task> action = async () => await sut.GetExportProfile(Guid.Empty, CancellationToken.None).ConfigureAwait(false);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullPhotoQueryTest()
        {
            var profileQuery = Substitute.For<IProfileQuery>();

            Action action = () => new ExportQuery(profileQuery, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileQueryTest()
        {
            var photoQuery = Substitute.For<IPhotoQuery>();

            Action action = () => new ExportQuery(null, photoQuery);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}