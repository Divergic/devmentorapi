namespace DevMentorApi.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DevMentorApi.Model;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public class ProfileResultFilterModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var filters = new List<ProfileResultFilter>();
            var queries = bindingContext.HttpContext.Request.Query;

            foreach (var query in queries)
            {
                CategoryGroup categoryGroup;

                if (Enum.TryParse(query.Key, true, out categoryGroup) == false)
                {
                    continue;
                }

                foreach (var value in query.Value)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    // This appears to be a valid filter
                    var filter = new ProfileResultFilter
                    {
                        CategoryGroup = categoryGroup,
                        CategoryName = value
                    };

                    filters.Add(filter);
                }
            }
            
            //bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);
            bindingContext.Result = ModelBindingResult.Success(filters);

            return Task.CompletedTask;
        }
    }
}