using ComfyEconomy.Database;
using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
using System.Data;
using System.IO.Streams;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace ComfyEconomy {
    [ApiVersion(2, 1)]
    public class ComfyEconomy : TerrariaPlugin {
        public ComfyEconomy(Main game) : base(game) {
        }
        public override string Name => "ComfyEconomy";
        public override Version Version => new Version(1, 4, 9);
        public override string Author => "Soofa";
        public override string Description => "Economy plugin with shop signs and mines.";

        public static DateTime MineSavedTime = DateTime.UtcNow;
        public static List<Mine> Mines = new List<Mine>();
        private static IDbConnection DB = new SqliteConnection(("Data Source=" + Path.Combine(TShock.SavePath, "ComfyEconomy.sqlite")));
        public static DbManager DBManager = new DbManager(DB);
        public static string ConfigPath = Path.Combine(TShock.SavePath + "/ComfyEconomyConfig.json");
        public static string LogPath = Path.Combine(TShock.SavePath + "/logs/ComfyEconomy");
        public static Config Config = new Config();
        public static bool ForceNextMineRefill = false;
        
        public override void Initialize() {
            Handlers.InitializeHandlers(this);
            Commands.InitializeCommands();
            
            Mines = DBManager.GetAllMines();

            if (!Directory.Exists(LogPath)) {
                Directory.CreateDirectory(LogPath);
            }
            LogPath += $"/{DateTime.Now.ToString("s")}.log";

            if (File.Exists(ConfigPath)) {
                Config = Config.Read();
            }
            else {
                Config.Write();
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                Handlers.DisposeHandlers(this);
            }
            
            base.Dispose(disposing);
        }
    }
}