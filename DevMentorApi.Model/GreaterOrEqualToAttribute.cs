namespace DevMentorApi.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;

    public class GreaterOrEqualToAttribute : CompareAttribute
    {
        public GreaterOrEqualToAttribute(string otherProperty) : base(otherProperty)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var property = validationContext.ObjectType.GetTypeInfo().GetProperty(OtherProperty);

            if (property == null)
            {
                throw new InvalidOperationException(
                    $"The property '{OtherProperty}' was not found on type '{validationContext.ObjectType.Name}'.");
            }

            var other = property.GetValue(validationContext.ObjectInstance, null);

            if (other == null)
            {
                return ValidationResult.Success;
            }

            var thisValue = (int)value;
            var otherValue = (int)other;

            if (thisValue < otherValue)
            {
                var names = new[]
                {
                    validationContext.MemberName,
                    property.Name
                };

                return new ValidationResult(
                    $"The value of {validationContext.MemberName} must be greater than or equal to {property.Name}.",
                    names);
            }

            return ValidationResult.Success;
        }
    }
}