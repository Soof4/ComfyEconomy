using TShockAPI;
using ComfyEconomy.Database;
using Terraria;
using System.Runtime.Intrinsics.X86;
using Microsoft.Xna.Framework;

namespace ComfyEconomy
{
    public class Commands
    {
        public static void InitializeCommands()
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.bal.check", BalanceCmd, "bal", "balance")
            {
                AllowServer = true,
                HelpText = "Check balance."
            });

            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.bal.admin", BalanceAdminCmd, "baladmin", "balanceadmin")
            {
                AllowServer = true,
                HelpText = "Do balance manipulation.",
                DoLog = true
            });

            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.pay", PayCmd, "pay")
            {
                AllowServer = false,
                HelpText = "Pay someone money. Usage: /pay [name] [amount]",
                DoLog = true
            });

            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.mine", MineCmd, "mine")
            {
                AllowServer = false,
                HelpText = "Create a new mine. Usage: /mine <sub-command> [args]",
                DoLog = true
            });

            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.updateeco", UpdateEcoCmd, "updateeco")
            {
                AllowServer = true,
                HelpText = "This command will do some updates related to ComfyEconomy.\nDO NOT USE THIS COMMAND IF YOU'RE NOT UPDATING FROM AN EARLIER VERSION OF COMFYECONOMY.",
                DoLog = true
            });

            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.toprich", TopRichCmd, "toprich")
            {
                AllowServer = true,
                HelpText = "Shows the top 5 wealthiest players.",
                DoLog = false
            });
        }

        private static void UpdateEcoCmd(CommandArgs args)
        {
            for (int i = 0; i < Sign.maxSigns; i++)
            {
                Sign sign = Main.sign[i];

                if (sign == null || sign.text == null)
                {
                    continue;
                }

                if (sign.text.StartsWith("-Buy-"))
                {
                    string[] lines = sign.text.Split('\n');

                    List<Item> item = TShock.Utils.GetItemByName(lines[1][6..]);

                    if (!lines[1].Contains('#'))
                    {
                        lines[1] = $"Name: {lines[1][6..]} #{item[0].netID}";
                    }

                    lines[3] = $"Price: {lines[3][5..]}";
                    sign.text = string.Join('\n', lines);

                    TSPlayer.All.SendData(PacketTypes.SignNew, "", i);
                }
                else if (sign.text.StartsWith("-S-Buy-"))
                {
                    string[] lines = sign.text.Split('\n');

                    List<Item> item = TShock.Utils.GetItemByName(lines[1][6..]);

                    if (!lines[1].Contains('#'))
                    {
                        lines[1] = $"Name: {lines[1][6..]} #{item[0].netID}";
                    }

                    lines[3] = $"Price: {lines[3][5..]}";
                    sign.text = string.Join('\n', lines);

                    TSPlayer.All.SendData(PacketTypes.SignNew, "", i);
                }
                else if (sign.text.StartsWith("-S-Sell-"))
                {
                    string[] lines = sign.text.Split('\n');

                    List<Item> item = TShock.Utils.GetItemByName(lines[1][6..]);

                    if (!lines[1].Contains('#'))
                    {
                        lines[1] = $"Name: {lines[1][6..]} #{item[0].netID}";
                    }

                    lines[3] = $"Price: {lines[3][5..]}";
                    sign.text = string.Join('\n', lines);

                    TSPlayer.All.SendData(PacketTypes.SignNew, "", i);
                }
            }

            LogManager.Log("Command", args.Player.Name, "Executed /updateeco");
        }

        private static void BalanceAdminCmd(CommandArgs args)
        {
            if (args.Parameters.Count < 3)
            {
                args.Player.SendErrorMessage("Not enough arguments were given to run the command. Usage: /baladmin [subcommand name] [amount] [player name]");
                return;
            }

            string plrName = string.Join(" ", args.Parameters.GetRange(2, args.Parameters.Count - 2));

            if (!int.TryParse(args.Parameters[1], out int amount))
            {
                args.Player.SendErrorMessage("Invalid amount were given.");
                return;
            }

            Account plrAccount;
            try
            {
                plrAccount = ComfyEconomy.DBManager.GetAccount(plrName);
            }
            catch (NullReferenceException)
            {
                args.Player.SendErrorMessage($"Couldn't find the account for {plrName}.");
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
                case "set":
                    ComfyEconomy.DBManager.SaveAccount(plrName, amount);
                    args.Player.SendSuccessMessage($"Successfully set {plrName}'s balance as {amount}.");

                    LogManager.Log("Command", args.Player.Name, $"Executed /baladmin set {amount} {plrAccount.AccountName}");
                    return;
                case "add":
                    ComfyEconomy.DBManager.SaveAccount(plrName, plrAccount.Balance + amount);
                    args.Player.SendSuccessMessage($"Successfully added {amount} to {plrName}'s balance.");

                    LogManager.Log("Command", args.Player.Name, $"Executed /baladmin add {amount} {plrAccount.AccountName}");
                    return;
                case "sub":
                    ComfyEconomy.DBManager.SaveAccount(plrName, plrAccount.Balance - amount);
                    args.Player.SendSuccessMessage($"Successfully subtracted {amount} from {plrName}'s balance.");

                    LogManager.Log("Command", args.Player.Name, $"Executed /baladmin sub {amount} {plrAccount.AccountName}");
                    return;
                default:
                    args.Player.SendErrorMessage("Subcommand not found.\n" +
                        "Command usage: /baladmin [subcommand] [amount] [player name]\n" +
                        "Subcommand list:\n" +
                        "set: Sets player's balance to the amount.\n" +
                        "add: Adds some amount of points to the player's balance.\n" +
                        "sub: Subtracts some amount of points from the player's balance."
                        );
                    return;
            }

        }

        public static void BalanceCmd(CommandArgs args)
        {
            int balance;
            string playerName = args.Player.Name;

            if (args.Parameters.Count > 0)
            {
                playerName = string.Join(" ", args.Parameters);
            }
            else if (!args.Player.RealPlayer)
            {
                args.Player.SendErrorMessage("Only in-game players can check without giving a player name. Do this instead: /bal [player name]");
                return;
            }

            try
            {
                balance = ComfyEconomy.DBManager.GetAccount(playerName).Balance;
            }
            catch (NullReferenceException)
            {
                args.Player.SendErrorMessage($"Couldn't find the account for {playerName}.");
                return;
            }

            args.Player.SendInfoMessage($"{playerName}'s balance is: {balance}");
        }

        public static void PayCmd(CommandArgs args)
        {
            Account payer, paid;
            int amount;

            payer = ComfyEconomy.DBManager.GetAccount(args.Player.Name);

            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("An argument was missing. Command usage: /pay [name] [amount]");
                return;
            }

            if (int.TryParse(args.Parameters[^1], out amount))
            {
                if (amount < 0)
                {
                    args.Player.SendErrorMessage("Cannot pay in negative values.");
                    return;
                }
                else if (amount > payer.Balance)
                {
                    args.Player.SendErrorMessage($"Cannot pay {amount}. Your balance is only: {payer.Balance}.");
                    return;
                }

                try
                {
                    paid = ComfyEconomy.DBManager.GetAccount(string.Join(' ', args.Parameters.GetRange(0, args.Parameters.Count - 1)));
                }
                catch (NullReferenceException)
                {
                    args.Player.SendErrorMessage("Player not found.");
                    return;
                }

                if (payer.AccountName.Equals(paid.AccountName))
                {
                    args.Player.SendErrorMessage("Cannot pay yourself.");
                    return;
                }

                ComfyEconomy.DBManager.SaveAccount(payer.AccountName, payer.Balance - amount);
                ComfyEconomy.DBManager.SaveAccount(paid.AccountName, paid.Balance + amount);

                // Send success messages
                args.Player.SendSuccessMessage($"You've paid {amount} to {paid.AccountName}.");

                List<TSPlayer> paidTSPlayer = TSPlayer.FindByNameOrID($"tsn:{paid.AccountName}");
                if (paidTSPlayer.Count > 0)
                {
                    paidTSPlayer[0].SendSuccessMessage($"{payer.AccountName} has paid you {amount}.");
                }

                LogManager.Log("Command", payer.AccountName, $"Executed /pay {paid.AccountName} {amount}");
            }
            else
            {
                args.Player.SendErrorMessage("Invalid amount of money.");
            }
        }

        public static void MineCmd(CommandArgs args)
        {
            TSPlayer plr = args.Player;

            if (args.Parameters.Count < 1)
            {
                plr.SendErrorMessage("Missing the sub-command.");
                return;
            }

            switch (args.Parameters[0])
            {
                case "set":
                    if (args.Parameters.Count < 2)
                    {
                        plr.SendErrorMessage("You need to specify which point are you going to set. (1 or 2) (eg. /addmine set 1)");
                        return;
                    }

                    int num;
                    if (!int.TryParse(args.Parameters[1], out num) || (num != 1 && num != 2))
                    {
                        plr.SendErrorMessage("Invalid number");
                        return;
                    }

                    plr.TempPoints[num - 1] = Point.Zero;
                    plr.SendInfoMessage($"Hit the block at the {(num == 1 ? "top-left" : "bottom-right")} position of the mine you want to create.");
                    plr.AwaitingTempPoint = num;

                    break;
                case "define":
                case "def":
                    int tId, pId;
                    string name;

                    if (args.Parameters.Count < 3)
                    {
                        plr.SendErrorMessage("Missing arguments. Usage: /mine <tile ID> <paint ID> <name>");
                        return;
                    }

                    try
                    {
                        tId = int.Parse(args.Parameters[1]);
                        pId = int.Parse(args.Parameters[2]);
                        name = string.Join(" ", args.Parameters.GetRange(3, args.Parameters.Count - 3));
                    }
                    catch
                    {
                        plr.SendErrorMessage("Error executing command. Couldn't parse arguments.");
                        return;
                    }

                    if (plr.TempPoints[0] == Point.Zero || plr.TempPoints[1] == Point.Zero) {
                        plr.SendErrorMessage("You need to set the points before using this sub-command. ");
                        return;
                    }

                    ComfyEconomy.DBManager.InsertMine(name, plr.TempPoints[0].X, plr.TempPoints[0].Y,
                                                      plr.TempPoints[1].X, plr.TempPoints[1].Y, tId, pId);
                    ComfyEconomy.Mines = ComfyEconomy.DBManager.GetAllMines();

                    args.Player.SendSuccessMessage("New mine has been added.");
                    Mine.RefillMine(ComfyEconomy.DBManager.GetMineIdFromName(name));

                    LogManager.Log("Command", args.Player.Name, $"Executed /addmine {tId} {pId} {name}");
                    break;
                case "list":
                case "ls":
                    List<Mine> mines = ComfyEconomy.DBManager.GetAllMines();

                    string msg = "Mines:\n";
                    foreach (Mine m in mines)
                    {
                        msg += $"{m.Name},";
                    }
                    msg.Remove(msg.Length - 1);

                    plr.SendInfoMessage(msg);
                    break;
                case "delete":
                case "del":
                    if (args.Parameters.Count < 2)
                    {
                        plr.SendErrorMessage("You need to specify the name of the mine you want to delete.");
                        return;
                    }

                    string mineName = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));

                    if (ComfyEconomy.DBManager.DeleteMine(mineName))
                    {
                        plr.SendSuccessMessage($"Successfully deleted {mineName}.");
                        ComfyEconomy.Mines = ComfyEconomy.DBManager.GetAllMines();
                    }
                    else
                    {
                        plr.SendSuccessMessage($"Couldn't find {mineName}.");
                    }
                    break;
                case "refill":
                    if (args.Parameters.Count < 2) {
                        plr.SendErrorMessage("You need to specify the name of the mşne you want to refill.");
                        return;
                    }

                    string mName = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));

                    foreach (Mine m in ComfyEconomy.Mines) {
                        if (mName == m.Name) {
                            Mine.RefillMine(m.MineID);
                            TSPlayer.All.SendMessage($"[i:3509]  {m.Name} has been refilled.", 153, 255, 204);
                            return;
                        }
                    }
                    break;
                case "help":
                    plr.SendInfoMessage("Sub-Commands:\n" +
                                        "set <1/2>: Sets points before defining a new mine.\n" +
                                        "define <tile ID> <paint ID> <name>: Defines a new mine. You must set two points before using this command.\n" +
                                        "list: Shows a list of all the mines.\n" +
                                        "delete <name>: Deletes a mine\n" +
                                        "refill <name>: Refill a mine");
                    break;
                default:
                    plr.SendErrorMessage("Invalid sub-command. (Valid ones are set/define/list/refill/help)");
                    break;
            }
        }

        public static async void TopRichCmd(CommandArgs args)
        {
            string message = "";

            await Task.Run(() =>
            {
                List<Account> accounts = ComfyEconomy.DBManager.GetAllAccounts();
                accounts.Sort((a1, a2) => a2.Balance - a1.Balance);

                Account a;

                if (accounts.TryGetValue(0, out a))
                {
                    message += $"{1}. {a.AccountName} : {a.Balance}";
                }

                for (int i = 1; i < 5; i++)
                {
                    if (accounts.TryGetValue(i, out a))
                    {
                        message += $"\n{i + 1}. {a.AccountName} : {a.Balance}";
                    }
                }
            });

            args.Player.SendInfoMessage(message);
        }
    }
}
