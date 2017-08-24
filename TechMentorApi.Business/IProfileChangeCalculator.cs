namespace TechMentorApi.Business
{
    using TechMentorApi.Model;

    public interface IProfileChangeCalculator
    {
        ProfileChangeResult CalculateChanges(Profile original, UpdatableProfile updated);

        ProfileChangeResult RemoveAllCategoryLinks(Profile original);
    }
}