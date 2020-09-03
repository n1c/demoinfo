using System;
using System.IO;

namespace DemoInfo
{
    /// <summary>
    /// A Demo header.
    /// </summary>
    // https://developer.valvesoftware.com/wiki/DEM_Format
    public class DemoHeader
    {
        private const int MAX_OSPATH = 260;

        public string Filestamp { get; private set; }       // Should be HL2DEMO
        public int DemoProtocol { get; private set; }       //
        public int NetworkProtocol { get; private set; }  //
        public string ServerName { get; private set; }    // Name of server
        public string ClientName { get; private set; }    // Name of client who recorded the game
        public string MapName { get; private set; }	      // Name of map
        public string GameDirectory { get; private set; } // Name of game directory (com_gamedir)
        public float PlaybackTime { get; private set; }	  // Time of track
        public int PlaybackTicks { get; private set; }    // # of ticks in track
        public int PlaybackFrames { get; private set; }   // # of frames in track
        public int SignonLength { get; private set; }     // length of sigondata in bytes

        public static DemoHeader ParseFrom(IBitStream reader)
        {
            return new DemoHeader()
            {
                Filestamp = reader.ReadCString(8),
                DemoProtocol = reader.ReadSignedInt(32),
                NetworkProtocol = reader.ReadSignedInt(32),
                ServerName = reader.ReadCString(MAX_OSPATH),

                ClientName = reader.ReadCString(MAX_OSPATH),
                MapName = reader.ReadCString(MAX_OSPATH),
                GameDirectory = reader.ReadCString(MAX_OSPATH),
                PlaybackTime = reader.ReadFloat(),

                PlaybackTicks = reader.ReadSignedInt(32),
                PlaybackFrames = reader.ReadSignedInt(32),
                SignonLength = reader.ReadSignedInt(32),
            };
        }
    }

    /// <summary>
    /// And Source-Engine Vector.
    /// </summary>
    public class Vector
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public double Angle2D => Math.Atan2(Y, X);

        public double Absolute => Math.Sqrt(AbsoluteSquared);

        public double AbsoluteSquared => (X * X) + (Y * Y) + (Z * Z);

        public static Vector Parse(IBitStream reader)
        {
            return new Vector
            {
                X = reader.ReadFloat(),
                Y = reader.ReadFloat(),
                Z = reader.ReadFloat(),
            };
        }

        public Vector()
        {

        }

        public Vector(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Copy this instance. So if you want to permanently store the position of a player at a point in time,
        /// COPY it.
        /// </summary>
        public Vector Copy()
        {
            return new Vector(X, Y, Z);
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector() { X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z };
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector() { X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z };
        }

        public override string ToString()
        {
            return "{X: " + X + ", Y: " + Y + ", Z: " + Z + " }";
        }
    }

    /// <summary>
    /// And Angle in the Source-Engine. Looks pretty much like a vector.
    /// </summary>
    internal class QAngle
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public static QAngle Parse(IBitStream reader)
        {
            return new QAngle
            {
                X = reader.ReadFloat(),
                Y = reader.ReadFloat(),
                Z = reader.ReadFloat(),
            };
        }
    }


    /// <summary>
    /// A split.
    /// </summary>
    internal class Split
    {
        // private const int FDEMO_NORMAL = 0;
        private const int FDEMO_USE_ORIGIN2 = 1;
        private const int FDEMO_USE_ANGLES2 = 2;
        // private const int FDEMO_NOINTERP = 4;

        public int Flags { get; private set; }
        private Vector ReaderViewOrigin { get; set; }
        private QAngle ReaderViewAngles { get; set; }
        private QAngle ReaderLocalViewAngles { get; set; }

        private Vector ReaderViewOrigin2 { get; set; }
        private QAngle ReaderViewAngles2 { get; set; }
        private QAngle ReaderLocalViewAngles2 { get; set; }

        public Vector ViewOrigin => (Flags & FDEMO_USE_ORIGIN2) != 0 ? ReaderViewOrigin2 : ReaderViewOrigin;
        public QAngle ViewAngles => (Flags & FDEMO_USE_ANGLES2) != 0 ? ReaderViewAngles2 : ReaderViewAngles;
        public QAngle LocalViewAngles => (Flags & FDEMO_USE_ANGLES2) != 0 ? ReaderLocalViewAngles2 : ReaderLocalViewAngles;

        public static Split Parse(IBitStream reader)
        {
            return new Split
            {
                Flags = reader.ReadSignedInt(32),
                ReaderViewOrigin = Vector.Parse(reader),
                ReaderViewAngles = QAngle.Parse(reader),
                ReaderLocalViewAngles = QAngle.Parse(reader),

                ReaderViewOrigin2 = Vector.Parse(reader),
                ReaderViewAngles2 = QAngle.Parse(reader),
                ReaderLocalViewAngles2 = QAngle.Parse(reader),
            };
        }
    }

    internal class CommandInfo
    {
        public Split[] U { get; private set; }

        public static CommandInfo Parse(IBitStream reader)
        {
            return new CommandInfo
            {
                U = new Split[2] { Split.Parse(reader), Split.Parse(reader) }
            };
        }
    }

    /// <summary>
    /// A playerinfo, based on playerinfo_t by Volvo.
    /// </summary>
    public class PlayerInfo
    {

        /// version for future compatibility
        public long Version { get; set; }

        // network xuid
        public long XUID { get; set; }

        // scoreboard information
        public string Name { get; set; } //MAX_PLAYER_NAME_LENGTH=128

        // local server user ID, unique while server is running
        public int UserID { get; set; }

        // global unique player identifer
        public string GUID { get; set; } //33bytes

        // friends identification number
        public int FriendsID { get; set; }
        // friends name
        public string FriendsName { get; set; } //128

        // true, if player is a bot controlled by game.dll
        public bool IsFakePlayer { get; set; }

        // true if player is the HLTV proxy
        public bool IsHLTV { get; set; }

        // custom files CRC for this player
        public int CustomFiles0 { get; set; }
        public int CustomFiles1 { get; set; }
        public int CustomFiles2 { get; set; }
        public int CustomFiles3 { get; set; }

        // this counter increases each time the server downloaded a new file
        private byte FilesDownloaded { get; set; }

        internal PlayerInfo()
        {
        }

        internal PlayerInfo(BinaryReader reader)
        {
            Version = reader.ReadInt64SwapEndian();
            XUID = reader.ReadInt64SwapEndian();
            Name = reader.ReadCString(128);
            UserID = reader.ReadInt32SwapEndian();
            GUID = reader.ReadCString(33);
            FriendsID = reader.ReadInt32SwapEndian();
            FriendsName = reader.ReadCString(128);

            IsFakePlayer = reader.ReadBoolean();
            IsHLTV = reader.ReadBoolean();

            CustomFiles0 = reader.ReadInt32();
            CustomFiles1 = reader.ReadInt32();
            CustomFiles2 = reader.ReadInt32();
            CustomFiles3 = reader.ReadInt32();

            FilesDownloaded = reader.ReadByte();
        }

        public static PlayerInfo ParseFrom(BinaryReader reader)
        {
            return new PlayerInfo(reader);
        }

        public static int SizeOf => 8 + 8 + 128 + 4 + 3 + 4 + 1 + 1 + (4 * 8) + 1;
    }


    /// <summary>
    /// This contains information about Collideables (specific edicts), mostly used for bombsites.
    /// </summary>
    internal class BoundingBoxInformation
    {
        public int Index { get; private set; }
        public Vector Min { get; set; }
        public Vector Max { get; set; }

        public BoundingBoxInformation(int index)
        {
            Index = index;
        }

        /// <summary>
        /// Checks wheter a point lies within the BoundingBox.
        /// </summary>
        /// <param name="point">The point to check</param>
        public bool Contains(Vector point)
        {
            return point.X >= Min.X && point.X <= Max.X &&
                point.Y >= Min.Y && point.Y <= Max.Y &&
                point.Z >= Min.Z && point.Z <= Max.Z;
        }
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
