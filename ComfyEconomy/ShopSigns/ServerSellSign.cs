using TShockAPI;
using Terraria;
using Terraria.Localization;
using ComfyEconomy.Database;

namespace ComfyEconomy
{
    public class ServerSellSign
    {
        public int ItemID { get; set; }
        public int Amount { get; set; }
        public int Price { get; set; }

        public ServerSellSign(int itemID, int amount, int price)
        {
            ItemID = itemID;
            Amount = amount;
            Price = price;
        }
        public void Sell(TSPlayer seller)
        {
            if (seller.SelectedItem.netID != ItemID)
            {
                Utils.SendFloatingMsg(seller, "Item doesn't match!", 255, 50, 50);
                return;
            }
            if (seller.SelectedItem.stack < Amount)
            {
                Utils.SendFloatingMsg(seller, "You don't have enough!", 255, 50, 50);
                return;
            }

            foreach (Item item in Main.item)
            {
                if (item != null && item.active && item.stack == seller.SelectedItem.stack &&
                    item.netID == ItemID && item.prefix == seller.SelectedItem.prefix &&
                    item.position.WithinRange(seller.TPlayer.position, 16 * 40))
                {
                    Utils.SendFloatingMsg(seller, "You dropped the item!", 255, 50, 50);
                    return;
                }
            }

            Account sellerAccount = ComfyEconomy.DBManager.GetAccount(seller.Name);

            seller.SelectedItem.stack -= Amount;

            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(seller.SelectedItem.Name), seller.Index, seller.TPlayer.selectedItem);
            NetMessage.SendData((int)PacketTypes.PlayerSlot, seller.Index, -1, NetworkText.FromLiteral(seller.SelectedItem.Name), seller.Index, seller.TPlayer.selectedItem);

            ComfyEconomy.DBManager.SaveAccount(seller.Name, sellerAccount.Balance + Price);

            Utils.SendFloatingMsg(seller, $"Sold {Amount} {TShock.Utils.GetItemById(ItemID).Name}", 50, 255, 50);

            LogManager.Log("Server-Sell-Sign", sellerAccount.AccountName, $"Sold {Amount} {TShock.Utils.GetItemById(ItemID).Name}");
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
    }
}