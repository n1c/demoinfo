namespace DemoInfo
{
    public enum Team
    {
        Spectate = 1,
        Terrorist = 2,
        CounterTerrorist = 3,
    }

    public enum EquipmentElement
    {
        Unknown = 0,

        // Pistols
        P2000 = 1,
        Glock = 2,
        P250 = 3,
        Deagle = 4,
        FiveSeven = 5,
        DualBarettas = 6,
        Tec9 = 7,
        CZ = 8,
        USP = 9,
        Revolver = 10,

        // SMGs
        MP7 = 101,
        MP9 = 102,
        Bizon = 103,
        Mac10 = 104,
        UMP = 105,
        P90 = 106,
        MP5SD = 107,

        // Heavy
        SawedOff = 201,
        Nova = 202,
        Swag7 = 203,
        XM1014 = 204,
        M249 = 205,
        Negev = 206,

        // Rifle
        Gallil = 301,
        Famas = 302,
        AK47 = 303,
        M4A4 = 304,
        M4A1 = 305,
        Scout = 306,
        SG556 = 307,
        AUG = 308,
        AWP = 309,
        Scar20 = 310,
        G3SG1 = 311,

        // Equipment
        Zeus = 401,
        Kevlar = 402,
        Helmet = 403,
        Bomb = 404,
        Knife = 405,
        DefuseKit = 406,
        World = 407,

        // Grenades
        Decoy = 501,
        Molotov = 502,
        Incendiary = 503,
        Flash = 504,
        Smoke = 505,
        HE = 506
    }

    public enum EquipmentClass
    {
        Unknown = 0,
        Pistol = 1,
        SMG = 2,
        Heavy = 3,
        Rifle = 4,
        Equipment = 5,
        Grenade = 6,
    }

    /// <summary>
    /// The demo-commands as given by Valve.
    /// </summary>
    internal enum DemoCommand
    {
        /// <summary>
        /// it's a startup message, process as fast as possible
        /// </summary>
        Signon = 1,
        /// <summary>
        // it's a normal network packet that we stored off
        /// </summary>
        Packet,

        /// <summary>
        /// sync client clock to demo tick
        /// </summary>
        Synctick,

        /// <summary>
        /// Console Command
        /// </summary>
        ConsoleCommand,

        /// <summary>
        /// user input command
        /// </summary>
        UserCommand,

        /// <summary>
        ///  network data tables
        /// </summary>
        DataTables,

        /// <summary>
        /// end of time.
        /// </summary>
        Stop,

        /// <summary>
        /// a blob of binary data understood by a callback function
        /// </summary>
        CustomData,

        StringTables,

        /// <summary>
        /// Last Command
        /// </summary>
        LastCommand = StringTables,

        /// <summary>
        /// First Command
        /// </summary>
        FirstCommand = Signon
    };

    public enum RoundEndReason
    {
        /// <summary>
        /// Target Successfully Bombed!
        /// </summary>
        TargetBombed = 1,
        /// <summary>
        /// The VIP has escaped.
        /// </summary>
        VIPEscaped,
        /// <summary>
        /// VIP has been assassinated
        /// </summary>
        VIPKilled,
        /// <summary>
        /// The terrorists have escaped
        /// </summary>
        TerroristsEscaped,

        /// <summary>
        /// The CTs have prevented most of the terrorists from escaping!
        /// </summary>
        CTStoppedEscape,
        /// <summary>
        /// Escaping terrorists have all been neutralized
        /// </summary>
        TerroristsStopped,
        /// <summary>
        /// The bomb has been defused!
        /// </summary>
        BombDefused,
        /// <summary>
        /// Counter-Terrorists Win!
        /// </summary>
        CTWin,
        /// <summary>
        /// Terrorists Win!
        /// </summary>
        TerroristWin,
        /// <summary>
        /// Round Draw!
        /// </summary>
        Draw,
        /// <summary>
        /// All Hostages have been rescued
        /// </summary>
        HostagesRescued,
        /// <summary>
        /// Target has been saved!
        /// </summary>
        TargetSaved,
        /// <summary>
        /// Hostages have not been rescued!
        /// </summary>
        HostagesNotRescued,
        /// <summary>
        /// Terrorists have not escaped!
        /// </summary>
        TerroristsNotEscaped,
        /// <summary>
        /// VIP has not escaped!
        /// </summary>
        VIPNotEscaped,
        /// <summary>
        /// Game Commencing!
        /// </summary>
        GameStart,
        /// <summary>
        /// Terrorists Surrender
        /// </summary>
        TerroristsSurrender,
        /// <summary>
        /// CTs Surrender
        /// </summary>
        CTSurrender
    };

    public enum RoundMVPReason
    {
        MostEliminations = 1,
        BombPlanted,
        BombDefused
    };

    public enum Hitgroup
    {
        Generic = 0,
        Head = 1,
        Chest = 2,
        Stomach = 3,
        LeftArm = 4,
        RightArm = 5,
        LeftLeg = 6,
        RightLeg = 7,
        Gear = 10,
    };


}
