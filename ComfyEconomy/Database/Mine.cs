using IL.Terraria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComfyEconomy.Database {
    public class Mine {

        public int MineID;
        public int PosX1;
        public int PosY1;
        public int PosX2;
        public int PosY2;
        public int TileID;
        public int PaintID;
        public Mine(int mineID, int posX1, int posY1, int posX2, int posY2, int tileId, int paintID) {
            MineID = mineID;
            PosX1 = posX1;
            PosX2 = posX2;
            PosY1 = posY1;
            PosY2 = posY2;
            TileID = tileId;
            PaintID = paintID;
        }
    }
}
