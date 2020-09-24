using System;

namespace DemoInfo
{
    public class Projectile
    {
        private const int MAX_COORD_INT = 16384;

        public string ServerClassName { get; set; }
        public Player Owner { get; set; }
        public int OwnerID { get; set; }

        public int CellBits { get; set; }
        public int CellX { get; set; }
        public int CellY { get; set; }
        public int CellZ { get; set; }
        public Vector VecOrigin { get; set; }

        public Vector Position => new Vector(CellXOffset, CellYOffset, CellYOffset);

        private float CellXOffset => (CellX * CellBits - MAX_COORD_INT) + VecOrigin.X;
        private float CellYOffset => (CellY * CellBits - MAX_COORD_INT) + VecOrigin.Y;
        private float CellZOffset => (CellZ * CellBits - MAX_COORD_INT) + VecOrigin.Z;
    }
}
