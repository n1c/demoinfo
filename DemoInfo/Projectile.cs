using System;

namespace DemoInfo
{
    internal class Projectile
    {
        public string ServerClassName { get; set; }
        public Player Owner { get; set; }
        public int OwnerID { get; set; }
        public Vector Position { get; set; }
    }
}
