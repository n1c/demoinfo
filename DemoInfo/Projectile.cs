using System;

namespace DemoInfo
{
    public class Projectile
    {
        public string ServerClassName { get; set; }
        public Player Owner { get; set; }
        public int OwnerID { get; set; }
        public Vector Position { get; set; }
        public Vector Velocity { get; set; }
    }
}
