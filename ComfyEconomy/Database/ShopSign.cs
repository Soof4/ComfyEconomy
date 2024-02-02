using NuGet.Protocol.Plugins;
using System;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace ComfyEconomy.Database {
    public class ShopSign {

        public class ItemSign {
            public int ItemID;
            public int Amount;
            public int Price;
            public string? Owner;

            public ItemSign(int itemID, int amount, int price, string? owner = null) {
                ItemID = itemID;
                Amount = amount;
                Price = price;
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


                if (buyerAccount.Balance < Price) {
                    ComfyEconomy.SendFloatingMsg(buyer, "You don't have enough money!", 255, 50, 50);
                    return;
                }

                ComfyEconomy.dbManager.SaveAccount(buyer.Name, buyerAccount.Balance - Price);
                buyer.GiveItem(ItemID, Amount);
                DeleteItemsFromChest(chestID, ItemID, Amount);
                ComfyEconomy.dbManager.SaveAccount(sellerAccount.AccountName, sellerAccount.Balance + Price);

                ComfyEconomy.SendFloatingMsg(buyer, $"Bought {Amount} {TShock.Utils.GetItemById(ItemID).Name}", 50, 255, 50);

                LogManager.Log("Buy-Sign", buyerAccount.AccountName, $"Bought {Amount} {TShock.Utils.GetItemById(ItemID).Name} from {Owner}");
            }

            public void ServerBuy(TSPlayer buyer) {
                Account buyerAccount = ComfyEconomy.dbManager.GetAccount(buyer.Name);

                if (buyerAccount.Balance < Price) {
                    ComfyEconomy.SendFloatingMsg(buyer, "You don't have enough money!", 255, 50, 50);
                    return;
                }

                ComfyEconomy.dbManager.SaveAccount(buyer.Name, buyerAccount.Balance - Price);
                buyer.GiveItem(ItemID, Amount);

                ComfyEconomy.SendFloatingMsg(buyer, $"Bought {Amount} {TShock.Utils.GetItemById(ItemID).Name}", 50, 255, 50);

                LogManager.Log("Server-Buy-Sign", buyerAccount.AccountName, $"Bought {Amount} {TShock.Utils.GetItemById(ItemID).Name}");
            }

            public void ServerSell(TSPlayer seller) {
                if (seller.SelectedItem.netID != ItemID) {
                    ComfyEconomy.SendFloatingMsg(seller, "Item doesn't match!", 255, 50, 50);
                    return;
                }
                if (seller.SelectedItem.stack < Amount) {
                    ComfyEconomy.SendFloatingMsg(seller, "You don't have enough!", 255, 50, 50);
                    return;
                }

                foreach (Item item in Main.item) {
                    if (item != null && item.active && item.stack == seller.SelectedItem.stack &&
                        item.netID == ItemID && item.prefix == seller.SelectedItem.prefix &&
                        item.position.WithinRange(seller.TPlayer.position, 16 * 40)) {
                        ComfyEconomy.SendFloatingMsg(seller, "You dropped the item!", 255, 50, 50);
                        return;
                    }
                }

                Account sellerAccount = ComfyEconomy.dbManager.GetAccount(seller.Name);

                seller.SelectedItem.stack -= Amount;

                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(seller.SelectedItem.Name), seller.Index, seller.TPlayer.selectedItem);
                NetMessage.SendData((int)PacketTypes.PlayerSlot, seller.Index, -1, NetworkText.FromLiteral(seller.SelectedItem.Name), seller.Index, seller.TPlayer.selectedItem);

                ComfyEconomy.dbManager.SaveAccount(seller.Name, sellerAccount.Balance + Price);

                ComfyEconomy.SendFloatingMsg(seller, $"Sold {Amount} {TShock.Utils.GetItemById(ItemID).Name}", 50, 255, 50);

                LogManager.Log("Server-Sell-Sign", sellerAccount.AccountName, $"Sold {Amount} {TShock.Utils.GetItemById(ItemID).Name}");
            }
        }

        public class CommandSign {
            public string Command;
            public int Price;

            public CommandSign(string command, int price) {
                Command = command;
                Price = price;
            }

            public void ExecuteCommand(TSPlayer buyer) {
                Account buyerAccount = ComfyEconomy.dbManager.GetAccount(buyer.Name);

                if (buyerAccount.Balance < Price) {
                    ComfyEconomy.SendFloatingMsg(buyer, "You don't have enough money!", 255, 50, 50);
                    return;
                }

                ComfyEconomy.dbManager.SaveAccount(buyer.Name, buyerAccount.Balance - Price);
                TShockAPI.Commands.HandleCommand(TSPlayer.Server, Command);

                ComfyEconomy.SendFloatingMsg(buyer, $"Executed {Command}", 50, 255, 50);

                LogManager.Log("Server-Command-Sign", buyerAccount.AccountName, $"Executed {Command}");
            }
        }
        
        public class TradeSign {
            public int ItemID;
            public int Amount;
            public int ReqItemID;
            public int ReqAmount;

            public TradeSign(int itemID, int amount, int reqItemID, int reqAmount) {
                ItemID = itemID;
                Amount = amount;
                ReqItemID = reqItemID;
                ReqAmount = reqAmount;
            }

            public void Trade(TSPlayer buyer) {
                if (buyer.SelectedItem.netID != ReqItemID) {
                    ComfyEconomy.SendFloatingMsg(buyer, "Item doesn't match!", 255, 50, 50);
                    return;
                }
                if (buyer.SelectedItem.stack < ReqAmount) {
                    ComfyEconomy.SendFloatingMsg(buyer, "You don't have enough!", 255, 50, 50);
                    return;
                }

                foreach (Item item in Main.item) {
                    if (item != null && item.active && item.stack == buyer.SelectedItem.stack &&
                        item.netID == ReqItemID && item.prefix == buyer.SelectedItem.prefix && 
                        item.position.WithinRange(buyer.TPlayer.position, 16 * 40)) {
                        ComfyEconomy.SendFloatingMsg(buyer, "You dropped the item!", 255, 50, 50);
                        return;
                    }
                }

                buyer.SelectedItem.stack -= ReqAmount;
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(buyer.SelectedItem.Name), buyer.Index, buyer.TPlayer.selectedItem);
                NetMessage.SendData((int)PacketTypes.PlayerSlot, buyer.Index, -1, NetworkText.FromLiteral(buyer.SelectedItem.Name), buyer.Index, buyer.TPlayer.selectedItem);
                buyer.GiveItem(ItemID, Amount);

                ComfyEconomy.SendFloatingMsg(buyer, $"Bought {Amount} {TShock.Utils.GetItemById(ItemID).Name}", 50, 255, 50);
                
                LogManager.Log("Server-Trade-Sign", buyer.Name, $"Traded {ReqAmount} {TShock.Utils.GetItemById(ReqItemID).Name} in for {Amount} {TShock.Utils.GetItemById(ItemID).Name}");
            }
        }

        private static void DeleteItemsFromChest(int chestID, int itemID, int amount) {
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

            // Replace semi-colons with new-line char (semi-colon syntax is targeted for mobile because mobile players can't type in new-line char)
            text = text.Replace("; ", "\n");
            text = text.Replace(';', '\n');

            // Get lines
            string[] signContent = text.Split('\n');
            if (signContent.Count() < 4) {
                return "-Error-\nMissing lines.";
            }                                                                                           
            

            if (signContent[0].StartsWith("-S-") && !player.HasPermission("comfyeco.serversign")) {
                return "-Error-\nYou don't have permission to use this tag.";
            }

            switch (signContent[0]) {  // sign tag
                case "-Buy-": {
                    List<Item> itemList;
                    int amount, price;

                    itemList = TShock.Utils.GetItemByIdOrName(signContent[1]);

                    if (itemList.Count < 1) {
                        return "-Error-\nItem could not be found. Typo?";
                    }

                    if (!int.TryParse(signContent[2], out amount) || itemList[0].maxStack < amount || amount < 0) {
                        return "-Error-\nAmount cannot exceed max stack amount of the item or can't be lower than 0.";
                    }

                    if (!int.TryParse(signContent[3], out price) || price < 0) {
                        return "-Error-\nInvalid price.";
                    }

                    return $"-Buy-\n" +
                            $"Name: {itemList[0].Name} #{itemList[0].netID}\n" +
                            $"Amount: {amount} \n" +
                            $"Price: {price} \n" +
                            $"Owner: {player.Name}";
                }
                case "-S-Buy-": {
                    List<Item> itemList;
                    int amount, price;

                    itemList = TShock.Utils.GetItemByIdOrName(signContent[1]);

                    if (itemList.Count < 1) {
                        return "-Error-\nItem could not be found. Typo?";
                    }

                    if (!int.TryParse(signContent[2], out amount) || itemList[0].maxStack < amount || amount < 0) {
                        return "-Error-\nAmount cannot exceed max stack amount of the item or can't be lower than 0.";
                    }

                    if (!int.TryParse(signContent[3], out price) || price < 0) {
                        return "-Error-\nInvalid price.";
                    }

                    return $"-S-Buy-\n" +
                            $"Name: {itemList[0].Name} #{itemList[0].netID}\n" +
                            $"Amount: {amount} \n" +
                            $"Price: {price}";
                }
                case "-S-Sell-": {
                    List<Item> itemList;
                    int amount, price;

                    itemList = TShock.Utils.GetItemByIdOrName(signContent[1]);

                    if (itemList.Count < 1) {
                        return "-Error-\nItem could not be found. Typo?";
                    }

                    if (!int.TryParse(signContent[2], out amount) || itemList[0].maxStack < amount || amount < 0) {
                        return "-Error-\nAmount cannot exceed max stack amount of the item or can't be lower than 0.";
                    }

                    if (!int.TryParse(signContent[3], out price) || price < 0) {
                        return "-Error-\nInvalid price.";
                    }

                    return $"-S-Sell-\n" +
                            $"Name: {itemList[0].Name} #{itemList[0].netID}\n" +
                            $"Amount: {amount}\n" +
                            $"Price: {price}";
                }
                case "-S-Command-": {
                    int price;

                    if (!int.TryParse(signContent[3], out price) || price < 0) {
                        return "-Error-\nInvalid price.";
                    }

                    return $"-S-Command-\n" +
                            $"Command: {signContent[1]}\n" +
                            $"Description: {signContent[2]}\n" +
                            $"Price: {price}";
                }
                case "-S-Trade-": {
                    List<Item> itemList, reqItemList;
                    int amount, reqAmount;

                    itemList = TShock.Utils.GetItemByIdOrName(signContent[1]);
                    reqItemList = TShock.Utils.GetItemByIdOrName(signContent[3]);

                    if (itemList.Count < 1 || reqItemList.Count < 1) {
                        return "-Error-\nItem could not be found. Typo?";
                    }

                    if (!int.TryParse(signContent[2], out amount) || itemList[0].maxStack < amount || amount < 0 ||
                        !int.TryParse(signContent[4], out reqAmount) || reqItemList[0].maxStack < reqAmount || reqAmount < 0) {
                        return "-Error-\nAmount cannot exceed max stack amount of the item or can't be lower than 0.";
                    }

                    return $"-S-Trade-\n" +
                            $"Name: {itemList[0].Name} #{itemList[0].netID}\n" +
                            $"Amount: {amount}\n" +
                            $"Requirement: {reqItemList[0].Name} #{reqItemList[0].netID}\n" +
                            $"Requirement Amount: {reqAmount}";
                }
                default: return "-Error-\nUnknown.";
            }
        }

        public static ItemSign GetItemSign(string text) {
            string[] lines = text.Split('\n');

            if (lines[0].Equals("-Buy-") || lines[0].Equals("-Sell-")) {
                return new ItemSign(
                       int.Parse(lines[1][(lines[1].LastIndexOf('#') + 1)..]),  // read item id that is after the pound (#) sign
                       int.Parse(lines[2][8..]),                                // read the amount
                       int.Parse(lines[3][7..]),                                // read the price
                       lines[4][7..]                                            // read the owner
                       );
            }
            return new ItemSign(
                   int.Parse(lines[1][(lines[1].LastIndexOf('#') + 1)..]),      // read item id that is after the pound (#) sign
                   int.Parse(lines[2][8..]),                                    // read the amount
                   int.Parse(lines[3][7..])                                     // read the price
                   );

        }

        public static CommandSign GetCommandSign(string text) {    //TODO: Update this, get rid of numbers, something more readable
            string[] lines = text.Split('\n');

            return new CommandSign(
                   lines[1][9..],                                         // read command
                   int.Parse(lines[3][7..])                               // read the price
                   );

        }

        public static TradeSign GetTradeSign(string text) {
            string[] lines = text.Split("\n");

            return new TradeSign(
                   int.Parse(lines[1][(lines[1].LastIndexOf('#') + 1)..]),
                   int.Parse(lines[2][8..]),
                   int.Parse(lines[3][(lines[3].LastIndexOf('#') + 1)..]),
                   int.Parse(lines[4][20..])
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
        ServerSell,
        ServerCommand
    }
}
