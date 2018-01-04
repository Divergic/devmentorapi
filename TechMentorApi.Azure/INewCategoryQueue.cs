namespace TechMentorApi.Azure
{
    using TechMentorApi.Model;

    public interface INewCategoryQueue : IQueueStore<Category>
    {
    }
}