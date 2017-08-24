namespace TechMentorApi.AcceptanceTests
{
    using System;
    using System.Text.RegularExpressions;
    using TechMentorApi.Model;
    using ModelBuilder;

    internal class YearInPastValueGenerator : RelativeValueGenerator
    {
        private static readonly Regex _supportedProperties =
            new Regex("YearLastUsed|YearStarted|YearStartedInTech", RegexOptions.IgnoreCase);

        private static readonly Regex _yearStartedExpression = new Regex("YearStarted", RegexOptions.IgnoreCase);

        public YearInPastValueGenerator() : base(_supportedProperties, typeof(int), typeof(int?))
        {
        }

        protected override object GenerateValue(Type type, string referenceName, IExecuteStrategy executeStrategy)
        {
            if (type.IsNullable())
            {
                // Allow for a 10% the chance that this might be null
                var range = Generator.NextValue(0, 100000);

                if (range < 10000)
                {
                    return null;
                }
            }

            var context = executeStrategy?.BuildChain?.Last?.Value;
            var minimum = GetMinimum(referenceName, context);
            var maximum = DateTimeOffset.UtcNow.Year;

            return Generator.NextValue(minimum, maximum);
        }

        /// <inheritdoc />
        private int GetMinimum(string referenceName, object context)
        {
            int? minimum = null;

            if (referenceName == nameof(Skill.YearLastUsed))
            {
                minimum = GetValue<int?>(_yearStartedExpression, context);
            }

            if (minimum == null)
            {
                return 1989;
            }

            return minimum.Value;
        }

        /// <inheritdoc />
        public override int Priority { get; } = 10000;
    }
}