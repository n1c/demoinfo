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
        public int CellWidth => 1 << CellBits;
        public int CellX { get; set; }
        public int CellY { get; set; }
        public int CellZ { get; set; }

        public Vector Origin { get; set; }
        public Vector Velocity { get; set; }

        public Vector Position => new Vector(CellXOffset, CellYOffset, CellZOffset);

        private float CellXOffset => (CellX * CellWidth - MAX_COORD_INT) + Origin.X;
        private float CellYOffset => (CellY * CellWidth - MAX_COORD_INT) + Origin.Y;
        private float CellZOffset => (CellZ * CellWidth - MAX_COORD_INT) + Origin.Z;
    }
}
