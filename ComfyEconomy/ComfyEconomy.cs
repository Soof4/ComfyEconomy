using ComfyEconomy.Database;
using Microsoft.Data.Sqlite;
using System.Data;
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
        public override Version Version => new Version(1, 0, 0);
        public override string Author => "Soofa";
        public override string Description => "Simple economy plugin.";

        private DateTime savedTime = DateTime.UtcNow;
        public static List<Mine> mines = new();
        private IDbConnection db;
        public static DbManager dbManager;

        public override void Initialize() {
            db = new SqliteConnection(("Data Source=" + Path.Combine(TShock.SavePath, "ComfyEconomy.sqlite")));
            dbManager = new DbManager(db);

            ServerApi.Hooks.NetGreetPlayer.Register(this, OnNetGreetPlayer);
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            GetDataHandlers.Sign += OnSignChange;
            GetDataHandlers.SignRead += OnSignRead;
            GetDataHandlers.TileEdit += OnTileChange;
            Commands.AddCommands();
            
            mines = dbManager.GetAllMines();
        }

        private void OnGameUpdate(EventArgs args) {
            if ((DateTime.UtcNow - savedTime).Minutes > 2) {
                TSPlayer.All.SendInfoMessage("[i:3509]  [c/ff99cc:Refilling the mines. Possible lag spike.]");

                foreach (var mine in mines) {
                    RefillMine(mine.MineID);
                }

                savedTime = DateTime.UtcNow;
            }
        }

        private void OnTileChange(object sender, GetDataHandlers.TileEditEventArgs args) {
            if (args.Action == 0 && (Main.tile[args.X, args.Y].type == 55 || Main.tile[args.X, args.Y].type == 85 || Main.tile[args.X, args.Y].type == 425)) {
                int x = args.X;
                int y = args.Y;
                int signId = ShopSign.GetSignIdByPos2(ref x, ref y);

                if (signId == -1) {
                    args.Player.SendErrorMessage("[Error] Couldn't find sign id.");
                    return;
                }



                if (!args.Player.HasPermission("comfyeco.tester") && (Main.sign[signId].text.StartsWith("-Sell-") || Main.sign[signId].text.StartsWith("-Buy-"))) {
                    args.Player.SendErrorMessage("You're not a tester.");
                    args.Player.SendTileSquareCentered(x, y);
                    args.Handled = true;
                    return;
                }


                ShopSign sign;

                try {
                    sign = dbManager.GetShopSign(x, y);
                    if (sign.Stock > 0) {
                        args.Player.SendErrorMessage("You cannot break a shop sign that still has stock.");
                        args.Player.SendTileSquareCentered(x, y);
                        args.Handled = true;
                    }
                    else {
                        dbManager.DeleteShopSign(x, y);
                        args.Player.SendSuccessMessage("Deleted a shop sign");
                    }
                }
                catch (NullReferenceException) {
                    return;
                }
                
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
            args.Data.Seek(0, SeekOrigin.Begin);
            int signId = args.Data.ReadInt16();
            int posX = args.Data.ReadInt16();
            int posY = args.Data.ReadInt16();
            string newText = args.Data.ReadString();

            if (!args.Player.HasPermission("comfyeco.tester") && (Main.sign[signId].text.StartsWith("-Sell-") || Main.sign[signId].text.StartsWith("-Buy-"))) {
                args.Player.SendErrorMessage("You're not a tester.");
                args.Player.TPlayer.CloseSign();
                args.Handled = true;
                return;
            }


            ShopSign oldShopSign;
            int itemId, amount, cost;
            Nullable<TagType> preType, newType;

            newType = null;

            if (newText.StartsWith("-Buy-")) {
                newType = TagType.buy;
            }
            else if (newText.StartsWith("-Sell-")) {
                newType = TagType.sell;
            }

            try {
                oldShopSign = dbManager.GetShopSign(posX, posY);
                preType = oldShopSign.TagType;
            }
            catch (NullReferenceException) {  // handles new signs (both ns and ss)

                if (newType == null) { // [00]
                    return;
                }

                // Error handling
                try { itemId = TShock.Utils.GetItemByIdOrName(newText.Split('\n')[1])[0].netID; }
                catch (ArgumentOutOfRangeException) {
                    Main.sign[signId].text = "-Error-\nItem not found.";
                    args.Player.SendErrorMessage("[Error] Item not found.");
                    args.Player.TPlayer.CloseSign();
                    args.Handled = true;
                    return;
                }

                if (!int.TryParse(newText.Split('\n')[2], out amount)) {
                    Main.sign[signId].text = "-Error-\nInvalid amount.";
                    args.Player.SendErrorMessage("[Error] Invalid amount.");
                    args.Player.TPlayer.CloseSign();
                    args.Handled = true;
                    return;
                }

                if (!int.TryParse(newText.Split('\n')[3], out cost)) {
                    Main.sign[signId].text = "-Error-\nInvalid cost.";
                    args.Player.SendErrorMessage("[Error] Invalid amount.");
                    args.Player.TPlayer.CloseSign();
                    args.Handled = true;
                    return;
                }
                ////


                if (newType == TagType.buy) { // [01] buy
                    
                    dbManager.InsertShopSign(signId, posX, posY, (TagType)newType, args.Player.Name, itemId, amount, cost, 0);
                }
                else { // [01] sell
                    if(!args.Player.HasPermission("comfyeco.sellsign")) {
                        args.Player.SendErrorMessage("[Error] You don't have permission for -Sell- tag.");
                        
                    }
                    else {
                        dbManager.InsertShopSign(signId, posX, posY, (TagType)newType, args.Player.Name, itemId, amount, cost, 0);
                    }
                    
                }

                Main.sign[signId].text = ShopSign.StandardizeText(newText);
                args.Player.TPlayer.CloseSign();
                args.Handled = true;
                return;
            }

            // Below handles the old shop sign change. [10] [11]

            if (oldShopSign.Stock > 0) {
                args.Player.SendErrorMessage("You need to empty the stock before changing a shop sign.");
                args.Player.TPlayer.CloseSign();
                args.Handled = true;
            }
            else {

                if (newType == null) { // [01]
                    return;
                }

                // Error handling
                try { itemId = TShock.Utils.GetItemByIdOrName(newText.Split('\n')[1])[0].netID; }
                catch (ArgumentOutOfRangeException) {
                    Main.sign[signId].text = "-Error-\nItem not found.";
                    args.Player.SendErrorMessage("[Error] Item not found.");
                    args.Player.TPlayer.CloseSign();
                    args.Handled = true;
                    return;
                }

                if (!int.TryParse(newText.Split('\n')[2], out amount)) {
                    Main.sign[signId].text = "-Error-\nInvalid amount.";
                    args.Player.SendErrorMessage("[Error] Invalid amount.");
                    args.Player.TPlayer.CloseSign();
                    args.Handled = true;
                    return;
                }

                if (!int.TryParse(newText.Split('\n')[3], out cost)) {
                    Main.sign[signId].text = "-Error-\nInvalid cost.";
                    args.Player.SendErrorMessage("[Error] Invalid amount.");
                    args.Player.TPlayer.CloseSign();
                    args.Handled = true;
                    return;
                }

                if (!args.Player.HasPermission("comfyeco.sellsign")) {
                    args.Player.SendErrorMessage("[Error] You don't have permission for -Sell- tag.");

                }
                else {
                    dbManager.DeleteShopSign(posX, posY);
                    dbManager.InsertShopSign(signId, posX, posY, (TagType)newType, args.Player.Name, itemId, amount, cost, 0);
                }
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


            if (!args.Player.HasPermission("comfyeco.tester") && (Main.sign[signID].text.StartsWith("-Sell-") || Main.sign[signID].text.StartsWith("-Buy-"))) {
                args.Player.SendErrorMessage("You're not a tester.");
                args.Player.TPlayer.CloseSign();
                args.Handled = true;
                return;
            }

            ShopSign shopSign;

            try {
                shopSign = dbManager.GetShopSign(posX, posY);                
            }
            catch (NullReferenceException) {
                return;
            }

            if (shopSign.TagType == TagType.sell) {
                shopSign.Sell(args.Player);
            }
            else {
                if (args.Player.Name == shopSign.Owner) {
                    shopSign.Restock(args.Player);
                }
                else {
                    shopSign.Buy(args.Player);
                }
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

        protected override void Dispose(bool disposing) {
            if (disposing) {
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnNetGreetPlayer);
                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
                GetDataHandlers.Sign -= OnSignChange;
                GetDataHandlers.SignRead -= OnSignRead;
                GetDataHandlers.TileEdit -= OnTileChange;
            }
            base.Dispose(disposing);
        }
    }
}