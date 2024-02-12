using TShockAPI;
using ComfyEconomy.Database;
using Terraria;
using System.Runtime.Intrinsics.X86;

namespace ComfyEconomy {
    public class Commands {
    
        public static void AddCommands() {
            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.bal.check", BalanceCmd, "bal", "balance") {
                AllowServer = true,
                HelpText = "Check balance."
            });

            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.bal.admin", BalanceAdminCmd, "baladmin", "balanceadmin") {
                AllowServer = true,
                HelpText = "Do balance manipulation.",
                DoLog = true
            });

            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.pay", PayCmd, "pay") {
                AllowServer = false,
                HelpText = "Pay someone money. Usage: /pay [name] [amount]",
                DoLog = true
            });

            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.addmine", AddMineCmd, "addmine") {
                AllowServer = false,
                HelpText = "Create a new mine. Usage: /addmine x1 y1 x2 y2 tileId paintId",
                DoLog = true
            });

            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.updateeco", UpdateEcoCmd, "updateeco") {
                AllowServer = true,
                HelpText = "This command will do some updates related to ComfyEconomy.\nDO NOT USE THIS COMMAND IF YOU'RE NOT UPDATING FROM AN EARLIER VERSION OF COMFYECONOMY.",
                DoLog = true
            });

            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.toprich", TopRichCmd, "toprich") {
                AllowServer = true,
                HelpText = "Shows the top 5 wealthiest players.",
                DoLog = false
            });
        }

        private static void UpdateEcoCmd(CommandArgs args) {
            for (int i = 0; i < Sign.maxSigns; i++) {
                Sign sign = Main.sign[i];

                if (sign == null || sign.text == null) {
                    continue;
                }

                if (sign.text.StartsWith("-Buy-")) {
                    string[] lines = sign.text.Split('\n');

                    List<Item> item = TShock.Utils.GetItemByName(lines[1][6..]);

                    if (!lines[1].Contains('#')) {
                        lines[1] = $"Name: {lines[1][6..]} #{item[0].netID}";
                    }
                    
                    lines[3] = $"Price: {lines[3][5..]}";
                    sign.text = string.Join('\n', lines);

                    TSPlayer.All.SendData(PacketTypes.SignNew, "", i);
                }
                else if (sign.text.StartsWith("-S-Buy-")) {
                    string[] lines = sign.text.Split('\n');

                    List<Item> item = TShock.Utils.GetItemByName(lines[1][6..]);

                    if (!lines[1].Contains('#')) {
                        lines[1] = $"Name: {lines[1][6..]} #{item[0].netID}";
                    }

                    lines[3] = $"Price: {lines[3][5..]}";
                    sign.text = string.Join('\n', lines);

                    TSPlayer.All.SendData(PacketTypes.SignNew, "", i);
                }
                else if (sign.text.StartsWith("-S-Sell-")) {
                    string[] lines = sign.text.Split('\n');

                    List<Item> item = TShock.Utils.GetItemByName(lines[1][6..]);

                    if (!lines[1].Contains('#')) {
                        lines[1] = $"Name: {lines[1][6..]} #{item[0].netID}";
                    }

                    lines[3] = $"Price: {lines[3][5..]}";
                    sign.text = string.Join('\n', lines);

                    TSPlayer.All.SendData(PacketTypes.SignNew, "", i);
                }
            }

            LogManager.Log("Command", args.Player.Name, "Executed /updateeco");
        }

        private static void BalanceAdminCmd(CommandArgs args) {
            if (args.Parameters.Count < 3) {
                args.Player.SendErrorMessage("Not enough arguments were given to run the command. Usage: /baladmin [subcommand name] [amount] [player name]");
                return;
            }

            string plrName = string.Join(" ", args.Parameters.GetRange(2, args.Parameters.Count - 2));

            if (!int.TryParse(args.Parameters[1], out int amount)) {
                args.Player.SendErrorMessage("Invalid amount were given.");
                return;
            }

            Account plrAccount;
            try {
                plrAccount = ComfyEconomy.dbManager.GetAccount(plrName);
            }
            catch (NullReferenceException) {
                args.Player.SendErrorMessage($"Couldn't find the account for {plrName}.");
                return;
            }

            switch (args.Parameters[0].ToLower()) {
                case "set":
                    ComfyEconomy.dbManager.SaveAccount(plrName, amount);
                    args.Player.SendSuccessMessage($"Successfully set {plrName}'s balance as {amount}.");

                    LogManager.Log("Command", args.Player.Name, $"Executed /baladmin set {amount} {plrAccount.AccountName}");
                    return;
                case "add":
                    ComfyEconomy.dbManager.SaveAccount(plrName, plrAccount.Balance + amount);
                    args.Player.SendSuccessMessage($"Successfully added {amount} to {plrName}'s balance.");

                    LogManager.Log("Command", args.Player.Name, $"Executed /baladmin add {amount} {plrAccount.AccountName}");
                    return;
                case "sub":
                    ComfyEconomy.dbManager.SaveAccount(plrName, plrAccount.Balance - amount);
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

        public static void BalanceCmd(CommandArgs args) {
            int balance;
            string playerName = args.Player.Name;

            if (args.Parameters.Count > 0) {
                playerName = string.Join(" ", args.Parameters);
            }
            else if (!args.Player.RealPlayer) {
                args.Player.SendErrorMessage("Only in-game players can check without giving a player name. Do this instead: /bal [player name]");
                return;
            }

            try {
                balance = ComfyEconomy.dbManager.GetAccount(playerName).Balance;
            }
            catch (NullReferenceException) {
                args.Player.SendErrorMessage($"Couldn't find the account for {playerName}.");
                return;
            }

            args.Player.SendInfoMessage($"{playerName}'s balance is: {balance}");
        }
        
        public static void PayCmd(CommandArgs args) {
            Account payer, paid;
            int amount;

            payer = ComfyEconomy.dbManager.GetAccount(args.Player.Name);

            if (args.Parameters.Count < 2) {
                args.Player.SendErrorMessage("An argument was missing. Command usage: /pay [name] [amount]");
                return;
            }

            if (int.TryParse(args.Parameters[^1], out amount)) {
                if (amount < 0) {
                    args.Player.SendErrorMessage("Cannot pay in negative values.");
                    return;
                }
                else if (amount > payer.Balance) {
                    args.Player.SendErrorMessage($"Cannot pay {amount}. Your balance is only: {payer.Balance}.");
                    return;
                }

                try {
                    paid = ComfyEconomy.dbManager.GetAccount(string.Join(' ', args.Parameters.GetRange(0, args.Parameters.Count - 1)));
                }
                catch (NullReferenceException) {
                    args.Player.SendErrorMessage("Player not found.");
                    return;
                }

                if (payer.AccountName.Equals(paid.AccountName)) {
                    args.Player.SendErrorMessage("Cannot pay yourself.");
                    return;
                }

                ComfyEconomy.dbManager.SaveAccount(payer.AccountName, payer.Balance - amount);
                ComfyEconomy.dbManager.SaveAccount(paid.AccountName, paid.Balance + amount);

                // Send success messages
                args.Player.SendSuccessMessage($"You've paid {amount} to {paid.AccountName}.");

                List<TSPlayer> paidTSPlayer = TSPlayer.FindByNameOrID($"tsn:{paid.AccountName}");
                if (paidTSPlayer.Count > 0) {
                    paidTSPlayer[0].SendSuccessMessage($"{payer.AccountName} has paid you {amount}.");
                }

                LogManager.Log("Command", payer.AccountName, $"Executed /pay {paid.AccountName} {amount}");
            }
            else {
                args.Player.SendErrorMessage("Invalid amount of money.");
            }
        }

        public static void AddMineCmd(CommandArgs args) {
            int x1, y1, x2, y2, tId, pId;
            try {
                x1 = int.Parse(args.Parameters[0]);
                y1 = int.Parse(args.Parameters[1]);
                x2 = int.Parse(args.Parameters[2]);
                y2 = int.Parse(args.Parameters[3]);
                tId = int.Parse(args.Parameters[4]);
                pId = int.Parse(args.Parameters[5]);
            }
            catch {
                args.Player.SendErrorMessage("Error executing command. Couldn't parse arguments.");
                return;
            }

            ComfyEconomy.dbManager.InsertMine(x1, y1, x2, y2, tId, pId);
            ComfyEconomy.mines = ComfyEconomy.dbManager.GetAllMines();

            args.Player.SendSuccessMessage("New mine has been added.");
            Mine.RefillMine(ComfyEconomy.dbManager.GetMineIdFromX1Y1(x1, y1));

            LogManager.Log("Command", args.Player.Name, $"Executed /addmine {x1} {y1} {x2} {y2} {tId} {pId}");
        }
    
        public static async void TopRichCmd(CommandArgs args) {
            string message = "";

            await Task.Run(() => {
                List<Account> accounts = ComfyEconomy.dbManager.GetAllAccounts();
                accounts.Sort((a1, a2) => a2.Balance - a1.Balance);

                Account a;

                if (accounts.TryGetValue(0, out a)) {
                        message += $"{1}. {a.AccountName} : {a.Balance}";
                }

                for (int i = 1; i < 5; i++) {
                    if (accounts.TryGetValue(i, out a)) {
                        message += $"\n{i + 1}. {a.AccountName} : {a.Balance}";
                    }
                }

                
            });

            args.Player.SendInfoMessage(message);
        }
    }
}
