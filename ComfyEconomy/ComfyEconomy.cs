﻿using ComfyEconomy.Database;
using Microsoft.Data.Sqlite;
using System.Data;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace ComfyEconomy
{
    [ApiVersion(2, 1)]
    public class ComfyEconomy : TerrariaPlugin
    {
        #region Plugin Info

        public ComfyEconomy(Main game) : base(game) { }
        public override string Name => "ComfyEconomy";
        public override Version Version => new Version(1, 5, 5);
        public override string Author => "Soofa";
        public override string Description => "Economy plugin with shop signs and mines.";

        #endregion
        public static TerrariaPlugin? Instance;
        public static List<Mine> Mines = new List<Mine>();
        private static IDbConnection DB = new SqliteConnection("Data Source=" + Path.Combine(TShock.SavePath, "ComfyEconomy.sqlite"));
        public static DbManager DBManager = new DbManager(DB);
        public static Configuration Config = Configuration.Reload();
        public static Dictionary<int, DateTime> ShopSignInteractionTimestamps = new Dictionary<int, DateTime>();

        public override void Initialize()
        {
            Handlers.InitializeHandlers(this);
            Commands.InitializeCommands();
            LogManager.InitializeLogging();

            Mines = DBManager.GetAllMines();
            Config = Configuration.Reload();
            Instance = this;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) Handlers.DisposeHandlers(this);
            base.Dispose(disposing);
        }
    }
}