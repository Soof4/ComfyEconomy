using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComfyEconomy.Database {
    public class Account {
        public string AccountName;
        public int Balance;

        public Account(string accountName, int balance) { 
            AccountName = accountName;
            Balance = balance;
        }
    }
}
