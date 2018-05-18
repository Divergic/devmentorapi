namespace TechMentorApi.Model.UnitTests
{
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using FluentAssertions;
    using Xunit;

    public class UpdatableProfileTests
    {
        [Fact]
        public void IsCreatedWithDefaultValuesTest()
        {
            var sut = new UpdatableProfile();

            sut.Languages.Should().NotBeNull();
            sut.Skills.Should().NotBeNull();
            sut.Status.Should().Be(ProfileStatus.Hidden);
        }

        [Theory]
        [InlineData(ProfileStatus.Available, false, false, true)]
        [InlineData(ProfileStatus.Available, false, true, true)]
        [InlineData(ProfileStatus.Available, true, false, true)]
        [InlineData(ProfileStatus.Available, true, true, false)]
        [InlineData(ProfileStatus.Unavailable, false, false, true)]
        [InlineData(ProfileStatus.Unavailable, false, true, true)]
        [InlineData(ProfileStatus.Unavailable, true, false, true)]
        [InlineData(ProfileStatus.Unavailable, true, true, false)]
        [InlineData(ProfileStatus.Hidden, false, false, false)]
        [InlineData(ProfileStatus.Hidden, false, true, false)]
        [InlineData(ProfileStatus.Hidden, true, false, false)]
        [InlineData(ProfileStatus.Hidden, true, true, false)]
        public void ValidateEvaluatesConsentTest(ProfileStatus status, bool acceptCoC, bool acceptToS,
            bool hasErrors)
        {
            var sut = new UpdatableProfile
            {
                AcceptCoC = acceptCoC,
                AcceptToS = acceptToS,
                Status = status
            };

            var context = new ValidationContext(sut);

            var errors = sut.Validate(context).ToList();

            if (hasErrors)
            {
                errors.Should().NotBeEmpty();
            }
            else
            {
                errors.Should().BeEmpty();
            }
        }

        [Fact]
        public void ValidateIncludesCoCConsentErrorTest()
        {
            var sut = new UpdatableProfile
            {
                AcceptCoC = false,
                AcceptToS = true,
                Status = ProfileStatus.Available
            };

            var context = new ValidationContext(sut);

            var errors = sut.Validate(context).ToList();

            errors[0].MemberNames.Should().HaveCount(1);
            errors[0].MemberNames.Should().Contain(x => x == nameof(UpdatableProfile.AcceptCoC));
        }

        [Fact]
        public void ValidateIncludesConsentErrorsTest()
        {
            var sut = new UpdatableProfile
            {
                AcceptCoC = false,
                AcceptToS = false,
                Status = ProfileStatus.Available
            };

            var context = new ValidationContext(sut);

            var errors = sut.Validate(context).ToList();

            errors[0].MemberNames.Should().HaveCount(2);
            errors[0].MemberNames.Should().Contain(x => x == nameof(UpdatableProfile.AcceptCoC));
            errors[0].MemberNames.Should().Contain(x => x == nameof(UpdatableProfile.AcceptToS));
        }

        [Fact]
        public void ValidateIncludesToSConsentErrorTest()
        {
            var sut = new UpdatableProfile
            {
                AcceptCoC = true,
                AcceptToS = false,
                Status = ProfileStatus.Available
            };

            var context = new ValidationContext(sut);

            var errors = sut.Validate(context).ToList();

            errors[0].MemberNames.Should().HaveCount(1);
            errors[0].MemberNames.Should().Contain(x => x == nameof(UpdatableProfile.AcceptToS));
        }
    }
}