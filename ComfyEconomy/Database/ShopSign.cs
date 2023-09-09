using Microsoft.Xna.Framework;
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
using static System.Net.Mime.MediaTypeNames;

namespace ComfyEconomy.Database {
    public class ShopSign {

        public int ItemID;
        public int Amount;
        public int Cost;
        public string Owner;

        public ShopSign(int itemID, int amount, int cost, string owner) {
            ItemID = itemID;
            Amount = amount;
            Cost = cost;
            Owner = owner;
        }


        public void Buy(TSPlayer buyer, int chestID) {

            int stock = 0;
            foreach (Item item in Main.chest[chestID].item) {
                if (item.netID == ItemID) {
                    stock += item.stack;
                }
            }

            if (stock < Amount) {
                ComfyEconomy.SendFloatingMsg(buyer, "Ran out of stock!", 255, 50, 50);
                return;
            }

            Account sellerAccount;
            Account buyerAccount = ComfyEconomy.dbManager.GetAccount(buyer.Name);
            try {
                sellerAccount = ComfyEconomy.dbManager.GetAccount(Owner);
            }
            catch (NullReferenceException) {
                ComfyEconomy.SendFloatingMsg(buyer, "Couldn't find the owner!", 255, 50, 50);
                return;
            }
             
            
            if (buyerAccount.Balance < Cost) {
                ComfyEconomy.SendFloatingMsg(buyer, "You don't have enough money!", 255, 50, 50);
                return;
            }

            ComfyEconomy.dbManager.SaveAccount(buyer.Name, buyerAccount.Balance - Cost);
            buyer.GiveItem(ItemID, Amount);
            DeleteItemsFromChest(chestID, ItemID, Amount);
            ComfyEconomy.dbManager.SaveAccount(sellerAccount.AccountName, sellerAccount.Balance + Cost);

            ComfyEconomy.SendFloatingMsg(buyer, $"Bought {Amount} {TShock.Utils.GetItemById(ItemID).Name}", 50, 255, 50);

            return;
        }

        public void ServerBuy(TSPlayer buyer) {

            Account buyerAccount = ComfyEconomy.dbManager.GetAccount(buyer.Name);

            if (buyerAccount.Balance < Cost) {
                ComfyEconomy.SendFloatingMsg(buyer, "You don't have enough money!", 255, 50, 50);
                return;
            }

            ComfyEconomy.dbManager.SaveAccount(buyer.Name, buyerAccount.Balance - Cost);
            buyer.GiveItem(ItemID, Amount);

            ComfyEconomy.SendFloatingMsg(buyer, $"Bought {Amount} {TShock.Utils.GetItemById(ItemID).Name}", 50, 255, 50);

            return;
        }


        /*
        public void Sell(TSPlayer seller) {

            if (seller.SelectedItem.netID != ItemID) {
                ComfyEconomy.SendFloatingMsg(seller, "Item doesn't match!", 255, 50, 50);
                return;
            }
            if (seller.SelectedItem.stack < Amount) {
                ComfyEconomy.SendFloatingMsg(seller, "You don't have enough!", 255, 50, 50);
                return;
            }

            Account sellerAccount = ComfyEconomy.dbManager.GetAccount(seller.Name);
            try {
                Account buyerAccount = ComfyEconomy.dbManager.GetAccount(Owner);
            }
            catch (NullReferenceException) {
                ComfyEconomy.SendFloatingMsg(seller, "Couldn't find the owner!", 255, 50, 50);
                return;
            }

            

            seller.SelectedItem.stack -= Amount;
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(seller.SelectedItem.Name), seller.Index, seller.TPlayer.selectedItem);
            NetMessage.SendData((int)PacketTypes.PlayerSlot, seller.Index, -1, NetworkText.FromLiteral(seller.SelectedItem.Name), seller.Index, seller.TPlayer.selectedItem);

            ComfyEconomy.dbManager.SaveAccount(seller.Name, sellerAccount.Balance + Cost);

            seller.SendSuccessMessage($"Sold {Amount} {TShock.Utils.GetItemById(ItemID).Name}");
            return;
        }
        */
        /*
        private void PutItemsInChest(int chestıD, int itemID, int amount) {

        }
        */
        public void ServerSell(TSPlayer seller) {

            if (seller.SelectedItem.netID != ItemID) {
                ComfyEconomy.SendFloatingMsg(seller, "Item doesn't match!", 255, 50, 50);
                return;
            }
            if (seller.SelectedItem.stack < Amount) {
                ComfyEconomy.SendFloatingMsg(seller, "You don't have enough!", 255, 50, 50);
                return;
            }

            Account sellerAccount = ComfyEconomy.dbManager.GetAccount(seller.Name);

            seller.SelectedItem.stack -= Amount;
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(seller.SelectedItem.Name), seller.Index, seller.TPlayer.selectedItem);
            NetMessage.SendData((int)PacketTypes.PlayerSlot, seller.Index, -1, NetworkText.FromLiteral(seller.SelectedItem.Name), seller.Index, seller.TPlayer.selectedItem);

            ComfyEconomy.dbManager.SaveAccount(seller.Name, sellerAccount.Balance + Cost);

            ComfyEconomy.SendFloatingMsg(seller, $"Sold {Amount} {TShock.Utils.GetItemById(ItemID).Name}", 50, 255, 50);
            return;
        }
        
        private void DeleteItemsFromChest(int chestID, int itemID, int amount) {
            for (int i= 0; i<40; i++) {
                Item item = Main.chest[chestID].item[i];
                if (item.netID == itemID) {
                    if (item.stack < amount) {
                        amount -= item.stack;
                        item.stack = 0;
                        TSPlayer.All.SendData(PacketTypes.ChestItem, "", chestID, i, item.stack, item.prefix, item.netID);
                    }
                    else {
                        item.stack -= amount;
                        TSPlayer.All.SendData(PacketTypes.ChestItem, "", chestID, i, item.stack, item.prefix, item.netID);
                        break;
                    }
                }
            }
        }
       
        public static string StandardizeText(string text, TSPlayer player) {
            text = text.Replace("; ", "\n");
            text = text.Replace(';', '\n');
            string[] signContent = text.Split('\n');
            if (signContent.Count() < 4) {
                return "-Error-\nMissing lines.";
            }
            
            if (!signContent[0].Equals("-Buy-") && !player.HasPermission("comfyeco.serversign")) {
                return "-Error-\nYou don't have permission to use this tag.";
            }

            List<Item> itemList;
            int amount, cost;

            if (!signContent[1].StartsWith("Name:")) {    // creating from scratch
                itemList = TShock.Utils.GetItemByIdOrName(signContent[1]);

                if (itemList.Count < 1) {
                    return "-Error-\nItem could not be found. Typo?";
                }

                if (!int.TryParse(signContent[2], out amount) || itemList[0].maxStack < amount) {
                    return "-Error-\nAmount cannot exceed max stack amount of the item.";
                }

                if (!int.TryParse(signContent[3], out cost)) {
                    return "-Error-\nInvalid cost.";
                }
            }
            else {    // editing an old one
                itemList = TShock.Utils.GetItemByIdOrName(signContent[1][6..]);

                if (itemList.Count < 1) {
                    return "-Error-\nItem could not be found. Typo?";
                }

                if (!int.TryParse(signContent[2][8..], out amount) || itemList[0].maxStack < amount) {
                    return "-Error-\nAmount cannot exceed max stack amount of the item.";
                }

                if (!int.TryParse(signContent[3], out cost)) {
                    return "-Error-\nInvalid cost.";
                }
            }

            if (signContent[0].Equals("-Buy-")) {
                return $"{signContent[0]}\n" +
                    $"Name: {itemList[0].Name}\n" +
                    $"Amount: {amount}\n" +
                    $"Cost: {cost}\n" +
                    $"Owner: {player.Name}";
            }
            else {
                return $"{signContent[0]}\n" +
                    $"Name: {itemList[0].Name}\n" +
                    $"Amount: {amount}\n" +
                    $"Cost: {cost}";
            }

        }

        public static ShopSign GetShopSign(string text) {
            string[] lines = text.Split('\n');
            if (lines[0].Equals("-Buy-") || lines[0].Equals("-Sell-")) {
                return new ShopSign(TShock.Utils.GetItemByName(lines[1][6..])[0].netID,
                int.Parse(lines[2][8..]),
                int.Parse(lines[3][6..]),
                lines[4][7..]
                );
            }
            return new ShopSign(TShock.Utils.GetItemByName(lines[1][6..])[0].netID,
                int.Parse(lines[2][8..]),
                int.Parse(lines[3][6..]),
                ""
                );

        }
        public static int GetSignIdByPos(int x, int y) {
            for (int i = 0; i < 1000; i++) {
                if (Main.sign[i] != null && Main.sign[i].x == x && Main.sign[i].y == y) {
                    return i;
                }
            }
            return -1;
        }

        public static int GetChestIdByPos(int x, int y) {
            for (int i = 0; i < Main.maxChests; i++) {
                if (Main.chest[i] != null && Main.chest[i].x == x && Main.chest[i].y == y) {
                    return i;
                }
            }
            return -1;
        }

    }

    public enum Tag {
        Buy,
        Sell,
        ServerBuy,
        ServerSell
    }
}
