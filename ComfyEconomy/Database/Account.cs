namespace ComfyEconomy.Database
{
    public class Account
    {
        public string AccountName { get; set; }
        public int Balance { get; set; }

        public Account(string accountName, int balance)
        {
            AccountName = accountName;
            Balance = balance;
        }
    }
}
