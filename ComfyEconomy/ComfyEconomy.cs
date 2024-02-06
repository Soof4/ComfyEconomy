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
        public override Version Version => new Version(1, 4, 7);
        public override string Author => "Soofa";
        public override string Description => "Economy plugin with shop signs and mines.";

        private DateTime mineSavedTime = DateTime.UtcNow;
        public static List<Mine> mines = new List<Mine>();
        private static IDbConnection db = new SqliteConnection(("Data Source=" + Path.Combine(TShock.SavePath, "ComfyEconomy.sqlite")));
        public static DbManager dbManager = new DbManager(db);
        public static string configPath = Path.Combine(TShock.SavePath + "/ComfyEconomyConfig.json");
        public static string logPath = Path.Combine(TShock.SavePath + "/logs/ComfyEconomy");
        public static Config Config = new Config();
        public static bool forceNextMineRefill = false;
        
        public override void Initialize() {
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnNetGreetPlayer);
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            GetDataHandlers.Sign += OnSignChange;
            GetDataHandlers.SignRead += OnSignRead;
            GeneralHooks.ReloadEvent += OnReload;
            Commands.AddCommands();
            
            mines = dbManager.GetAllMines();

            if (!Directory.Exists(logPath)) {
                Directory.CreateDirectory(logPath);
            }
            logPath += $"/{DateTime.Now.ToString("s")}.log";

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
            e.Player.SendSuccessMessage("ComfyEconomy has been reloaded.");
        }

        private void OnGameUpdate(EventArgs args) {
            if ((DateTime.UtcNow - mineSavedTime).TotalMinutes > Config.MineRefillIntervalInMins) {
                int activePlrCount = TShock.Utils.GetActivePlayerCount();
                bool minesRefilled = false;

                if (activePlrCount > 0) {
                    TSPlayer.All.SendMessage("[i:3509]  Refilling the mines. Possible lag spike.", 255, 153, 204);
                }

                if (!forceNextMineRefill) {
                    foreach (var mine in mines) {
                        foreach (TSPlayer plr in TShock.Players) {
                            if (plr != null && plr.Active && !plr.Dead && plr.TileX <= mine.PosX2 && plr.TileX + 1 >= mine.PosX1 && plr.TileY + 2 >= mine.PosY1 && plr.TileY <= mine.PosY2) {
                                TSPlayer.All.SendMessage($"[i:3509]  Couldn't refill, there were active players in mines.\n" +
                                	$"[i:15]  Refilling has been postponed for {Config.MinePostponeMins} mins.", 255, 68, 119);
                                mineSavedTime = mineSavedTime.AddMinutes(Config.MinePostponeMins);
                                forceNextMineRefill = true;
                                return;
                            }
                        }
                    }
                }

                foreach (var mine in mines) {
                    if (Mine.RefillMine(mine.MineID)) {
                        minesRefilled = true;
                    }
                }

                if (activePlrCount > 0) {
                    if (minesRefilled) {
                        TSPlayer.All.SendMessage("[i:3509]  Mines have been refilled.", 153, 255, 204);
                    }
                    else {
                        TSPlayer.All.SendMessage("[i:3509]  Couldn't refill, mines were already full.", 255, 68, 119);
                    }
                }

                forceNextMineRefill = false;
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

            IEnumerable<TShockAPI.DB.Region> region = TShock.Regions.InAreaRegion(posX, posY); 
            if (region.Any() && !region.First().Owner.Equals(args.Player.Name)) {
                return;
            }

            if (newText.StartsWith("-Buy-") || newText.StartsWith("-S-Buy-") || newText.StartsWith("-S-Sell-") || newText.StartsWith("-S-Command-") || newText.StartsWith("-S-Trade-")) {
                newText = ShopSign.StandardizeText(newText, args.Player);
                Main.sign[signId].text = newText;
                TSPlayer.All.SendData(PacketTypes.SignNew, newText, signId, posX, posY);
                args.Handled = true;

                LogManager.Log("Shop-Sign-Creation", args.Player.Name, $"Created a {newText.Split("\n")[0]} tagged shop sign.");
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

            if (text.StartsWith("-Buy-") && !text.EndsWith(args.Player.Name)) {
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
            else if (text.StartsWith("-S-Trade-")) {
                ShopSign.TradeSign sign = ShopSign.GetTradeSign(text);
                sign.Trade(args.Player);
            }
            else {
                return;
            }

            args.Player.TPlayer.CloseSign();
            args.Handled = true;
        }

        public static void SendFloatingMsg(TSPlayer plr, string msg, byte r, byte g, byte b) {
            NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, plr.Index, -1,
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