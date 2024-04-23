using NuGet.Protocol.Plugins;
using System;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace ComfyEconomy.Database
{
    public class ShopSign
    {
        public static void DeleteItemsFromChest(int chestID, int itemID, int amount)
        {
            for (int i = 0; i < 40; i++)
            {
                Item item = Main.chest[chestID].item[i];
                if (item.netID == itemID)
                {
                    if (item.stack < amount)
                    {
                        amount -= item.stack;
                        item.stack = 0;
                        TSPlayer.All.SendData(PacketTypes.ChestItem, "", chestID, i, item.stack, item.prefix, item.netID);
                    }
                    else
                    {
                        item.stack -= amount;
                        TSPlayer.All.SendData(PacketTypes.ChestItem, "", chestID, i, item.stack, item.prefix, item.netID);
                        break;
                    }
                }
            }
        }

        public static string StandardizeText(string text, TSPlayer player)
        {

            // Replace semi-colons with new-line char (semi-colon syntax is targeted for mobile because mobile players can't type in new-line char)
            text = text.Replace("; ", "\n");
            text = text.Replace(';', '\n');

            // Get lines
            string[] signContent = text.Split('\n');
            if (signContent.Count() < 4)
            {
                return "-Error-\nMissing lines.";
            }


            if (signContent[0].StartsWith("-S-") && !player.HasPermission("comfyeco.serversign"))
            {
                return "-Error-\nYou don't have permission to use this tag.";
            }

            switch (signContent[0])
            {  // sign tag
                case "-Buy-":
                    {
                        List<Item> itemList;
                        int amount, price;

                        itemList = TShock.Utils.GetItemByIdOrName(signContent[1]);

                        if (itemList.Count < 1)
                        {
                            return "-Error-\nItem could not be found. Typo?";
                        }

                        if (!int.TryParse(signContent[2], out amount) || itemList[0].maxStack < amount || amount < 0)
                        {
                            return "-Error-\nAmount cannot exceed max stack amount of the item or can't be lower than 0.";
                        }

                        if (!int.TryParse(signContent[3], out price) || price < 0)
                        {
                            return "-Error-\nInvalid price.";
                        }

                        return $"-Buy-\n" +
                                $"Name: {itemList[0].Name} #{itemList[0].netID}\n" +
                                $"Amount: {amount} \n" +
                                $"Price: {price} \n" +
                                $"Owner: {player.Name}";
                    }
                case "-S-Buy-":
                    {
                        List<Item> itemList;
                        int amount, price;

                        itemList = TShock.Utils.GetItemByIdOrName(signContent[1]);

                        if (itemList.Count < 1)
                        {
                            return "-Error-\nItem could not be found. Typo?";
                        }

                        if (!int.TryParse(signContent[2], out amount) || itemList[0].maxStack < amount || amount < 0)
                        {
                            return "-Error-\nAmount cannot exceed max stack amount of the item or can't be lower than 0.";
                        }

                        if (!int.TryParse(signContent[3], out price) || price < 0)
                        {
                            return "-Error-\nInvalid price.";
                        }

                        return $"-S-Buy-\n" +
                                $"Name: {itemList[0].Name} #{itemList[0].netID}\n" +
                                $"Amount: {amount} \n" +
                                $"Price: {price}";
                    }
                case "-S-Sell-":
                    {
                        List<Item> itemList;
                        int amount, price;

                        itemList = TShock.Utils.GetItemByIdOrName(signContent[1]);

                        if (itemList.Count < 1)
                        {
                            return "-Error-\nItem could not be found. Typo?";
                        }

                        if (!int.TryParse(signContent[2], out amount) || itemList[0].maxStack < amount || amount < 0)
                        {
                            return "-Error-\nAmount cannot exceed max stack amount of the item or can't be lower than 0.";
                        }

                        if (!int.TryParse(signContent[3], out price) || price < 0)
                        {
                            return "-Error-\nInvalid price.";
                        }

                        return $"-S-Sell-\n" +
                                $"Name: {itemList[0].Name} #{itemList[0].netID}\n" +
                                $"Amount: {amount}\n" +
                                $"Price: {price}";
                    }
                case "-S-Command-":
                    {
                        int price;

                        if (!int.TryParse(signContent[3], out price) || price < 0)
                        {
                            return "-Error-\nInvalid price.";
                        }

                        return $"-S-Command-\n" +
                                $"Command: {signContent[1]}\n" +
                                $"Description: {signContent[2]}\n" +
                                $"Price: {price}";
                    }
                case "-S-Trade-":
                    {
                        List<Item> itemList, reqItemList;
                        int amount, reqAmount;

                        itemList = TShock.Utils.GetItemByIdOrName(signContent[1]);
                        reqItemList = TShock.Utils.GetItemByIdOrName(signContent[3]);

                        if (itemList.Count < 1 || reqItemList.Count < 1)
                        {
                            return "-Error-\nItem could not be found. Typo?";
                        }

                        if (!int.TryParse(signContent[2], out amount) || itemList[0].maxStack < amount || amount < 0 ||
                            !int.TryParse(signContent[4], out reqAmount) || reqItemList[0].maxStack < reqAmount || reqAmount < 0)
                        {
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

        public static BuySign GetBuySign(string text)
        {
            string[] lines = text.Split('\n');

            return new BuySign(
                   int.Parse(lines[1][(lines[1].LastIndexOf('#') + 1)..]),  // read item id that is after the pound (#) sign
                   int.Parse(lines[2][8..]),                                // read the amount
                   int.Parse(lines[3][7..]),                                // read the price
                   lines[4][7..]                                            // read the owner
                   );
        }

        public static ServerBuySign GetServerBuySign(string text)
        {
            string[] lines = text.Split('\n');

            return new ServerBuySign(
                   int.Parse(lines[1][(lines[1].LastIndexOf('#') + 1)..]),      // read item id that is after the pound (#) sign
                   int.Parse(lines[2][8..]),                                    // read the amount
                   int.Parse(lines[3][7..])                                     // read the price
                   );
        }

        public static ServerSellSign GetServerSellSign(string text)
        {
            string[] lines = text.Split('\n');

            return new ServerSellSign(
                   int.Parse(lines[1][(lines[1].LastIndexOf('#') + 1)..]),      // read item id that is after the pound (#) sign
                   int.Parse(lines[2][8..]),                                    // read the amount
                   int.Parse(lines[3][7..])                                     // read the price
                   );
        }

        public static CommandSign GetCommandSign(string text)
        {    //TODO: Update this, get rid of numbers, something more readable
            string[] lines = text.Split('\n');

            return new CommandSign(
                   lines[1][9..],                                         // read command
                   int.Parse(lines[3][7..])                               // read the price
                   );

        }

        public static TradeSign GetTradeSign(string text)
        {
            string[] lines = text.Split("\n");

            return new TradeSign(
                   int.Parse(lines[1][(lines[1].LastIndexOf('#') + 1)..]),
                   int.Parse(lines[2][8..]),
                   int.Parse(lines[3][(lines[3].LastIndexOf('#') + 1)..]),
                   int.Parse(lines[4][20..])
                   );
        }

        public static int GetSignIdByPos(int x, int y)
        {
            for (int i = 0; i < 1000; i++)
            {
                if (Main.sign[i] != null && Main.sign[i].x == x && Main.sign[i].y == y)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int GetChestIdByPos(int x, int y)
        {
            for (int i = 0; i < Main.maxChests; i++)
            {
                if (Main.chest[i] != null && Main.chest[i].x == x && Main.chest[i].y == y)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
