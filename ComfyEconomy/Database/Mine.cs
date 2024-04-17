using Terraria;
using TShockAPI;

namespace ComfyEconomy.Database {
    public class Mine {

        public int MineID;
        public string Name;
        public int PosX1;
        public int PosY1;
        public int PosX2;
        public int PosY2;
        public int TileID;
        public int PaintID;

        public Mine(int mineID, string name, int posX1, int posY1, int posX2, int posY2, int tileId, int paintID) {
            MineID = mineID;
            Name = name;
            PosX1 = posX1;
            PosX2 = posX2;
            PosY1 = posY1;
            PosY2 = posY2;
            TileID = tileId;
            PaintID = paintID;
        }

        public bool Refill() {
            bool isRefilled = false;

            for (int i = PosX1; i <= PosX2; i++) {
                for (int j = PosY1; j <= PosY2; j++) {
                    if (Main.tile[i, j].type != TileID || Main.tile[i, j].color() != PaintID) {
                        WorldGen.PlaceTile(i, j, TileID, forced: true);
                        WorldGen.paintTile(i, j, (byte)PaintID);
                        isRefilled = true;
                    }
                }
            }
            
            if (isRefilled) {
                TSPlayer.All.SendTileRect((short)PosX1, (short)PosY1, (byte)(PosX2 - PosX1 + 1), (byte)(PosY2 - PosY1 + 1));
            }

            return isRefilled;
        }
        /*
        public static bool RefillMine(int mineId) {
            bool isRefilled = false;
            Mine mine = ComfyEconomy.DBManager.GetMine(mineId);
            mine.PosX2++;
            mine.PosY2++;

            for (int i = mine.PosX1; i < mine.PosX2; i++) {
                for (int j = mine.PosY1; j < mine.PosY2; j++) {
                    if (Main.tile[i, j].type != mine.TileID || Main.tile[i, j].color() != mine.PaintID) {
                        WorldGen.PlaceTile(i, j, mine.TileID, forced: true);
                        WorldGen.paintTile(i, j, (byte)mine.PaintID);
                        isRefilled = true;
                    }
                }
            }
            
            if (isRefilled) {
                TSPlayer.All.SendTileRect((short)mine.PosX1, (short)mine.PosY1, (byte)(mine.PosX2 - mine.PosX1 + 1), (byte)(mine.PosY2 - mine.PosY1 + 1));
            }

            return isRefilled;
        }*/
    }
}
