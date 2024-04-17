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

        public static bool IsPlayerInMine(TSPlayer player, Mine mine) {
            return player.TileX <= mine.PosX2 && player.TileX + 1 >= mine.PosX1 && player.TileY + 2 >= mine.PosY1 && player.TileY <= mine.PosY2;
        }
    }
}