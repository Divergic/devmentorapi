namespace DevMentorApi.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using DevMentorApi.Model.Properties;
    using EnsureThat;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ValidPastYearAttribute : ValidationAttribute
    {
        private readonly bool _allowNull;
        private readonly int _minYear;

        public ValidPastYearAttribute(int minYear, bool allowNull = true)
        {
            Ensure.That(minYear, nameof(minYear)).IsLte(DateTimeOffset.UtcNow.Year);

            _minYear = minYear;
            _allowNull = allowNull;
            ErrorMessageResourceName = nameof(Resources.InvalidYearInRangeFormat);
            ErrorMessageResourceType = typeof(Resources);
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return _allowNull;
            }

            if (value is int == false)
            {
                return false;
            }

            var year = (int)value;
            var currentYear = DateTimeOffset.UtcNow.Year;

            if (year > currentYear)
            {
                return false;
            }

            if (year < _minYear)
            {
                return false;
            }

            return true;
        }
    }
}