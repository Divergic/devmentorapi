namespace DevMentorApi.UnitTests.Core
{
    using System;
    using System.Collections.Generic;
    using DevMentorApi.Core;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
    using Model;
    using NSubstitute;
    using Xunit;

    public class ProfileFilterModelBinderProviderTests
    {
        [Fact]
        public void GetBinderReturnsBinderForSupportedTypeTest()
        {
            var context = Substitute.For<ModelBinderProviderContext>();
            var provider = Substitute.For<IModelMetadataProvider>();
            var detailsProvider = Substitute.For<ICompositeMetadataDetailsProvider>();
            var key = ModelMetadataIdentity.ForType(typeof(IEnumerable<ProfileFilter>));
            IEnumerable<object> typeAttributes = new List<object>();
            var attributes = new ModelAttributes(typeAttributes);
            var details = new DefaultMetadataDetails(key, attributes);
            var metadata = new DefaultModelMetadata(provider, detailsProvider, details);

            context.Metadata.Returns(metadata);

            var sut = new ProfileFilterModelBinderProvider();

            var actual = sut.GetBinder(context);

            actual.Should().BeOfType<BinderTypeModelBinder>();
        }

        [Fact]
        public void GetBinderReturnsNullForUnsupportedTypeTest()
        {
            var context = Substitute.For<ModelBinderProviderContext>();
            var provider = Substitute.For<IModelMetadataProvider>();
            var detailsProvider = Substitute.For<ICompositeMetadataDetailsProvider>();
            var key = ModelMetadataIdentity.ForType(typeof(string));
            IEnumerable<object> typeAttributes = new List<object>();
            var attributes = new ModelAttributes(typeAttributes);
            var details = new DefaultMetadataDetails(key, attributes);
            var metadata = new DefaultModelMetadata(provider, detailsProvider, details);

            context.Metadata.Returns(metadata);

            var sut = new ProfileFilterModelBinderProvider();

            var actual = sut.GetBinder(context);

            actual.Should().BeNull();
        }

        [Fact]
        public void GetBinderThrowsExceptionWithNullContextTest()
        {
            var sut = new ProfileFilterModelBinderProvider();

            Action action = () => sut.GetBinder(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}