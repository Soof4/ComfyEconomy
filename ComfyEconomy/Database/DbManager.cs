using MySql.Data.MySqlClient;
using System.Data;
using TShockAPI.DB;

namespace ComfyEconomy.Database
{
    public class DbManager
    {
        private IDbConnection _db;

        public DbManager(IDbConnection db)
        {
            _db = db;

            var sqlCreator = new SqlTableCreator(db, new SqliteQueryCreator());

            sqlCreator.EnsureTableStructure(new SqlTable("Accounts",
                new SqlColumn("AccountName", MySqlDbType.String) { Primary = true, Unique = true },
                new SqlColumn("Balance", MySqlDbType.Int32)));

            sqlCreator.EnsureTableStructure(new SqlTable("Mines",
                new SqlColumn("MineID", MySqlDbType.Int32) { Primary = true, Unique = true, AutoIncrement = true },
                new SqlColumn("Name", MySqlDbType.String) { Unique = true },
                new SqlColumn("PosX1", MySqlDbType.Int32),
                new SqlColumn("PosY1", MySqlDbType.Int32),
                new SqlColumn("PosX2", MySqlDbType.Int32),
                new SqlColumn("PosY2", MySqlDbType.Int32),
                new SqlColumn("TileID", MySqlDbType.Int32),
                new SqlColumn("PaintID", MySqlDbType.Int32)
                ));

            sqlCreator.EnsureTableStructure(new SqlTable("Jobs",
                new SqlColumn("JobID", MySqlDbType.Int32) { Primary = true, Unique = true, AutoIncrement = true },
                new SqlColumn("Owner", MySqlDbType.String),
                new SqlColumn("ItemID", MySqlDbType.Int32),
                new SqlColumn("Stack", MySqlDbType.Int32),
                new SqlColumn("Payment", MySqlDbType.Int32),
                new SqlColumn("Active", MySqlDbType.Int32)
            ));
        }


        #region Account Methods

        /// <exception cref="NullReferenceException"></exception>
        public Account GetAccount(string name)
        {
            using var reader = _db.QueryReader("SELECT * FROM Accounts WHERE AccountName = @0", name);
            while (reader.Read())
            {
                return new Account(
                    reader.Get<string>("AccountName"),
                    reader.Get<int>("Balance")
                );
            }
            throw new NullReferenceException();
        }

        public bool InsertAccount(string name, int balance)
        {
            return _db.Query("INSERT INTO Accounts (AccountName, Balance) VALUES (@0, @1)", name, balance) != 0;
        }

        public bool DeleteAccount(string accountName)
        {
            return _db.Query("DELETE FROM Accounts WHERE AccountName = @0", accountName) != 0;
        }

        public bool SaveAccount(string accountName, int balance)
        {
            return _db.Query("UPDATE Accounts SET Balance = @0 WHERE AccountName = @1",
                balance, accountName) != 0;
        }

        public List<Account> GetAllAccounts()
        {
            List<Account> accounts = new List<Account>();
            using var reader = _db.QueryReader("SELECT * FROM Accounts");
            while (reader.Read())
            {
                accounts.Add(new Account(
                    reader.Get<string>("AccountName"),
                    reader.Get<int>("Balance")
                ));
            }

            return accounts;
        }

        public List<Account> GetTop5Accounts()
        {
            List<Account> accounts = new List<Account>();
            using var reader = _db.QueryReader("SELECT * FROM Accounts ORDER BY Balance DESC LIMIT 5");
            while (reader.Read())
            {
                accounts.Add(new Account(
                    reader.Get<string>("AccountName"),
                    reader.Get<int>("Balance")
                ));
            }

            return accounts;

        }
        #endregion


        #region Mine Methods

        /// <exception cref="NullReferenceException"></exception>
        public Mine GetMine(int id)
        {
            using var reader = _db.QueryReader("SELECT * FROM Mines WHERE MineID = @0", id);
            while (reader.Read())
            {
                return new Mine(
                    reader.Get<int>("MineID"),
                    reader.Get<string>("Name"),
                    reader.Get<int>("PosX1"),
                    reader.Get<int>("PosY1"),
                    reader.Get<int>("PosX2"),
                    reader.Get<int>("PosY2"),
                    reader.Get<int>("TileID"),
                    reader.Get<int>("PaintID")
                );
            }
            throw new NullReferenceException();
        }

        /// <exception cref="NullReferenceException"></exception>
        public int GetMineIdFromX1Y1(int x1, int y1)
        {
            using var reader = _db.QueryReader("SELECT * FROM Mines WHERE PosX1 = @0 AND PosY1 = @1", x1, y1);
            while (reader.Read())
            {
                return reader.Get<int>("MineID");
            }
            throw new NullReferenceException();
        }

        /// <exception cref="NullReferenceException"></exception>
        public int GetMineIdFromName(string name)
        {
            using var reader = _db.QueryReader("SELECT * FROM Mines WHERE Name = @0", name);
            while (reader.Read())
            {
                return reader.Get<int>("MineID");
            }
            throw new NullReferenceException();
        }

        public bool InsertMne(Mine mine)
        {
            return _db.Query("INSERT INTO Mines (Name, PosX1, PosY1, PosX2, PosY2, TileID, PaintID) VALUES (@0, @1, @2, @3, @4, @5, @6)",
            mine.Name, mine.PosX1, mine.PosY1, mine.PosX2, mine.PosY2, mine.TileID, mine.PaintID) != 0;
        }
        public bool InsertMine(string name, int posX1, int posY1, int posX2, int posY2, int tileId, int paintId)
        {
            return _db.Query("INSERT INTO Mines (Name, PosX1, PosY1, PosX2, PosY2, TileID, PaintID) VALUES (@0, @1, @2, @3, @4, @5, @6)", name, posX1, posY1, posX2, posY2, tileId, paintId) != 0;
        }
        public bool DeleteMine(int mineId)
        {
            return _db.Query("DELETE FROM Mines WHERE MineID = @0", mineId) != 0;
        }

        public bool DeleteMine(string name)
        {
            return _db.Query("DELETE FROM Mines WHERE Name = @0", name) != 0;
        }

        public List<Mine> GetAllMines()
        {
            List<Mine> mines = new List<Mine>();
            using var reader = _db.QueryReader("SELECT * FROM Mines");
            while (reader.Read())
            {
                mines.Add(new Mine(
                    reader.Get<int>("MineID"),
                    reader.Get<string>("Name"),
                    reader.Get<int>("PosX1"),
                    reader.Get<int>("PosY1"),
                    reader.Get<int>("PosX2"),
                    reader.Get<int>("PosY2"),
                    reader.Get<int>("TileID"),
                    reader.Get<int>("PaintID")
                    )
                );
            }

            return mines;
        }

        #endregion


        #region Job Methods
        public bool InsertJob(string owner, int itemId, int stack, int payment)
        {
            return _db.Query("INSERT INTO Jobs (Owner, ItemID, Stack, Payment, Active) VALUES (@0, @1, @2, @3, 1)", owner, itemId, stack, payment) != 0;
        }

        public bool DeleteJob(int jobId)
        {
            return _db.Query("DELETE FROM Jobs WHERE JobID = @0", jobId) != 0;
        }

        public bool UpdateJob(int jobId, int itemId, int stack, int payment, bool active)
        {
            return _db.Query("UPDATE Jobs SET ItemID = @0, Stack = @1, Payment = @2, Active = @3 WHERE JobID = @4", itemId, stack, payment, active ? 1 : 0, jobId) != 0;
        }

        public Job GetJob(int jobId)
        {
            using var reader = _db.QueryReader("SELECT * FROM Jobs WHERE JobID = @0", jobId);
            while (reader.Read())
            {
                return new Job(
                    reader.Get<int>("JobID"),
                    reader.Get<string>("Owner"),
                    reader.Get<int>("ItemID"),
                    reader.Get<int>("Stack"),
                    reader.Get<int>("Payment"),
                    reader.Get<int>("Active") != 0
                    );
            }
            throw new NullReferenceException();

        }

        public List<Job> GetAllJobs()
        {
            List<Job> jobs = new List<Job>();
            using var reader = _db.QueryReader("SELECT * FROM Jobs");
            while (reader.Read())
            {
                jobs.Add(new Job(
                    reader.Get<int>("JobID"),
                    reader.Get<string>("Owner"),
                    reader.Get<int>("ItemID"),
                    reader.Get<int>("Stack"),
                    reader.Get<int>("Payment"),
                    reader.Get<int>("Active") != 0
                    )
                );
            }
            return jobs;
        }

        public List<Job> GetAllActiveJobs()
        {
            List<Job> jobs = new List<Job>();
            using var reader = _db.QueryReader("SELECT * FROM Jobs WHERE Active = 1");
            while (reader.Read())
            {
                jobs.Add(new Job(
                    reader.Get<int>("JobID"),
                    reader.Get<string>("Owner"),
                    reader.Get<int>("ItemID"),
                    reader.Get<int>("Stack"),
                    reader.Get<int>("Payment"),
                    reader.Get<int>("Active") != 0
                    )
                );
            }
            return jobs;
        }

        public List<Job> GetPlayerDeactiveJobs(string owner)
        {
            List<Job> jobs = new List<Job>();
            using var reader = _db.QueryReader("SELECT * FROM Jobs WHERE Owner = @0 AND Active = 0", owner);
            while (reader.Read())
            {
                jobs.Add(new Job(
                    reader.Get<int>("JobID"),
                    reader.Get<string>("Owner"),
                    reader.Get<int>("ItemID"),
                    reader.Get<int>("Stack"),
                    reader.Get<int>("Payment"),
                    reader.Get<int>("Active") != 0
                    )
                );
            }
            return jobs;
        }
        #endregion

    }
}
