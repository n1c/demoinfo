using System.Diagnostics;

namespace DemoInfo
{
    public class Equipment
    {
        internal int EntityID { get; set; }
        public EquipmentElement Weapon { get; set; }
        public EquipmentClass Class => (EquipmentClass)(((int)Weapon / 100) + 1);
        public string OriginalString { get; set; }
        public string SkinID { get; set; }
        public int AmmoInMagazine { get; set; }
        internal int AmmoType { get; set; }
        public Player Owner { get; set; }
        public Player LastOwner { get; set; }
        public int ReserveAmmo => (Owner != null && AmmoType != -1) ? Owner.AmmoLeft[AmmoType] : -1;

        internal Equipment()
        {
            Weapon = EquipmentElement.Unknown;
        }

        internal Equipment(string originalString)
        {
            OriginalString = originalString;
            Weapon = MapEquipment(originalString);
        }

        internal Equipment(string originalString, string skin)
        {
            OriginalString = originalString;
            Weapon = MapEquipment(originalString);
            SkinID = skin;
        }

        private const string WEAPON_PREFIX = "weapon_";

        public static EquipmentElement MapEquipment(string OriginalString)
        {
            EquipmentElement weapon = EquipmentElement.Unknown;

            OriginalString = OriginalString.StartsWith(WEAPON_PREFIX)
                ? OriginalString.Substring(WEAPON_PREFIX.Length)
                : OriginalString;

            if (OriginalString.Contains("knife") || OriginalString == "bayonet")
            {
                weapon = EquipmentElement.Knife;
            }

            if (weapon == EquipmentElement.Unknown)
            {
                switch (OriginalString)
                {
                    case "ak47":
                        weapon = EquipmentElement.AK47;
                        break;
                    case "aug":
                        weapon = EquipmentElement.AUG;
                        break;
                    case "awp":
                        weapon = EquipmentElement.AWP;
                        break;
                    case "bizon":
                        weapon = EquipmentElement.Bizon;
                        break;
                    case "c4":
                        weapon = EquipmentElement.Bomb;
                        break;
                    case "deagle":
                        weapon = EquipmentElement.Deagle;
                        break;
                    case "decoy":
                    case "decoygrenade":
                        weapon = EquipmentElement.Decoy;
                        break;
                    case "elite":
                        weapon = EquipmentElement.DualBarettas;
                        break;
                    case "famas":
                        weapon = EquipmentElement.Famas;
                        break;
                    case "fiveseven":
                        weapon = EquipmentElement.FiveSeven;
                        break;
                    case "flashbang":
                        weapon = EquipmentElement.Flash;
                        break;
                    case "g3sg1":
                        weapon = EquipmentElement.G3SG1;
                        break;
                    case "galil":
                    case "galilar":
                        weapon = EquipmentElement.Gallil;
                        break;
                    case "glock":
                        weapon = EquipmentElement.Glock;
                        break;
                    case "hegrenade":
                        weapon = EquipmentElement.HE;
                        break;
                    case "hkp2000":
                        weapon = EquipmentElement.P2000;
                        break;
                    case "incgrenade":
                    case "incendiarygrenade":
                        weapon = EquipmentElement.Incendiary;
                        break;
                    case "m249":
                        weapon = EquipmentElement.M249;
                        break;
                    case "m4a1":
                        weapon = EquipmentElement.M4A4;
                        break;
                    case "mac10":
                        weapon = EquipmentElement.Mac10;
                        break;
                    case "mag7":
                        weapon = EquipmentElement.Swag7;
                        break;
                    case "molotov":
                    case "molotovgrenade":
                    case "molotov_projectile":
                        weapon = EquipmentElement.Molotov;
                        break;
                    case "mp7":
                        weapon = EquipmentElement.MP7;
                        break;
                    case "mp9":
                        weapon = EquipmentElement.MP9;
                        break;
                    case "negev":
                        weapon = EquipmentElement.Negev;
                        break;
                    case "nova":
                        weapon = EquipmentElement.Nova;
                        break;
                    case "p250":
                        weapon = EquipmentElement.P250;
                        break;
                    case "p90":
                        weapon = EquipmentElement.P90;
                        break;
                    case "sawedoff":
                        weapon = EquipmentElement.SawedOff;
                        break;
                    case "scar20":
                        weapon = EquipmentElement.Scar20;
                        break;
                    case "sg556":
                        weapon = EquipmentElement.SG556;
                        break;
                    case "smokegrenade":
                        weapon = EquipmentElement.Smoke;
                        break;
                    case "ssg08":
                        weapon = EquipmentElement.Scout;
                        break;
                    case "taser":
                        weapon = EquipmentElement.Zeus;
                        break;
                    case "tec9":
                        weapon = EquipmentElement.Tec9;
                        break;
                    case "ump45":
                        weapon = EquipmentElement.UMP;
                        break;
                    case "xm1014":
                        weapon = EquipmentElement.XM1014;
                        break;
                    case "m4a1_silencer":
                    case "m4a1_silencer_off":
                        weapon = EquipmentElement.M4A1;
                        break;
                    case "cz75a":
                        weapon = EquipmentElement.CZ;
                        break;
                    case "usp":
                    case "usp_silencer":
                    case "usp_silencer_off":
                        weapon = EquipmentElement.USP;
                        break;
                    case "world":
                        weapon = EquipmentElement.World;
                        break;
                    case "inferno":
                        weapon = EquipmentElement.Incendiary;
                        break;
                    case "revolver":
                        weapon = EquipmentElement.Revolver;
                        break;
                    case "mp5sd":
                        weapon = EquipmentElement.MP5SD;
                        break;
                    case "scar17"://These crash the game when given via give weapon_[mp5navy|...], and cannot be purchased ingame.
                    case "sg550"://yet the server-classes are networked, so I need to resolve them.
                    case "mp5navy":
                    case "p228":
                    case "scout":
                    case "sg552":
                    case "tmp":
                        weapon = EquipmentElement.Unknown;
                        break;
                    default:
                        Trace.WriteLine("Unknown weapon. " + OriginalString, "Equipment.MapEquipment()");
                        break;
                }
            }

            return weapon;
        }
    }
}