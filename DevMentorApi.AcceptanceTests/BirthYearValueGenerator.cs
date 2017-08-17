namespace DevMentorApi.AcceptanceTests
{
    using System;
    using System.Text.RegularExpressions;
    using Model;
    using ModelBuilder;

    public class BirthYearValueGenerator : ValueGeneratorMatcher
    {
        public BirthYearValueGenerator()
            : base(
                new Regex(nameof(Profile.BirthYear), RegexOptions.IgnoreCase),
                typeof(int),
                typeof(int?))
        {
        }

        protected override object GenerateValue(Type type, string referenceName, IExecuteStrategy executeStrategy)
        {
            var generateType = type;

            if (generateType.IsNullable())
            {
                // Allow for a 10% the chance that this might be null
                var range = Generator.NextValue(0, 100);

                if (range < 10)
                {
                    return null;
                }
            }

            // Generate a year between 20-70
            var years = Generator.NextValue(20, 70);

            var point = DateTime.UtcNow;

            point = point.AddYears(-years);

            return point.Year;
        }

        /// <inheritdoc />
        public override int Priority { get; } = 1000;
    }
}