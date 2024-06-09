using TShockAPI;
using Terraria;
using ComfyEconomy.Database;

namespace ComfyEconomy
{
    public class BuySign
    {
        public int ItemID { get; set; }
        public int Amount { get; set; }
        public int Price { get; set; }
        public string Owner { get; set; }

        public BuySign(int itemID, int amount, int price, string owner)
        {
            ItemID = itemID;
            Amount = amount;
            Price = price;
            Owner = owner;
        }
        public void Buy(TSPlayer buyer, int chestID)
        {
            if (Utils.IsSignInteractionInCooldown(buyer.Index)) return;
            
            int stock = 0;

            foreach (Item item in Main.chest[chestID].item)
            {
                if (item.netID == ItemID)
                {
                    stock += item.stack;
                }
            }

            if (stock < Amount)
            {
                Utils.SendFloatingMsg(buyer, "Ran out of stock!", 255, 50, 50);
                return;
            }

            Account sellerAccount;
            Account buyerAccount = ComfyEconomy.DBManager.GetAccount(buyer.Name);
            try
            {
                sellerAccount = ComfyEconomy.DBManager.GetAccount(Owner);
            }
            catch (NullReferenceException)
            {
                Utils.SendFloatingMsg(buyer, "Couldn't find the owner!", 255, 50, 50);
                return;
            }

            if (buyerAccount.Balance < Price)
            {
                Utils.SendFloatingMsg(buyer, "You don't have enough money!", 255, 50, 50);
                return;
            }

            ComfyEconomy.DBManager.SaveAccount(buyer.Name, buyerAccount.Balance - Price);
            buyer.GiveItem(ItemID, Amount);
            Utils.DeleteItemsFromChest(chestID, ItemID, Amount);
            ComfyEconomy.DBManager.SaveAccount(sellerAccount.AccountName, sellerAccount.Balance + Price);

            Utils.SendFloatingMsg(buyer, $"Bought {Amount} {TShock.Utils.GetItemById(ItemID).Name}", 50, 255, 50);

            LogManager.Log("Buy-Sign", buyerAccount.AccountName, $"Bought {Amount} {TShock.Utils.GetItemById(ItemID).Name} from {Owner}");
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
    }
}