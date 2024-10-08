using TShockAPI;
using Terraria;
using ComfyEconomy.Database;

namespace ComfyEconomy
{
    public class CommandSign
    {
        public string Command { get; set; }
        public int Price { get; set; }

        public CommandSign(string command, int price)
        {
            Command = command;
            Price = price;
        }

        public void ExecuteCommand(TSPlayer buyer)
        {
            if (Utils.IsSignInteractionInCooldown(buyer.Index)) return;

            Account buyerAccount = ComfyEconomy.DBManager.GetAccount(buyer.Name);

            if (buyerAccount.Balance < Price)
            {
                Utils.SendFloatingMsg(buyer, "You don't have enough money!", 255, 50, 50);
                return;
            }

            ComfyEconomy.DBManager.SaveAccount(buyer.Name, buyerAccount.Balance - Price);
            string command = Command.Replace("%playername%", buyer.Name);

            Utils.ForceHandleCommand(buyer, command);
            Utils.SendFloatingMsg(buyer, $"Executed {command}", 50, 255, 50);

            LogManager.Log("Server-Command-Sign", buyerAccount.AccountName, $"Executed {Command}");
        }

        public static CommandSign GetCommandSign(string text)
        {    //TODO: Update this, get rid of numbers, something more readable
            string[] lines = text.Split('\n');

            return new CommandSign(
                   lines[1][9..],                                         // read command
                   int.Parse(lines[3][7..])                               // read the price
                   );

        }
    }
}