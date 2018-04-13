using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace TechMentorApi.Model.UnitTests
{
    public class ExportProfileTests
    {
        [Fact]
        public void CreatesPhotoSetOnDefaultConstructorTest()
        {
            var sut = new ExportProfile();

            sut.Photos.Should().NotBeNull();
        }

        [Fact]
        public void InitialisesPhotoSetTest()
        {
            var profile = ModelBuilder.Model.Create<Profile>();
            var photos = ModelBuilder.Model.Create<List<ExportPhoto>>();

            var sut = new ExportProfile(profile, photos);

            sut.Should().BeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
            sut.Photos.Should().BeEquivalentTo(photos);
        }
    }
}