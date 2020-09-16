using System.Diagnostics;
using System.Linq;

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
        public Player PrevOwner { get; set; } // Same as LastOwner?
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
        private const string ITEM_PREFIX = "item_";

        /// <summary>
        /// Map item string coming from demos events with equipment.
        /// Used for special equipments such as kevlar and weapons that have special name such as molotov.
        /// Others weapons detection is based on their item index definition.
        /// </summary>
        /// <param name="OriginalString"></param>
        /// <returns>EquipmentElement</returns>
        public static EquipmentElement MapEquipment(string OriginalString)
        {
            EquipmentElement weapon = EquipmentElement.Unknown;

            if (!OriginalString.StartsWith(ITEM_PREFIX) && !OriginalString.StartsWith(WEAPON_PREFIX))
            {
                OriginalString = WEAPON_PREFIX + OriginalString;
            }

            EquipmentMapping equipment = Equipments.FirstOrDefault(e => e.OriginalName == OriginalString);
            if (equipment.ItemIndex == 0)
            {
                switch (OriginalString)
                {
                    case "item_kevlar":
                    case "item_vest":
                        weapon = EquipmentElement.Kevlar;
                        break;
                    case "item_assaultsuit":
                    case "item_vesthelm":
                        weapon = EquipmentElement.Helmet;
                        break;
                    case "item_defuser":
                        weapon = EquipmentElement.DefuseKit;
                        break;
                    case "weapon_world":
                    case "weapon_worldspawn":
                        weapon = EquipmentElement.World;
                        break;
                    case "weapon_inferno":
                        weapon = EquipmentElement.Incendiary;
                        break;
                    case "weapon_molotov_projectile":
                    case "weapon_molotovgrenade":
                        weapon = EquipmentElement.Molotov;
                        break;
                    default:
                        Trace.WriteLine("Unknown weapon. " + OriginalString, "Equipment.MapEquipment()");
                        break;
                }
            }
            else
            {
                weapon = equipment.Element;
            }

            return weapon;
        }

        /// <summary>
        /// Mapping between item index definition and EquipmentElement.
        /// Item indexes are located in the game file /csgo/scripts/items_game.txt
        /// </summary>
        public static EquipmentMapping[] Equipments =
        {
            new EquipmentMapping
            {
                ItemIndex = 1,
                OriginalName = "weapon_deagle",
                Element = EquipmentElement.Deagle,
            },
            new EquipmentMapping
            {
                ItemIndex = 2,
                OriginalName = "weapon_elite",
                Element = EquipmentElement.DualBarettas,
            },
            new EquipmentMapping
            {
                ItemIndex = 3,
                OriginalName = "weapon_fiveseven",
                Element = EquipmentElement.FiveSeven,
            },
            new EquipmentMapping
            {
                ItemIndex = 4,
                OriginalName = "weapon_glock",
                Element = EquipmentElement.Glock,
            },
            new EquipmentMapping
            {
                ItemIndex = 7,
                OriginalName = "weapon_ak47",
                Element = EquipmentElement.AK47,
            },
            new EquipmentMapping
            {
                ItemIndex = 8,
                OriginalName = "weapon_aug",
                Element = EquipmentElement.AUG,
            },
            new EquipmentMapping
            {
                ItemIndex = 9,
                OriginalName = "weapon_awp",
                Element = EquipmentElement.AWP,
            },
            new EquipmentMapping
            {
                ItemIndex = 10,
                OriginalName = "weapon_famas",
                Element = EquipmentElement.Famas,
            },
            new EquipmentMapping
            {
                ItemIndex = 11,
                OriginalName = "weapon_g3sg1",
                Element = EquipmentElement.G3SG1,
            },
            new EquipmentMapping
            {
                ItemIndex = 13,
                OriginalName = "weapon_galilar",
                Element = EquipmentElement.Gallil,
            },
            new EquipmentMapping
            {
                ItemIndex = 14,
                OriginalName = "weapon_m249",
                Element = EquipmentElement.M249,
            },
            new EquipmentMapping
            {
                ItemIndex = 16,
                OriginalName = "weapon_m4a1",
                Element = EquipmentElement.M4A4,
            },
            new EquipmentMapping
            {
                ItemIndex = 17,
                OriginalName = "weapon_mac10",
                Element = EquipmentElement.Mac10,
            },
            new EquipmentMapping
            {
                ItemIndex = 19,
                OriginalName = "weapon_p90",
                Element = EquipmentElement.P90,
            },
            new EquipmentMapping
            {
                ItemIndex = 23,
                OriginalName = "weapon_mp5sd",
                Element = EquipmentElement.MP5SD,
            },
            new EquipmentMapping
            {
                ItemIndex = 24,
                OriginalName = "weapon_ump45",
                Element = EquipmentElement.UMP,
            },
            new EquipmentMapping
            {
                ItemIndex = 25,
                OriginalName = "weapon_xm1014",
                Element = EquipmentElement.XM1014,
            },
            new EquipmentMapping
            {
                ItemIndex = 26,
                OriginalName = "weapon_bizon",
                Element = EquipmentElement.Bizon,
            },
            new EquipmentMapping
            {
                ItemIndex = 27,
                OriginalName = "weapon_mag7",
                Element = EquipmentElement.Swag7,
            },
            new EquipmentMapping
            {
                ItemIndex = 28,
                OriginalName = "weapon_negev",
                Element = EquipmentElement.Negev,
            },
            new EquipmentMapping
            {
                ItemIndex = 29,
                OriginalName = "weapon_sawedoff",
                Element = EquipmentElement.SawedOff,
            },
            new EquipmentMapping
            {
                ItemIndex = 30,
                OriginalName = "weapon_tec9",
                Element = EquipmentElement.Tec9,
            },
            new EquipmentMapping
            {
                ItemIndex = 31,
                OriginalName = "weapon_taser",
                Element = EquipmentElement.Zeus,
            },
            new EquipmentMapping
            {
                ItemIndex = 32,
                OriginalName = "weapon_hkp2000",
                Element = EquipmentElement.P2000,
            },
            new EquipmentMapping
            {
                ItemIndex = 33,
                OriginalName = "weapon_mp7",
                Element = EquipmentElement.MP7,
            },
            new EquipmentMapping
            {
                ItemIndex = 34,
                OriginalName = "weapon_mp9",
                Element = EquipmentElement.MP9,
            },
            new EquipmentMapping
            {
                ItemIndex = 35,
                OriginalName = "weapon_nova",
                Element = EquipmentElement.Nova,
            },
            new EquipmentMapping
            {
                ItemIndex = 36,
                OriginalName = "weapon_p250",
                Element = EquipmentElement.P250,
            },
            new EquipmentMapping
            {
                ItemIndex = 38,
                OriginalName = "weapon_scar20",
                Element = EquipmentElement.Scar20,
            },
            new EquipmentMapping
            {
                ItemIndex = 39,
                OriginalName = "weapon_sg556",
                Element = EquipmentElement.SG556,
            },
            new EquipmentMapping
            {
                ItemIndex = 40,
                OriginalName = "weapon_ssg08",
                Element = EquipmentElement.Scout,
            },
            new EquipmentMapping
            {
                ItemIndex = 41,
                OriginalName = "weapon_knifegg",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 42,
                OriginalName = "weapon_knife",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 43,
                OriginalName = "weapon_flashbang",
                Element = EquipmentElement.Flash,
            },
            new EquipmentMapping
            {
                ItemIndex = 44,
                OriginalName = "weapon_hegrenade",
                Element = EquipmentElement.HE,
            },
            new EquipmentMapping
            {
                ItemIndex = 45,
                OriginalName = "weapon_smokegrenade",
                Element = EquipmentElement.Smoke,
            },
            new EquipmentMapping
            {
                ItemIndex = 46,
                OriginalName = "weapon_molotov",
                Element = EquipmentElement.Molotov,
            },
            new EquipmentMapping
            {
                ItemIndex = 47,
                OriginalName = "weapon_decoy",
                Element = EquipmentElement.Decoy,
            },
            new EquipmentMapping
            {
                ItemIndex = 48,
                OriginalName = "weapon_incgrenade",
                Element = EquipmentElement.Incendiary,
            },
            new EquipmentMapping
            {
                ItemIndex = 49,
                OriginalName = "weapon_c4",
                Element = EquipmentElement.Bomb,
            },
            new EquipmentMapping
            {
                ItemIndex = 50,
                OriginalName = "item_kevlar",
                Element = EquipmentElement.Kevlar,
            },
            new EquipmentMapping
            {
                ItemIndex = 51,
                OriginalName = "item_assaultsuit",
                Element = EquipmentElement.Helmet,
            },
            new EquipmentMapping
            {
                ItemIndex = 55,
                OriginalName = "item_defuser",
                Element = EquipmentElement.DefuseKit,
            },
            new EquipmentMapping
            {
                ItemIndex = 59,
                OriginalName = "weapon_knife_t",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 60,
                OriginalName = "weapon_m4a1_silencer",
                Element = EquipmentElement.M4A1,
            },
            new EquipmentMapping
            {
                ItemIndex = 61,
                OriginalName = "weapon_usp_silencer",
                Element = EquipmentElement.USP,
            },
            new EquipmentMapping
            {
                ItemIndex = 63,
                OriginalName = "weapon_cz75a",
                Element = EquipmentElement.CZ,
            },
            new EquipmentMapping
            {
                ItemIndex = 64,
                OriginalName = "weapon_revolver",
                Element = EquipmentElement.Revolver,
            },
            new EquipmentMapping
            {
                ItemIndex = 80,
                OriginalName = "weapon_knife_ghost",
                Element = EquipmentElement.Knife,
            },
             new EquipmentMapping
            {
                ItemIndex = 83,
                OriginalName = "weapon_frag_grenade",
                Element = EquipmentElement.HE, // Not sure
			},
            new EquipmentMapping
            {
                ItemIndex = 500,
                OriginalName = "weapon_bayonet",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 503,
                OriginalName = "weapon_knife_css",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 505,
                OriginalName = "weapon_knife_flip",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 506,
                OriginalName = "weapon_knife_gut",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 507,
                OriginalName = "weapon_knife_karambit",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 508,
                OriginalName = "weapon_knife_m9_bayonet",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 509,
                OriginalName = "weapon_knife_tactical",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 512,
                OriginalName = "weapon_knife_falchion",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 514,
                OriginalName = "weapon_knife_survival_bowie",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 515,
                OriginalName = "weapon_knife_butterfly",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 516,
                OriginalName = "weapon_knife_push",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 517,
                OriginalName = "weapon_knife_cord",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 518,
                OriginalName = "weapon_knife_canis",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 519,
                OriginalName = "weapon_knife_ursus",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 520,
                OriginalName = "weapon_knife_gypsy_jackknife",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 521,
                OriginalName = "weapon_knife_outdoor",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 522,
                OriginalName = "weapon_knife_stiletto",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 523,
                OriginalName = "weapon_knife_widowmaker",
                Element = EquipmentElement.Knife,
            },
            new EquipmentMapping
            {
                ItemIndex = 525,
                OriginalName = "weapon_knife_skeleton",
                Element = EquipmentElement.Knife,
            },
        };
    }

    /// <summary>
    /// Struct to map Equipment with game item index and DemoInfo EquipmentElement API.
    /// </summary>
    public struct EquipmentMapping
    {
        /// <summary>
        /// Index defined in the game file csgo\scripts\items\items_game.txt
        /// </summary>
        public int ItemIndex;

        /// <summary>
        /// Equipment string name defined in the game file items_game.txt
        /// </summary>
        public string OriginalName;

        /// <summary>
        /// Mapping to EquipmentElement to not break current API
        /// </summary>
        public EquipmentElement Element;
    }
}
