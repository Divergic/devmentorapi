namespace DevMentorApi.AcceptanceTests
{
    using DevMentorApi.Model;
    using ModelBuilder;

    public class ProfileBuildStrategy : BuildStrategy
    {
        public ProfileBuildStrategy() : base(CreateBuildStrategy())
        {
        }

        private static IBuildStrategy CreateBuildStrategy()
        {
            var compiler = new DefaultBuildStrategyCompiler();

            compiler.AddExecuteOrderRule<Skill>(x => x.YearStarted, 30000);
            compiler.AddExecuteOrderRule<Skill>(x => x.YearLastUsed, 20000);

            compiler.AddValueGenerator<YearInPastValueGenerator>();

            return compiler.Compile();
        }
    }
}