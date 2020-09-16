using DemoInfo.DP;
using System.Collections.Generic;

namespace DemoInfo
{
    public class Player
    {
        public string Name { get; set; }
        public long SteamID { get; set; }
        public string SteamID32 { get; set; }
        public Vector Position { get; set; }
        public int EntityID { get; set; }
        public int HP { get; set; }
        public int Armor { get; set; }
        public Vector LastAlivePosition { get; set; }
        public Vector Velocity { get; set; }
        public float ViewDirectionX { get; set; }
        public float ViewDirectionY { get; set; }
        public float FlashDuration { get; set; }
        public int Money { get; set; }
        public int CurrentEquipmentValue { get; set; }
        public int FreezetimeEndEquipmentValue { get; set; }
        public int RoundStartEquipmentValue { get; set; }

        /// <summary>
        /// Used to avoid triggering player buy kevlar event when he actually bought an assaultsuit
        /// </summary>
        public int LastItemBoughtValue { get; set; }

        public bool IsDucking { get; set; }
        public bool IsInBuyZone { get; set; }
        public bool IsScoped { get; set; }
        internal Entity Entity;
        public bool Disconnected { get; set; }
        internal int ActiveWeaponID;
        public Equipment ActiveWeapon => ActiveWeaponID == DemoParser.INDEX_MASK ? null : rawWeapons[ActiveWeaponID];
        internal Dictionary<int, Equipment> rawWeapons = new Dictionary<int, Equipment>();
        public IEnumerable<Equipment> Weapons => rawWeapons.Values;
        public bool IsAlive => HP > 0;
        public Team Team { get; set; }
        public bool HasDefuseKit { get; set; }
        public bool HasHelmet { get; set; }
        internal int TeamID;
        internal int[] AmmoLeft = new int[32];
        public AdditionalPlayerInformation AdditionalInformations { get; internal set; }

        /// <summary>
        /// Because data updates are inconsistent, we track player's weapons dropped
        /// during each tick and raise events at the end of it.
        /// </summary>
        internal Queue<Equipment> DroppedWeapons = new Queue<Equipment>();

        /// <summary>
        /// Same as DropppedWeapons but for weapons picked during the tick.
        /// </summary>
        internal Queue<Equipment> PickedWeapons = new Queue<Equipment>();

        public Player()
        {
            Velocity = new Vector();
            LastAlivePosition = new Vector();
        }

        /// <summary>
        /// Copy this instance for multi-threading use.
        /// </summary>
        public Player Copy()
        {
            Player me = new Player
            {
                EntityID = -1, // this should not be copied
                Entity = null,
                Name = Name,
                SteamID = SteamID,
                HP = HP,
                Armor = Armor,
                ViewDirectionX = ViewDirectionX,
                ViewDirectionY = ViewDirectionY,
                Disconnected = Disconnected,
                FlashDuration = FlashDuration,
                Team = Team,
                ActiveWeaponID = ActiveWeaponID,
                rawWeapons = new Dictionary<int, Equipment>(rawWeapons),
                PickedWeapons = new Queue<Equipment>(PickedWeapons),
                DroppedWeapons = new Queue<Equipment>(DroppedWeapons),
                HasDefuseKit = HasDefuseKit,
                HasHelmet = HasHelmet
            };

            if (Position != null)
            {
                // Vector is a class, not a struct so copy for thread-safety
                me.Position = Position.Copy();
            }

            if (LastAlivePosition != null)
            {
                me.LastAlivePosition = LastAlivePosition.Copy();
            }

            if (Velocity != null)
            {
                me.Velocity = Velocity.Copy();
            }

            return me;
        }
    }
}
