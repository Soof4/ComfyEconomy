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
        public override Version Version => new Version(1, 3, 0);
        public override string Author => "Soofa";
        public override string Description => "Economy plugin with shop signs and mines.";

        private DateTime mineSavedTime = DateTime.UtcNow;
        public static List<Mine> mines = new();
        private static IDbConnection db = new SqliteConnection(("Data Source=" + Path.Combine(TShock.SavePath, "ComfyEconomy.sqlite")));
        public static DbManager dbManager = new DbManager(db);
        public static string configPath = Path.Combine(TShock.SavePath + "/ComfyEconomyConfig.json");
        public static Config Config = new Config();
        public override void Initialize() {
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
                if (TShock.Utils.GetActivePlayerCount() > 0) {
                    TSPlayer.All.SendInfoMessage("[i:3509]  [c/ff99cc:Refilling the mines. Possible lag spike.]");
                }

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

        public void OnSignChange(object? sender, GetDataHandlers.SignEventArgs args) {
            if (args.Handled) {
                return;
            }

            args.Data.Seek(0, SeekOrigin.Begin);
            int signId = args.Data.ReadInt16();
            int posX = args.Data.ReadInt16();
            int posY = args.Data.ReadInt16();
            string newText = args.Data.ReadString();


            if (newText.StartsWith("-Buy-") || newText.StartsWith("-S-Buy-") || newText.StartsWith("-S-Sell-") || newText.StartsWith("-S-Command-")) {
                newText = ShopSign.StandardizeText(newText, args.Player);
                Main.sign[signId].text = newText;
                TSPlayer.All.SendData(PacketTypes.SignNew, newText, signId, posX, posY);
                args.Handled = true;
            }
        }

        public void OnSignRead(object? sender, GetDataHandlers.SignReadEventArgs args) {
            args.Data.Seek(0, SeekOrigin.Begin);
            int posX = args.Data.ReadInt16();
            int posY = args.Data.ReadInt16();
            int signID = ShopSign.GetSignIdByPos(posX, posY);

            if (signID == -1) {  // sign is new
                return;
            }

            string text = Main.sign[signID].text;

            if (text.StartsWith("-Buy-") && !text.EndsWith(args.Player.Name) /*!Main.sign[signID].text.EndsWith(args.Player.Name)*/) {
                ShopSign.ItemSign sign = ShopSign.GetItemSign(text);
                int chestID = ShopSign.GetChestIdByPos(posX, posY + 2);
                if (chestID == -1) {
                    SendFloatingMsg(args.Player, "This sign is not connected to a chest!", 255, 50, 50);
                }
                else {
                    sign.Buy(args.Player, chestID);
                }
            }
            else if (text.StartsWith("-S-Buy-")) {
                ShopSign.ItemSign sign = ShopSign.GetItemSign(text);
                sign.ServerBuy(args.Player);
            }
            else if (text.StartsWith("-S-Sell-")) {
                ShopSign.ItemSign sign = ShopSign.GetItemSign(text);
                sign.ServerSell(args.Player);
            }
            else if (text.StartsWith("-S-Command-")) {
                ShopSign.CommandSign sign = ShopSign.GetCommandSign(text);
                sign.ExecuteCommand(args.Player);
            }
            else {
                return;
            }

            args.Player.TPlayer.CloseSign();
            args.Handled = true;
        }

        public static bool RefillMine(int mineId) {
            bool isRefilled = false;
            Mine mine = ComfyEconomy.dbManager.GetMine(mineId);
            mine.PosX2++;
            mine.PosY2++;

            for (int i = mine.PosX1; i < mine.PosX2; i++) {
                for (int j = mine.PosY1; j < mine.PosY2; j++) {
                    if (Main.tile[i, j].type != mine.TileID || Main.tile[i, j].color() != mine.PaintID) {
                        WorldGen.PlaceTile(i, j, mine.TileID, forced: true);
                        WorldGen.paintTile(i, j, (byte)mine.PaintID);
                        isRefilled = true;
                    }
                }
            }
            
            if (isRefilled) {
                TSPlayer.All.SendTileRect((short)mine.PosX1, (short)mine.PosY1, (byte)(mine.PosX2 - mine.PosX1 + 1), (byte)(mine.PosY2 - mine.PosY1 + 1));
            }

            return isRefilled;
        }

        public static void SendFloatingMsg(TSPlayer plr, string msg, byte r, byte g, byte b) {
            NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, -1, -1,
                Terraria.Localization.NetworkText.FromLiteral(msg), (int)new Color(r, g, b).PackedValue,
                plr.X + 16, plr.Y + 32);
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