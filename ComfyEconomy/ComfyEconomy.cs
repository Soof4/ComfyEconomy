using ComfyEconomy.Database;
using IL.Terraria.GameContent;
using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
using ModFramework.Plugins;
using System;
using System.Data;
using System.IO;
using System.IO.Streams;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Handlers;
using TShockAPI.Hooks;

namespace ComfyEconomy {
    [ApiVersion(2, 1)]
    public class ComfyEconomy : TerrariaPlugin {
        public ComfyEconomy(Main game) : base(game) {
        }
        public override string Name => "ComfyEconomy";
        public override Version Version => new Version(1, 2, 1);
        public override string Author => "Soofa";
        public override string Description => "Simple economy plugin.";

        private DateTime mineSavedTime = DateTime.UtcNow;
        public static List<Mine> mines = new();
        private IDbConnection db;
        public static DbManager dbManager;
        public static string configPath = Path.Combine(TShock.SavePath + "/ComfyEconomyConfig.json");
        public static Config Config = new Config();
        public override void Initialize() {
            db = new SqliteConnection(("Data Source=" + Path.Combine(TShock.SavePath, "ComfyEconomy.sqlite")));
            dbManager = new DbManager(db);

            ServerApi.Hooks.NetGreetPlayer.Register(this, OnNetGreetPlayer);
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            GetDataHandlers.Sign += OnSignChange;
            GetDataHandlers.SignRead += OnSignRead;
            GeneralHooks.ReloadEvent += OnReload;
            Commands.AddCommands();
            
            mines = dbManager.GetAllMines();

            if (File.Exists(configPath)) {
                Config = Config.Read();
            }
            else {
                Config.Write();
            }
        }

        
        private void OnReload(ReloadEventArgs e) {
            if (File.Exists(configPath)) {
                Config = Config.Read();
            }
            else {
                Config.Write();
            }
            e.Player.SendSuccessMessage("PlaytimeRewards plugin has been reloaded.");
        }

        private void OnGameUpdate(EventArgs args) {
            if ((DateTime.UtcNow - mineSavedTime).TotalMinutes > Config.MineRefillIntervalInMins) {
                TSPlayer.All.SendInfoMessage("[i:3509]  [c/ff99cc:Refilling the mines. Possible lag spike.]");

                foreach (var mine in mines) {
                    RefillMine(mine.MineID);
                }

                mineSavedTime = DateTime.UtcNow;
            }
        }

        private void OnNetGreetPlayer(GreetPlayerEventArgs args) {
            try {
                dbManager.GetAccount(TShock.Players[args.Who].Name);
            }
            catch (NullReferenceException) {
                dbManager.InsertAccount(TShock.Players[args.Who].Name, 100);
                TShock.Players[args.Who].SendInfoMessage("A bank account has been created for you.");
            }
        }

        public void OnSignChange(object sender, GetDataHandlers.SignEventArgs args) {
            if (args.Handled) {
                return;
            }

            args.Data.Seek(0, SeekOrigin.Begin);
            int signId = args.Data.ReadInt16();
            int posX = args.Data.ReadInt16();
            int posY = args.Data.ReadInt16();
            string newText = args.Data.ReadString();


            if (newText.StartsWith("-Buy-") || newText.StartsWith("-S-Buy-") || newText.StartsWith("-S-Sell-")) {
                newText = ShopSign.StandardizeText(newText, args.Player);
                Main.sign[signId].text = newText;
                TSPlayer.All.SendData(PacketTypes.SignNew, newText, signId, posX, posY);
                args.Handled = true;
            }
        }

        public void OnSignRead(object sender, GetDataHandlers.SignReadEventArgs args) {
            args.Data.Seek(0, SeekOrigin.Begin);
            int posX = args.Data.ReadInt16();
            int posY = args.Data.ReadInt16();
            int signID = ShopSign.GetSignIdByPos(posX, posY);

            if (signID == -1) {  // sign is new
                return;
            }

            if (Main.sign[signID].text.StartsWith("-Buy-") && !Main.sign[signID].text.EndsWith(args.Player.Name)) {
                ShopSign sign = ShopSign.GetShopSign(Main.sign[signID].text);
                int chestID = ShopSign.GetChestIdByPos(posX, posY + 2);
                if (chestID == -1) {
                    SendFloatingMsg(args.Player, "This sign is not connected to a chest!", 255, 50, 50);
                }
                else {
                    sign.Buy(args.Player, chestID);
                }
            }/*
            else if (Main.sign[signID].text.StartsWith("-Sell-") && !Main.sign[signID].text.EndsWith(args.Player.Name)) {
                ShopSign sign = ShopSign.GetShopSign(Main.sign[signID].text);
                int chestID = ShopSign.GetChestIdByPos(posX, posY + 2);
                if (chestID == -1) {
                    SendFloatingMsg(args.Player, "This sign is not connected to a chest!", 255, 50, 50);
                }
                else {
                    // sell
                }
            }*/
            else if (Main.sign[signID].text.StartsWith("-S-Buy-")) {
                ShopSign sign = ShopSign.GetShopSign(Main.sign[signID].text);
                sign.ServerBuy(args.Player);
            }
            else if (Main.sign[signID].text.StartsWith("-S-Sell-")) {
                ShopSign sign = ShopSign.GetShopSign(Main.sign[signID].text);
                sign.ServerSell(args.Player);
            }
            else {
                return;
            }

            args.Player.TPlayer.CloseSign();
            args.Handled = true;
        }

        public static void RefillMine(int mineId) {
            Mine mine = ComfyEconomy.dbManager.GetMine(mineId);
            mine.PosX2++;
            mine.PosY2++;

            for (int i=mine.PosX1; i<mine.PosX2; i++) {
                for (int j=mine.PosY1; j<mine.PosY2; j++) {
                    WorldGen.PlaceTile(i, j, mine.TileID, forced: true);
                    WorldGen.paintTile(i, j, (byte)mine.PaintID);
                }
            }

            TSPlayer.All.SendTileRect((short)mine.PosX1, (short)mine.PosY1, (byte)(mine.PosX2 - mine.PosX1 + 1), (byte)(mine.PosY2 - mine.PosY1 + 1));
        }

        public static void SendFloatingMsg(TSPlayer plr, string msg, byte r, byte g, byte b) {
            NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, -1, -1,
                Terraria.Localization.NetworkText.FromLiteral(msg), (int)new Color(r, g, b).PackedValue,
                plr.X, plr.Y + 32);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnNetGreetPlayer);
                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
                GetDataHandlers.Sign -= OnSignChange;
                GetDataHandlers.SignRead -= OnSignRead;
                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }
    }
}