using System.IO.Streams;
using Terraria;
using TShockAPI;
using TShockAPI.Hooks;
using TerrariaApi.Server;

namespace ComfyEconomy
{
    public static class Handlers
    {
        private static DateTime MineSavedTime = DateTime.UtcNow;
        private static bool ForceNextMineRefill = false;

        public static void InitializeHandlers(TerrariaPlugin registrator)
        {
            ServerApi.Hooks.NetGreetPlayer.Register(registrator, OnNetGreetPlayer);
            ServerApi.Hooks.GameUpdate.Register(registrator, OnGameUpdate);
            ServerApi.Hooks.GamePostInitialize.Register(registrator, OnGamePostInitialize);
            GetDataHandlers.Sign += OnSignChange;
            GetDataHandlers.SignRead += OnSignRead;
            GeneralHooks.ReloadEvent += OnReload;
        }

        public static void DisposeHandlers(TerrariaPlugin registrator)
        {
            ServerApi.Hooks.NetGreetPlayer.Deregister(registrator, OnNetGreetPlayer);
            ServerApi.Hooks.GameUpdate.Deregister(registrator, OnGameUpdate);
            ServerApi.Hooks.GamePostInitialize.Deregister(registrator, OnGamePostInitialize);
            GetDataHandlers.Sign -= OnSignChange;
            GetDataHandlers.SignRead -= OnSignRead;
            GeneralHooks.ReloadEvent -= OnReload;
        }

        private static void OnGamePostInitialize(EventArgs args)
        {
            UpdateManager.CheckUpdateVerbose(ComfyEconomy.Instance);
        }

        public static void OnReload(ReloadEventArgs args)
        {
            ComfyEconomy.Config = Configuration.Reload();
            args.Player.SendSuccessMessage("ComfyEconomy has been reloaded.");
        }

        public static void OnGameUpdate(EventArgs args)
        {
            if ((DateTime.UtcNow - MineSavedTime).TotalMinutes < ComfyEconomy.Config.MineRefillIntervalInMins) return;

            int activePlrCount = TShock.Utils.GetActivePlayerCount();
            bool minesRefilled = false;

            if (activePlrCount > 0) TSPlayer.All.SendMessage("[i:3509]  Refilling the mines. Possible lag spike.", 255, 153, 204);

            // Check if there are any active players in any of the mines
            if (!ForceNextMineRefill)
            {
                foreach (var mine in ComfyEconomy.Mines)
                {
                    foreach (TSPlayer plr in TShock.Players)
                    {
                        if (plr != null && plr.Active && !plr.Dead && Utils.IsPlayerInMine(plr, mine))
                        {
                            TSPlayer.All.SendMessage($"[i:3509]  Couldn't refill, there were active players in mines.\n" +
                                $"[i:15]  Refilling has been postponed for {ComfyEconomy.Config.MinePostponeMins} mins.", 255, 68, 119);
                            MineSavedTime = MineSavedTime.AddMinutes(ComfyEconomy.Config.MinePostponeMins);
                            ForceNextMineRefill = true;
                            return;
                        }
                    }
                }
            }

            // Try to refill all mines
            foreach (var mine in ComfyEconomy.Mines) if (mine.Refill()) minesRefilled = true;

            // Send the info message
            if (activePlrCount > 0)
            {
                if (minesRefilled) TSPlayer.All.SendMessage("[i:3509]  Mines have been refilled.", 153, 255, 204);
                else TSPlayer.All.SendMessage("[i:3509]  Couldn't refill, mines were already full.", 255, 68, 119);
            }

            ForceNextMineRefill = false;
            MineSavedTime = DateTime.UtcNow;
        }

        public static void OnNetGreetPlayer(GreetPlayerEventArgs args)
        {
            try
            {
                ComfyEconomy.DBManager.GetAccount(TShock.Players[args.Who].Name);
            }
            catch (NullReferenceException)
            {
                ComfyEconomy.DBManager.InsertAccount(TShock.Players[args.Who].Name, 100);
            }
        }

        public static void OnSignChange(object? sender, GetDataHandlers.SignEventArgs args)
        {
            if (args.Handled) return;

            // Reading the data
            args.Data.Seek(0, SeekOrigin.Begin);
            int signId = args.Data.ReadInt16();
            int posX = args.Data.ReadInt16();
            int posY = args.Data.ReadInt16();
            string newText = args.Data.ReadString();

            // Check against region protections
            // IEnumerable<TShockAPI.DB.Region> regions = TShock.Regions.InAreaRegion(posX, posY);
            if (!args.Player.HasBuildPermissionForTileObject(posX, posY, 2, 2, false)) return;

            // Detect shop sign -> standardize text -> change sign -> send packet
            if (newText.StartsWith("-Buy-") ||
                newText.StartsWith("-S-Buy-") ||
                newText.StartsWith("-S-Sell-") ||
                newText.StartsWith("-S-Command-") ||
                newText.StartsWith("-S-Trade-"))
            {
                newText = Utils.StandardizeText(newText, args.Player);
                Main.sign[signId].text = newText;
                TSPlayer.All.SendData(PacketTypes.SignNew, newText, signId, posX, posY);
                args.Handled = true;

                LogManager.Log("Shop-Sign-Creation", args.Player.Name, $"Created a {newText.Split("\n")[0]} tagged shop sign.");
            }
        }

        public static void OnSignRead(object? sender, GetDataHandlers.SignReadEventArgs args)
        {
            // Read the data
            args.Data.Seek(0, SeekOrigin.Begin);
            int posX = args.Data.ReadInt16();
            int posY = args.Data.ReadInt16();
            int signID = Utils.GetSignIdByPos(posX, posY);

            if (signID == -1) return;    // If sign is new -> return

            string text = Main.sign[signID].text;

            if (text.StartsWith("-Buy-") && !text.EndsWith(args.Player.Name))
            {
                BuySign sign = BuySign.GetBuySign(text);
                int chestID = Utils.GetChestIdByPos(posX, posY + 2);
                if (chestID == -1)
                {
                    Utils.SendFloatingMsg(args.Player, "This sign is not connected to a chest!", 255, 50, 50);
                }
                else
                {
                    sign.Buy(args.Player, chestID);
                }
            }
            else if (text.StartsWith("-S-Buy-"))
            {
                ServerBuySign sign = ServerBuySign.GetServerBuySign(text);
                sign.Buy(args.Player);
            }
            else if (text.StartsWith("-S-Sell-"))
            {
                ServerSellSign sign = ServerSellSign.GetServerSellSign(text);
                sign.Sell(args.Player);
            }
            else if (text.StartsWith("-S-Command-"))
            {
                CommandSign sign = CommandSign.GetCommandSign(text);
                sign.ExecuteCommand(args.Player);
            }
            else if (text.StartsWith("-S-Trade-"))
            {
                TradeSign sign = TradeSign.GetTradeSign(text);
                sign.Trade(args.Player);
            }
            else
            {
                return;
            }

            args.Player.TPlayer.CloseSign();
            args.Handled = true;
        }
    }
}