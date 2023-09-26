using MySql.Data.MySqlClient;
using System.Data;
using TShockAPI.DB;

namespace ComfyEconomy.Database {
    public class DbManager {
        private IDbConnection _db;

        public DbManager(IDbConnection db) {
            _db = db;

            var sqlCreator = new SqlTableCreator(db, new SqliteQueryCreator());

            sqlCreator.EnsureTableStructure(new SqlTable("Accounts",
                new SqlColumn("AccountName", MySqlDbType.String) { Primary = true, Unique = true},
                new SqlColumn("Balance", MySqlDbType.Int32)));

            sqlCreator.EnsureTableStructure(new SqlTable("Mines",
                new SqlColumn("MineID", MySqlDbType.Int32) { Primary = true, Unique = true, AutoIncrement = true },
                new SqlColumn("PosX1", MySqlDbType.Int32),
                new SqlColumn("PosY1", MySqlDbType.Int32),
                new SqlColumn("PosX2", MySqlDbType.Int32),
                new SqlColumn("PosY2", MySqlDbType.Int32),
                new SqlColumn("TileID", MySqlDbType.Int32),
                new SqlColumn("PaintID", MySqlDbType.Int32)
                ));

        }


        // ACCOUNT METHODS
        /// <exception cref="NullReferenceException"></exception>
        public Account GetAccount(string name) {
            using var reader = _db.QueryReader("SELECT * FROM Accounts WHERE AccountName = @0", name);
            while (reader.Read()) {
                return new Account(
                    reader.Get<string>("AccountName"),
                    reader.Get<int>("Balance")
                );
            }
            throw new NullReferenceException();
        }

        public bool InsertAccount(string name, int balance) {
            return _db.Query("INSERT INTO Accounts (AccountName, Balance) VALUES (@0, @1)", name, balance) != 0;
        }

        public bool DeleteAccount(string accountName) {
            return _db.Query("DELETE FROM Accounts WHERE AccountName = @0", accountName) != 0;
        }

        public bool SaveAccount(string accountName, int balance) {
            return _db.Query("UPDATE Accounts SET Balance = @0 WHERE AccountName = @1",
                balance, accountName) != 0;
        }


        // MINE METHODS
        /// <exception cref="NullReferenceException"></exception>
        public Mine GetMine(int id) {
            using var reader = _db.QueryReader("SELECT * FROM Mines WHERE MineID = @0", id);
            while (reader.Read()) {
                return new Mine(
                    reader.Get<int>("MineID"),
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
        public int GetMineIdFromX1Y1(int x1, int y1) {
            using var reader = _db.QueryReader("SELECT * FROM Mines WHERE PosX1 = @0 AND PosY1 = @1", x1, y1);
            while (reader.Read()) {
                return reader.Get<int>("MineID");
            }
            throw new NullReferenceException();
        }
        public bool InsertMine(int posX1, int posY1, int posX2, int posY2, int tileId, int paintId) {
            return _db.Query("INSERT INTO Mines (PosX1, PosY1, PosX2, PosY2, TileID, PaintID) VALUES (@0, @1, @2, @3, @4, @5)", posX1, posY1, posX2, posY2, tileId, paintId) != 0;
        }
        public bool DeleteMine(int mineId) {
            return _db.Query("DELETE FROM Mines WHERE MineID = @0", mineId) != 0;
        }

        public List<Mine> GetAllMines() {
            List<Mine> mines = new List<Mine>();
            using var reader = _db.QueryReader("SELECT * FROM Mines");
            while (reader.Read()) {
                mines.Add(new Mine(
                    reader.Get<int>("MineID"),
                    reader.Get<int>("PosX1"),
                    reader.Get<int>("PosY1"),
                    reader.Get<int>("PosX2"),
                    reader.Get<int>("PosY2"),
                    reader.Get<int>("TileID"),
                    reader.Get<int>("PaintID")
                ));
            }

            return mines;
        }
    }
}
