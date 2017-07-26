namespace DevMentorApi.Azure
{
    using DevMentorApi.Model;

    public class AccountAdapter : EntityAdapter<Account>
    {
        public AccountAdapter()
        {
        }

        public AccountAdapter(Account account) : base(account)
        {
        }
        
        protected override string BuildPartitionKey()
        {
            return Value.Provider;
        }

        protected override string BuildRowKey()
        {
            return Value.Username;
        }
    }
}