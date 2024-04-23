using TShockAPI;
using Terraria;
using ComfyEconomy.Database;

namespace ComfyEconomy
{
    public class ServerBuySign
    {
        public int ItemID { get; set; }
        public int Amount { get; set; }
        public int Price { get; set; }

        public ServerBuySign(int itemID, int amount, int price)
        {
            ItemID = itemID;
            Amount = amount;
            Price = price;
        }
        public void Buy(TSPlayer buyer)
        {
            Account buyerAccount = ComfyEconomy.DBManager.GetAccount(buyer.Name);

            if (buyerAccount.Balance < Price)
            {
                Utils.SendFloatingMsg(buyer, "You don't have enough money!", 255, 50, 50);
                return;
            }

            ComfyEconomy.DBManager.SaveAccount(buyer.Name, buyerAccount.Balance - Price);
            buyer.GiveItem(ItemID, Amount);

            Utils.SendFloatingMsg(buyer, $"Bought {Amount} {TShock.Utils.GetItemById(ItemID).Name}", 50, 255, 50);

            LogManager.Log("Server-Buy-Sign", buyerAccount.AccountName, $"Bought {Amount} {TShock.Utils.GetItemById(ItemID).Name}");
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
    }
}