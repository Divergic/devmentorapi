namespace TechMentorApi.Core
{
    using System;
    using System.Collections.Generic;
    using TechMentorApi.Model;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

    public class ProfileFilterModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(IEnumerable<ProfileFilter>))
            {
                return new BinderTypeModelBinder(typeof(ProfileFilterModelBinder));
            }

            return null;
        }
    }
}