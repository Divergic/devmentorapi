using System;
using System.Collections.Generic;
using System.Text;

namespace DevMentorApi.Model
{
    using System.ComponentModel.DataAnnotations;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ValidPastYearAttribute : ValidationAttribute
    {
        private int? _minYear;

        public ValidPastYearAttribute()
        {
            _minYear = null;
        }

        public ValidPastYearAttribute(int? minYear)
        {
            _minYear = minYear;
        }

        public override bool IsValid(object value)
        {

        }
    }
}
