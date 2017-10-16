namespace TechMentorApi.Business.Commands
{
    using TechMentorApi.Model;

    public interface IProfileChangeCalculator
    {
        ProfileChangeResult CalculateChanges(Profile original, UpdatableProfile updated);

        ProfileChangeResult RemoveAllCategoryLinks(UpdatableProfile original);
    }
}