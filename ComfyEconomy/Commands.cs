using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using ComfyEconomy.Database;


namespace ComfyEconomy {
    public class Commands {
    
        public static void AddCommands() {
            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.bal", Commands.BalanceCmd, "bal", "balance") {
                AllowServer = false,
                HelpText = "Check your wallet."
            });
            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.pay", Commands.PayCmd, "pay") {
                AllowServer = false,
                HelpText = "Pay someone money. Usage: /pay [name] [amount]"
            });

            TShockAPI.Commands.ChatCommands.Add(new Command("comfyeco.addmine", Commands.AddMineCmd, "addmine") {
                AllowServer = false,
                HelpText = "Create a new mine. Usage: /addmine x1 y1 x2 y2 tileId paintId"
            });
        }
        public static void BalanceCmd(CommandArgs args) {
            args.Player.SendInfoMessage($"Your Balance is: {ComfyEconomy.dbManager.GetAccount(args.Player.Name).Balance}");
        }
        
        public static void PayCmd(CommandArgs args) {
            Account payer, paid;
            int amount;

            payer = ComfyEconomy.dbManager.GetAccount(args.Player.Name);

            if (int.TryParse(args.Parameters[^1], out amount)) {
                if (amount < 0) {
                    args.Player.SendErrorMessage("Did you just try to steal someone's money?! SUSSY BAKA!");
                    return;
                }
                else if (amount > payer.Balance) {
                    args.Player.SendErrorMessage($"Cannot pay {amount}. Your balance is only: {payer.Balance}.");
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
            ComfyEconomy.RefillMine(ComfyEconomy.dbManager.GetMineIdFromX1Y1(x1, y1));
        }
    }
}
