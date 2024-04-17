using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
