using ComfyEconomy.Database;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;

namespace ComfyEconomy
{
    public static class Utils
    {
        public static void SendFloatingMsg(TSPlayer plr, string msg, byte r, byte g, byte b)
        {
            NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, plr.Index, -1,
                Terraria.Localization.NetworkText.FromLiteral(msg), (int)new Color(r, g, b).PackedValue,
                plr.X + 16, plr.Y + 32);
        }

        public static void ForceHandleCommand(TSPlayer player, string command)
        {
            Group plrGroup = player.Group;
            player.Group = TShock.Groups.GetGroupByName("superadmin");
            TShockAPI.Commands.HandleCommand(player, command);
            player.Group = plrGroup;
        }

        public static bool IsPlayerInMine(TSPlayer player, Mine mine)
        {
            return player.TileX <= mine.PosX2 && player.TileX + 1 >= mine.PosX1 && player.TileY + 2 >= mine.PosY1 && player.TileY <= mine.PosY2;
        }

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
            // Get rid of \r\n
            text = text.Replace("\r", "");

            // Replace semi-colons with new-line char (semi-colon syntax is targeted for mobile because mobile players can't type in new-line char)
            text = text.Replace(";", "\n");

            // Get lines
            string[] signContent = text.Split('\n');

            if (signContent.Count() < 4)
            {
                return "-Error-\nMissing lines.";
            }

            // Trim lines
            for (int i = 0; i < signContent.Length; i++)
            {
                signContent[i] = signContent[i].Trim();
            }

            // Check for server sign permission
            if (signContent[0].StartsWith("-S-") && !player.HasPermission("comfyeco.serversign"))
            {
                return "-Error-\nYou don't have permission to use this tag.";
            }

            // Check for cooldown            
            string[] firstLine = signContent[0].Trim().Split(' ');
            string cooldown = "";
            bool hasCooldown = false;

            if (firstLine.Length > 1)
            {
                if (firstLine.Contains("global"))
                {
                    cooldown += "GlobalCooldown: ";

                    int hour = 0;
                    int min = 0;
                    int sec = 0;

                    if (firstLine.Length == 2) return "-Error-\nYou need to provide time after the global keyword. <number>h <number>m <number>s";

                    foreach (string tw in firstLine[2..])
                    {
                        if (tw.EndsWith('h'))
                        {
                            int h = 0;

                            if (int.TryParse(tw[..^1], out h))
                            {
                                hour += h;
                            }
                            else
                            {
                                return "-Error-\nInvalid time syntax. Valid syntax is: <number>h <number>m <number>s";
                            }
                        }
                        else if (tw.EndsWith('m'))
                        {
                            int m = 0;

                            if (int.TryParse(tw[..^1], out m))
                            {
                                min += m;
                            }
                            else
                            {
                                return "-Error-\nInvalid time syntax. Valid syntax is: <number>h <number>m <number>s";
                            }
                        }
                        else if (tw.EndsWith('s'))
                        {
                            int s = 0;

                            if (int.TryParse(tw[..^1], out s))
                            {
                                sec += s;
                            }
                            else
                            {
                                return "-Error-\nInvalid time syntax. Valid syntax is: <number>h <number>m <number>s";
                            }
                        }
                        else
                        {
                            return "-Error-\nInvalid time syntax. Valid syntax is: <number>h <number>m <number>s";
                        }
                    }

                    if (hour < 0 || min < 0 || sec < 0)
                    {
                        return "-Error-\nCan't have negative time values.";
                    }

                    if (hour > 0)
                    {
                        cooldown += $"{hour}h ";
                    }
                    if (min > 0)
                    {
                        cooldown += $"{min}m ";
                    }
                    if (sec > 0)
                    {
                        cooldown += $"{sec}s ";
                    }

                    hasCooldown = true;
                }
                else    // Individual cooldown
                {
                    cooldown += "Cooldown: ";

                    int hour = 0;
                    int min = 0;
                    int sec = 0;

                    foreach (string tw in firstLine[1..])
                    {
                        if (tw.EndsWith('h'))
                        {
                            int h = 0;

                            if (int.TryParse(tw[..^1], out h))
                            {
                                hour += h;
                            }
                            else
                            {
                                return "-Error-\nInvalid time syntax. Valid syntax is: <number>h <number>m <number>s";
                            }
                        }
                        else if (tw.EndsWith('m'))
                        {
                            int m = 0;

                            if (int.TryParse(tw[..^1], out m))
                            {
                                min += m;
                            }
                            else
                            {
                                return "-Error-\nInvalid time syntax. Valid syntax is: <number>h <number>m <number>s";
                            }
                        }
                        else if (tw.EndsWith('s'))
                        {
                            int s = 0;

                            if (int.TryParse(tw[..^1], out s))
                            {
                                sec += s;
                            }
                            else
                            {
                                return "-Error-\nInvalid time syntax. Valid syntax is: <number>h <number>m <number>s";
                            }
                        }
                        else
                        {
                            return "-Error-\nInvalid time syntax. Valid syntax is: <number>h <number>m <number>s";
                        }
                    }

                    if (hour < 0 || min < 0 || sec < 0)
                    {
                        return "-Error-\nCan't have negative time values.";
                    }

                    if (hour > 0)
                    {
                        cooldown += $"{hour}h ";
                    }
                    if (min > 0)
                    {
                        cooldown += $"{min}m ";
                    }
                    if (sec > 0)
                    {
                        cooldown += $"{sec}s ";
                    }

                    hasCooldown = true;
                }
            }

            string parsedTag = firstLine.First();

            if (hasCooldown)
            {
                parsedTag += " " + cooldown;
            }

            // Find the sign tag and standardize the text
            switch (firstLine.First())
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

                        return $"{parsedTag}\n" +
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

                        return $"{parsedTag}\n" +
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

                        return $"{parsedTag}\n" +
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

                        return $"{parsedTag}\n" +
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

                        return $"{parsedTag}\n" +
                                $"Name: {itemList[0].Name} #{itemList[0].netID}\n" +
                                $"Amount: {amount}\n" +
                                $"Requirement: {reqItemList[0].Name} #{reqItemList[0].netID}\n" +
                                $"Requirement Amount: {reqAmount}";
                    }
                default: return "-Error-\nUnknown.";
            }
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

        public static bool IsSignInteractionInCooldown(int playerIndex)
        {
            if (ComfyEconomy.ShopSignInteractionTimestamps.ContainsKey(playerIndex))
            {
                if ((DateTime.UtcNow - ComfyEconomy.ShopSignInteractionTimestamps[playerIndex]).TotalMilliseconds < 600)
                {
                    return true;
                }
            }
            else
            {
                ComfyEconomy.ShopSignInteractionTimestamps.Add(playerIndex, DateTime.UtcNow);
            }

            ComfyEconomy.ShopSignInteractionTimestamps[playerIndex] = DateTime.UtcNow;
            return false;
        }

        public static void Console_WriteLine(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}