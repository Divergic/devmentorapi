namespace TechMentorApi.Business
{
    using TechMentorApi.Model;

    public interface IAccountCache
    {
        Account GetAccount(string username);

        void RemoveAccount(string username);

        void StoreAccount(Account account);
    }
}