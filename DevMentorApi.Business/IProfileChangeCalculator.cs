namespace DevMentorApi.Business
{
    using DevMentorApi.Model;

    public interface IProfileChangeCalculator
    {
        ProfileChangeResult CalculateChanges(Profile original, UpdatableProfile updated);

        ProfileChangeResult RemoveAllCategoryLinks(Profile original);
    }
}