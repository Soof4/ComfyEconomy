using NuGet.Protocol.Plugins;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO.Streams;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace ComfyEconomy.Database {
    public class ShopSign {
        public int SignID;
        public int PosX;
        public int PosY;
        public TagType TagType;
        public string Owner;
        public int ItemId;
        public int Amount;
        public int Cost;
        public int Stock;

        public ShopSign(int signId, int posX, int posY, TagType type, string owner, int itemId, int amount, int cost, int stock) {
            SignID = signId;
            PosX = posX;
            PosY = posY;
            TagType = type;
            Owner = owner;
            ItemId = itemId;
            Amount = amount;
            Cost = cost;
            Stock = stock;
        }


        public void Buy(TSPlayer buyer) {
            if (Stock < Amount) {
                buyer.SendErrorMessage("This sign ran out of stock.");
                return;
            }

            Account buyerAccount, sellerAccount;
            try {
                buyerAccount = ComfyEconomy.dbManager.GetAccount(buyer.Name);
                sellerAccount = ComfyEconomy.dbManager.GetAccount(Owner);
            }
            catch (NullReferenceException) {
                buyer.SendErrorMessage("An error has occured.");
                return;
            }
            
            if (buyerAccount.Balance < Cost) {
                buyer.SendErrorMessage($"You don't have enough money to buy that. Your balance is: {buyerAccount.Balance}");
                return;
            }

            ComfyEconomy.dbManager.SaveAccount(buyer.Name, buyerAccount.Balance - Cost);
            buyer.GiveItem(ItemId, Amount);
            ComfyEconomy.dbManager.SaveShopSign(Stock - Amount, PosX, PosY);
            ComfyEconomy.dbManager.SaveAccount(sellerAccount.AccountName, sellerAccount.Balance + Cost);
            buyer.SendSuccessMessage($"Bought {Amount} {TShock.Utils.GetItemById(ItemId).Name}");
            return;
        }

        public void Sell(TSPlayer seller) {

            if (seller.SelectedItem.netID != ItemId) {
                seller.SendErrorMessage("The item you're holding doesn't match the sign.");
                return;
            }
            if (seller.SelectedItem.stack < Amount) {
                seller.SendErrorMessage("Holding item amount is lower than sign requirement.");
                return;
            }

            Account sellerAccount;
            try {
                sellerAccount = ComfyEconomy.dbManager.GetAccount(seller.Name);
            }
            catch (NullReferenceException) {
                seller.SendErrorMessage("An error has occured.");
                return;
            }

            seller.SelectedItem.stack -= Amount;
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(seller.SelectedItem.Name), seller.Index, seller.TPlayer.selectedItem);
            NetMessage.SendData((int)PacketTypes.PlayerSlot, seller.Index, -1, NetworkText.FromLiteral(seller.SelectedItem.Name), seller.Index, seller.TPlayer.selectedItem);

            ComfyEconomy.dbManager.SaveAccount(seller.Name, sellerAccount.Balance + Cost);

            seller.SendSuccessMessage($"Sold {Amount} {TShock.Utils.GetItemById(ItemId).Name}");
            return;
        }

        public void Restock(TSPlayer player) {
            if (player.SelectedItem.netID != ItemId) {
                player.SendErrorMessage("Cannot restock. Item you're holding doesn't match.");
                return;
            }

            Account playerAccount;
            try {
                playerAccount = ComfyEconomy.dbManager.GetAccount(player.Name);
            }
            catch (NullReferenceException) {
                player.SendErrorMessage("An error has occured.");
                return;
            }

            ComfyEconomy.dbManager.SaveShopSign(Stock + player.SelectedItem.stack, PosX, PosY);
            player.SelectedItem.stack = 0;
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(player.SelectedItem.Name), player.Index, player.TPlayer.selectedItem);
            NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.FromLiteral(player.SelectedItem.Name), player.Index, player.TPlayer.selectedItem);
            player.SendInfoMessage("Restocked!");
            return;
        }

        public static string StandardizeText(string text) {
            string[] signContent = text.Split('\n');
            if (signContent.Count() != 4) {
                return "-Error-\nMissing lines or extra lines.";
            }
            List<Item> itemList = TShock.Utils.GetItemByIdOrName(signContent[1]);
            int amount;

            if (!signContent[0].Equals("-Buy-") && !signContent[0].Equals("-Sell-")) {
                return "-Error-\nInvalid shop sign tag. (Use: -Buy- or -Sell-)";
            }

            if (itemList.Count < 1) {
                return "-Error-\nItem could not be found. Typo?";
            }

            if (!int.TryParse(signContent[2], out amount) || itemList[0].maxStack < amount) {
                return "-Error-\nAmount cannot exceed max stack amount of the item.";
                
            }

            return $"{signContent[0]}\nName: {itemList[0].Name}\nAmount: {amount}\nCost: {signContent[3]}";   
        }

        public static int GetSignIdByPos(int x, int y) {
            for (int i = 0; i < 1000; i++) {
                if (Main.sign[i] != null && Main.sign[i].x == x && Main.sign[i].y == y) {
                    return i;
                }
            }
            return -1;
        }

        public static int GetSignIdByPos2(ref int x, ref int y) {

            for (int i = 0; i < 1000; i++) {
                if (Main.sign[i] != null && Main.sign[i].x == x && Main.sign[i].y == y) {
                    return i;
                }
            }

            for (int i = 0; i < 1000; i++) {
                if (Main.sign[i] != null && Main.sign[i].x == x - 1 && Main.sign[i].y == y) {
                    x--;
                    return i;
                }
            }

            for (int i = 0; i < 1000; i++) {
                if (Main.sign[i] != null && Main.sign[i].x == x && Main.sign[i].y == y - 1) {
                    y--;
                    return i;
                }
            }

            for (int i = 0; i < 1000; i++) {
                if (Main.sign[i] != null && Main.sign[i].x == x - 1 && Main.sign[i].y == y - 1) {
                    x--;
                    y--;
                    return i;
                }
            }

            return -1;
        }

    }
    public enum TagType {
        buy,
        sell
    }
}
