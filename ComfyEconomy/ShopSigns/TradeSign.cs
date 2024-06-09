using TShockAPI;
using Terraria;
using Terraria.Localization;

namespace ComfyEconomy
{
    public class TradeSign
    {
        public int ItemID { get; set; }
        public int Amount { get; set; }
        public int ReqItemID { get; set; }
        public int ReqAmount { get; set; }

        public TradeSign(int itemID, int amount, int reqItemID, int reqAmount)
        {
            ItemID = itemID;
            Amount = amount;
            ReqItemID = reqItemID;
            ReqAmount = reqAmount;
        }

        public void Trade(TSPlayer buyer)
        {
            if (Utils.IsSignInteractionInCooldown(buyer.Index)) return;

            if (buyer.SelectedItem.netID != ReqItemID)
            {
                Utils.SendFloatingMsg(buyer, "Item doesn't match!", 255, 50, 50);
                return;
            }
            if (buyer.SelectedItem.stack < ReqAmount)
            {
                Utils.SendFloatingMsg(buyer, "You don't have enough!", 255, 50, 50);
                return;
            }

            foreach (Item item in Main.item)
            {
                if (item != null && item.active && item.stack == buyer.SelectedItem.stack &&
                    item.netID == ReqItemID && item.prefix == buyer.SelectedItem.prefix &&
                    item.position.WithinRange(buyer.TPlayer.position, 16 * 40))
                {
                    Utils.SendFloatingMsg(buyer, "You dropped the item!", 255, 50, 50);
                    return;
                }
            }

            buyer.SelectedItem.stack -= ReqAmount;
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(buyer.SelectedItem.Name), buyer.Index, buyer.TPlayer.selectedItem);
            NetMessage.SendData((int)PacketTypes.PlayerSlot, buyer.Index, -1, NetworkText.FromLiteral(buyer.SelectedItem.Name), buyer.Index, buyer.TPlayer.selectedItem);
            buyer.GiveItem(ItemID, Amount);

            Utils.SendFloatingMsg(buyer, $"Bought {Amount} {TShock.Utils.GetItemById(ItemID).Name}", 50, 255, 50);

            LogManager.Log("Server-Trade-Sign", buyer.Name, $"Traded {ReqAmount} {TShock.Utils.GetItemById(ReqItemID).Name} in for {Amount} {TShock.Utils.GetItemById(ItemID).Name}");
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
    }
}