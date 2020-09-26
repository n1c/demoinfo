using DemoInfo.DP;
using DemoInfo.DT;
using DemoInfo.ST;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DemoInfo
{
    public class DemoParser : IDisposable
    {
        private const int MAX_EDICT_BITS = 11;
        internal const int INDEX_MASK = (1 << MAX_EDICT_BITS) - 1;
        internal const int MAX_ENTITIES = 1 << MAX_EDICT_BITS;
        private const int MAXPLAYERS = 64;
        private const int MAXWEAPONS = 64;
        private bool MustParse = false;

        #region Events
        /// <summary>
        /// Raised once when the Header of the demo is parsed
        /// </summary>
        public event EventHandler<HeaderParsedEventArgs> HeaderParsed;

        /// <summary>
        /// Occurs when the match started, so when the "begin_new_match"-GameEvent is dropped.
        /// This usually right before the freezetime of the 1st round. Be careful, since the players
        /// usually still have warmup-money when this drops.
        /// </summary>
        public event EventHandler<MatchStartedEventArgs> MatchStarted;

        /// <summary>
        /// Occurs when the first round of a new match start "round_announce_match_start"
        /// </summary>
        public event EventHandler<RoundAnnounceMatchStartedEventArgs> RoundAnnounceMatchStarted;

        /// <summary>
        /// Occurs when round starts, on the round_start event of the demo. Usually the players haven't spawned yet, but have recieved the money for the next round.
        /// </summary>
        public event EventHandler<RoundStartedEventArgs> RoundStart;

        /// <summary>
        /// Occurs when round ends
        /// </summary>
        public event EventHandler<RoundEndedEventArgs> RoundEnd;

        /// <summary>
        /// Occurs at the end of the match, when the scoreboard is shown
        /// </summary>
        public event EventHandler<WinPanelMatchEventArgs> WinPanelMatch;

        /// <summary>
        /// Occurs when it's the last round of a match
        /// </summary>
        public event EventHandler<RoundFinalEventArgs> RoundFinal;

        /// <summary>
        /// Occurs at the half of a side
        /// </summary>
        public event EventHandler<LastRoundHalfEventArgs> LastRoundHalf;

        /// <summary>
        /// Occurs when round really ended
        /// </summary>
        public event EventHandler<RoundOfficiallyEndedEventArgs> RoundOfficiallyEnd;

        /// <summary>
        /// Occurs on round end with the MVP
        /// </summary>
        public event EventHandler<RoundMVPEventArgs> RoundMVP;

        /// <summary>
        /// Occurs when a player take control of a bot
        /// </summary>
        public event EventHandler<BotTakeOverEventArgs> BotTakeOver;

        /// <summary>
        /// Occurs when freezetime ended. Raised on "round_freeze_end"
        /// </summary>
        public event EventHandler<FreezetimeEndedEventArgs> FreezetimeEnded;

        /// <summary>
        /// Occurs on the end of every tick, after the gameevents were processed and the packet-entities updated
        /// </summary>
        public event EventHandler<TickDoneEventArgs> TickDone;

        /// <summary>
        /// This is raised when a player is killed. Not that the killer might be dead by the time is raised (e.g. nade-kills),
        /// also note that the killed player is still alive when this is killed
        /// </summary>
        public event EventHandler<PlayerKilledEventArgs> PlayerKilled;

        /// <summary>
        /// Occurs when a player select a team
        /// </summary>
        public event EventHandler<PlayerTeamEventArgs> PlayerTeam;

        /// <summary>
        /// Occurs when a weapon is fired.
        /// </summary>
        public event EventHandler<WeaponFiredEventArgs> WeaponFired;

        /// <summary>
        /// Occurs when smoke nade started.
        /// </summary>
        public event EventHandler<SmokeEventArgs> SmokeNadeStarted;

        /// <summary>
        /// Occurs when smoke nade ended.
        /// Hint: When a round ends, this is *not* caĺled.
        /// Make sure to clear nades yourself at the end of rounds
        /// </summary>
        public event EventHandler<SmokeEventArgs> SmokeNadeEnded;

        /// <summary>
        /// Occurs when decoy nade started.
        /// </summary>
        public event EventHandler<DecoyEventArgs> DecoyNadeStarted;

        /// <summary>
        /// Occurs when decoy nade ended.
        /// Hint: When a round ends, this is *not* caĺled.
        /// Make sure to clear nades yourself at the end of rounds
        /// </summary>
        public event EventHandler<DecoyEventArgs> DecoyNadeEnded;

        /// <summary>
        /// Occurs when a fire nade (incendiary / molotov) started.
        /// This currently *doesn't* contain who it threw since this is for some weird reason not networked
        /// </summary>
        public event EventHandler<FireEventArgs> FireNadeStarted;

        /// <summary>
        /// FireNadeStarted, but with correct ThrownBy player.
        /// Hint: Raised at the end of inferno_startburn tick instead of exactly when the event is parsed
        /// </summary>
        public event EventHandler<FireEventArgs> FireNadeWithOwnerStarted;

        /// <summary>
        /// Occurs when fire nade ended.
        /// Hint: When a round ends, this is *not* caĺled.
        /// Make sure to clear nades yourself at the end of rounds
        /// </summary>
        public event EventHandler<FireEventArgs> FireNadeEnded;

        /// <summary>
        /// Occurs when flash nade exploded.
        /// </summary>
        public event EventHandler<FlashEventArgs> FlashNadeExploded;

        /// <summary>
        /// Occurs when explosive nade exploded.
        /// </summary>
        public event EventHandler<GrenadeEventArgs> ExplosiveNadeExploded;

        /// <summary>
        /// Occurs when any nade reached it's target.
        /// </summary>
        public event EventHandler<NadeEventArgs> NadeReachedTarget;

        /// <summary>
        /// Occurs when bomb is being planted.
        /// </summary>
        public event EventHandler<BombEventArgs> BombBeginPlant;

        /// <summary>
        /// Occurs when the plant is aborted
        /// </summary>
        public event EventHandler<BombEventArgs> BombAbortPlant;

        /// <summary>
        /// Occurs when the bomb has been planted.
        /// </summary>
        public event EventHandler<BombEventArgs> BombPlanted;

        /// <summary>
        /// Occurs when the bomb has been defused.
        /// </summary>
        public event EventHandler<BombEventArgs> BombDefused;

        /// <summary>
        /// Occurs when bomb has exploded.
        /// </summary>
        public event EventHandler<BombEventArgs> BombExploded;

        /// <summary>
        /// Occurs when someone begins to defuse the bomb.
        /// </summary>
        public event EventHandler<BombDefuseEventArgs> BombBeginDefuse;

        /// <summary>
        /// Occurs when someone aborts to defuse the bomb.
        /// </summary>
        public event EventHandler<BombDefuseEventArgs> BombAbortDefuse;

        /// <summary>
        /// Occurs when an player is attacked by another player.
        /// Hint: Only occurs in GOTV-demos.
        /// </summary>
        public event EventHandler<PlayerHurtEventArgs> PlayerHurt;

        /// <summary>
        /// Occurs when player is blinded by flashbang
        /// Hint: The order of the blind event and FlashNadeExploded event is not always the same
        /// </summary>
        public event EventHandler<BlindEventArgs> Blind;

        /// <summary>
        /// Occurs when the player object is first updated to reference all the necessary information
        /// Hint: Event will be raised when any player with a SteamID connects, not just PlayingParticipants
        /// </summary>
        public event EventHandler<PlayerBindEventArgs> PlayerBind;

        /// <summary>
        /// Occurs when a player disconnects from the server.
        /// </summary>
        public event EventHandler<PlayerDisconnectEventArgs> PlayerDisconnect;

        /// <summary>
        /// Occurs when the server uses the "say" command
        /// </summary>
        public event EventHandler<SayTextEventArgs> SayText;

        /// <summary>
        /// Occurs when a player uses the "say" command
        /// </summary>
        public event EventHandler<SayText2EventArgs> SayText2;

        /// <summary>
        /// Occurs when the server display a player rank
        /// </summary>
        public event EventHandler<RankUpdateEventArgs> RankUpdate;

        /// <summary>
        /// Occurs when a player left a buy zone
        /// </summary>
        public event EventHandler<PlayerLeftBuyZoneEventArgs> PlayerLeftBuyZone;

        /// <summary>
        /// Occurs when a player's money have changed
        /// </summary>
        public event EventHandler<PlayerMoneyChangedEventArgs> PlayerMoneyChanged;

        /// <summary>
        /// Occurs when a player pick a weapon (buy or not)
        /// </summary>
        public event EventHandler<PlayerPickWeaponEventArgs> PlayerPickWeapon;

        /// <summary>
        /// Occurs when a player drop a weapon
        /// Drops happen when a player is killed too, check for IsAlive to know if drops are due to a kill
        /// </summary>
        public event EventHandler<PlayerDropWeaponEventArgs> PlayerDropWeapon;

        /// <summary>
        /// Occurs when a player buy an equipment
        /// </summary>
        public event EventHandler<PlayerBuyEventArgs> PlayerBuy;

        /// <summary>
        /// Occurs when a ConVar has changed
        /// </summary>
        public event EventHandler<ConVarChangeEventArgs> ConVarChange;

        /// <summary>
        /// Occurs when a team's score change
        /// </summary>
        public event EventHandler<TeamScoreChangeEventArgs> TeamScoreChange;
        #endregion

        /// <summary>
        /// The mapname of the Demo. Only avaible after the header is parsed.
        /// Is a string like "de_dust2".
        /// </summary>
        /// <value>The map.</value>
        public string Map => Header.MapName;

        /// <summary>
        /// The header of the demo, containing some useful information.
        /// </summary>
        /// <value>The header.</value>
        public DemoHeader Header { get; private set; }

        /// <summary>
        /// Gets the participants of this game
        /// </summary>
        /// <value>The participants.</value>
        public IEnumerable<Player> Participants => Players.Values;

        /// <summary>
        /// Gets all the participants of this game, that aren't spectating.
        /// </summary>
        /// <value>The playing participants.</value>
        public IEnumerable<Player> PlayingParticipants => Players.Values.Where(a => a.Team != Team.Spectate);

        /// <summary>
        /// The tickrate *of the demo* (16 for normal GOTV-demos)
        /// </summary>
        /// <value>The tick rate.</value>
        public float TickRate => Header.PlaybackFrames / Header.PlaybackTime;

        /// <summary>
        /// How long a tick of the demo is in s^-1
        /// </summary>
        /// <value>The tick time.</value>
        public float TickTime => Header.PlaybackTime / Header.PlaybackFrames;

        /// <summary>
        /// Gets the parsing progress. 0 = beginning, ~1 = finished (it can actually be > 1, so be careful!)
        /// </summary>
        /// <value>The parsing progress.</value>
        public float ParsingProgress => CurrentTick / (float)Header.PlaybackFrames;

        /// <summary>
        /// The current tick the parser has seen. So if it's a 16-tick demo,
        /// it will have 16 after one second.
        /// </summary>
        /// <value>The current tick.</value>
        public int CurrentTick { get; private set; }

        /// <summary>
        /// The current ingame-tick as reported by the demo-file.
        /// </summary>
        /// <value>The current tick.</value>
        public int IngameTick { get; internal set; }

        /// <summary>
        /// How far we've advanced in the demo in seconds.
        /// </summary>
        /// <value>The current time.</value>
        public float CurrentTime => CurrentTick * TickTime;

        /// <summary>
        /// The current round number.
        /// </summary>
        /// <value>The Current Round number.</value>
        public int CurrentRound { get; internal set; } = 0;

        /// <summary>
        /// The Rounds the Counter-Terrorists have won at this point.
        /// </summary>
        /// <value>The CT score.</value>
        public int CTScore { get; private set; }

        /// <summary>
        /// The Rounds the Terrorists have won at this point.
        /// </summary>
        /// <value>The T score.</value>
        public int TScore { get; private set; }

        /// <summary>
        /// The clan name of the Counter-Terrorists
        /// </summary>
        /// <value>The name of the CT clan.</value>
        public string CTClanName { get; private set; }

        /// <summary>
        /// The clan name of the Terrorists
        /// </summary>
        /// <value>The name of the T clan.</value>
        public string TClanName { get; private set; }

        /// <summary>
        /// The flag of the Counter-Terrorists
        /// </summary>
        /// <value>The flag of the CT clan.</value>
        public string CTFlag { get; private set; }

        /// <summary>
        /// The flag of the Terrorists
        /// </summary>
        /// <value>The flag of the T clan.</value>
        public string TFlag { get; private set; }

        /// <summary>
        /// An map entity <-> weapon. Used to remember whether a weapon is a p250,
        /// how much ammunition it has, etc.
        /// </summary>
        public Equipment[] Weapons = new Equipment[MAX_ENTITIES];

        /// <summary>
        /// The projectiles currently flying around. This is important since a Projectile has a m_hThrower, and this is cool for molotovs.
        /// </summary>
        public Projectile[] Projectiles = new Projectile[MAX_ENTITIES];

        /// <summary>
        /// A parser for DataTables. This contains the ServerClasses and DataTables.
        /// </summary>
        internal DataTableParser SendTableParser = new DataTableParser();

        /// <summary>
        /// List of Players updated each tick from RawPlayers & AdditionalInformation
        /// </summary>
        internal Dictionary<int, Player> Players = new Dictionary<int, Player>();

        /// <summary>
        /// All entities currently alive in the demo.
        /// </summary>
        internal Entity[] Entities = new Entity[MAX_ENTITIES]; // Max 2048 entities.

        /// <summary>
        /// The ModelPrecache. With this we can tell which model an entity has.
        /// Useful for finding out whether a weapon is a P250 or a CZ
        /// </summary>
        internal List<string> ModelPrecache = new List<string>();

        /// <summary>
        /// The string tables sent by the server.
        /// </summary>
        internal List<CreateStringTable> StringTables = new List<CreateStringTable>();

        /// <summary>
        /// A parser for DEM_STRINGTABLES-Packets
        /// </summary>
        internal readonly StringTableParser StringTablesParser = new StringTableParser();

        internal List<BoundingBoxInformation> triggers = new List<BoundingBoxInformation>();

        internal Dictionary<int, Player> InfernoOwners = new Dictionary<int, Player>();

        /// <summary>
        /// The indicies of the bombsites - useful to find out
        /// where the bomb is planted
        /// </summary>
        internal int BombsiteAIndex = -1;
        internal int BombsiteBIndex = -1;

        internal Vector BombsiteACenter;
        internal Vector BombsiteBCenter;

        /// <summary>
        /// And GameEvent is just sent with ID |--> Value, but we need Name |--> Value.
        /// Luckily these contain a map ID |--> Name.
        /// </summary>
        internal Dictionary<int, GameEventList.Descriptor> GEH_Descriptors = null;

        /// <summary>
        /// Holds inferno_startburn event args so they can be matched with player
        /// </summary>
        internal Queue<Tuple<int, FireEventArgs>> GEH_StartBurns = new Queue<Tuple<int, FireEventArgs>>();

        /// <summary>
        /// The preprocessed baselines, useful to create entities fast
        /// </summary>
        internal Dictionary<int, object[]> PreprocessedBaselines = new Dictionary<int, object[]>();

        /// <summary>
        /// The instance baselines.
        /// When a new edict is created one would need to send all the information twice.
        /// Since this is (was) expensive, valve sends an instancebaseline, which contains defaults
        /// for all the properties.
        /// </summary>
        internal Dictionary<int, byte[]> InstanceBaseline = new Dictionary<int, byte[]>();

        /// <summary>
        /// Contains information about the players, accessible by the userid.
        /// </summary>
        internal PlayerInfo[] RawPlayers = new PlayerInfo[MAXPLAYERS];

        /// <summary>
        /// The stream of the demo - all the information go here
        /// </summary>
        private readonly IBitStream BitStream;

        /// <summary>
        /// Containing info about players, accessible by the entity-id
        /// </summary>
        private Player[] PlayerInformations = new Player[MAXPLAYERS];

        /// <summary>
        /// This contains additional informations about each player, such as Kills, Deaths, etc.
        /// This is networked seperately from the player, so we need to cache it somewhere else.
        /// </summary>
        private AdditionalPlayerInformation[] additionalInformations = new AdditionalPlayerInformation[MAXPLAYERS];

        /// <summary>
        /// The ID of the CT-Team
        /// </summary>
        private int ctID = -1;

        /// <summary>
        /// The ID of the terrorist team
        /// </summary>
        private int tID = -1;

        /// <summary>
        /// Initializes a new DemoParser. Right point if you want to start analyzing demos.
        /// Hint: ParseHeader() is propably what you want to look into next.
        /// </summary>
        /// <param name="input">An input-stream.</param>
        public DemoParser(Stream input)
        {
            BitStream = BitStreamUtil.Create(input);

            for (int i = 0; i < MAXPLAYERS; i++)
            {
                additionalInformations[i] = new AdditionalPlayerInformation();
            }
        }

        /// <summary>
        /// Parses the header (first few hundret bytes) of the demo.
        /// </summary>
        public void ParseHeader()
        {
            Header = DemoHeader.ParseFrom(BitStream);

            if (Header.Filestamp != "HL2DEMO")
            {
                throw new InvalidDataException("Invalid File-Type - expecting HL2DEMO");
            }

            if (Header.GameDirectory != "csgo")
            {
                throw new InvalidDataException("Invalid Demo-Game");
            }

            if (Header.DemoProtocol != 4)
            {
                throw new InvalidDataException("Invalid Demo-Protocol");
            }

            HeaderParsed?.Invoke(this, new HeaderParsedEventArgs(Header));
        }

        /// <summary>
        /// Parses this file until the end of the demo is reached.
        /// </summary>
        public Task ParseToEnd()
        {
            MustParse = true;
            return Task.Run(() =>
            {
                while (ParseNextTick() && MustParse) ;

                // @TODO: Is this a bad assumption?
                Dispose();
            });
        }

        public void CancelParsing()
        {
            MustParse = false;
        }

        /// <summary>
        /// Parses the next tick of the demo.
        /// </summary>
        /// <returns><c>true</c>, if this wasn't the last tick, <c>false</c> otherwise.</returns>
        public bool ParseNextTick()
        {
            if (Header == null)
            {
                throw new InvalidOperationException("You need to call ParseHeader first before you call ParseToEnd or ParseNextTick!");
            }

            bool b = ParseTick();

            for (int i = 0; i < RawPlayers.Length; i++)
            {
                if (RawPlayers[i] == null)
                {
                    continue;
                }

                PlayerInfo rawPlayer = RawPlayers[i];
                int id = rawPlayer.UserID;

                if (PlayerInformations[i] != null)
                {
                    bool newplayer = false;
                    if (!Players.ContainsKey(id))
                    {
                        Players[id] = PlayerInformations[i];
                        newplayer = true;
                    }

                    Player p = Players[id];
                    p.Name = rawPlayer.Name;
                    p.SteamID = rawPlayer.XUID;
                    p.SteamID32 = rawPlayer.GUID;
                    p.AdditionalInformations = additionalInformations[p.EntityID];

                    if (p.IsAlive)
                    {
                        p.LastAlivePosition = p.Position.Copy();
                    }

                    if (newplayer && p.SteamID != 0)
                    {
                        RaisePlayerBind(new PlayerBindEventArgs
                        {
                            Player = p
                        });
                    }

                    while (p.DroppedWeapons.Count > 0)
                    {
                        RaisePlayerDropWeapon(new PlayerDropWeaponEventArgs
                        {
                            Player = p,
                            Weapon = p.DroppedWeapons.Dequeue(),
                        });
                    }

                    while (p.PickedWeapons.Count > 0)
                    {
                        Equipment equipment = p.PickedWeapons.Dequeue();
                        RaisePlayerPickWeapon(new PlayerPickWeaponEventArgs
                        {
                            Player = p,
                            Weapon = equipment,
                        });

                        // Since item_purchase event isn't networked we use equipment picks to detect buy events based on 3 things:
                        // 1. The weapon pick happened in the buy zone
                        // 2. There is no previous weapon owner (avoid drop from a friend detected has a buy)
                        // 3. It's not a knife / C4 or a default pistol (glock or USP)
                        // This logic may not be 100% perfect but it seems to be accurate
                        if (p.IsInBuyZone && equipment.PrevOwner == null
                            && equipment.Weapon != EquipmentElement.Knife && equipment.Weapon != EquipmentElement.Glock
                            && equipment.Weapon != EquipmentElement.USP && equipment.Weapon != EquipmentElement.Bomb)
                        {
                            RaisePlayerBuyWeapon(new PlayerBuyEventArgs
                            {
                                Player = p,
                                Weapon = equipment,
                            });
                        }
                    }
                }
            }

            while (GEH_StartBurns.Count > 0)
            {
                Tuple<int, FireEventArgs> fireTup = GEH_StartBurns.Dequeue();
                if (InfernoOwners.ContainsKey(fireTup.Item1))
                {
                    fireTup.Item2.ThrownBy = InfernoOwners[fireTup.Item1];
                    RaiseFireWithOwnerStart(fireTup.Item2);
                }
            }

            if (b)
            {
                TickDone?.Invoke(this, new TickDoneEventArgs
                {
                    CurrentTick = CurrentTick,
                    ParsingProgress = ParsingProgress,
                });
            }

            return b;
        }

        /// <summary>
        /// Parses the tick internally
        /// </summary>
        /// <returns><c>true</c>, if tick was parsed, <c>false</c> otherwise.</returns>
        private bool ParseTick()
        {
            DemoCommand command = (DemoCommand)BitStream.ReadByte();

            IngameTick = (int)BitStream.ReadInt(32); // Tick number
            _ = BitStream.ReadByte(); // Player slot
            CurrentTick++;

            switch (command)
            {
                case DemoCommand.Synctick:
                    break;
                case DemoCommand.Stop:
                    return false;
                case DemoCommand.ConsoleCommand:
                    BitStream.BeginChunk(BitStream.ReadSignedInt(32) * 8);
                    BitStream.EndChunk();
                    break;
                case DemoCommand.DataTables:
                    BitStream.BeginChunk(BitStream.ReadSignedInt(32) * 8);
                    SendTableParser.ParsePacket(BitStream);
                    BitStream.EndChunk();

                    // And now we have the entities, we can bind events on them.
                    BindEntites();

                    break;
                case DemoCommand.StringTables:
                    BitStream.BeginChunk(BitStream.ReadSignedInt(32) * 8);
                    StringTablesParser.ParsePacket(BitStream, this);
                    BitStream.EndChunk();
                    break;
                case DemoCommand.UserCommand:
                    _ = BitStream.ReadInt(32);
                    BitStream.BeginChunk(BitStream.ReadSignedInt(32) * 8);
                    BitStream.EndChunk();
                    break;
                case DemoCommand.Signon:
                case DemoCommand.Packet:
                    ParseDemoPacket();
                    break;
                case DemoCommand.CustomData:
                    break;
                default:
                    throw new Exception("Can't handle Demo-Command " + command);
            }

            return true;
        }

        /// <summary>
        /// Parses a DEM_Packet.
        /// </summary>
        private void ParseDemoPacket()
        {
            _ = CommandInfo.Parse(BitStream);
            _ = BitStream.ReadInt(32); // SeqNrIn
            _ = BitStream.ReadInt(32); // SeqNrOut

            BitStream.BeginChunk(BitStream.ReadSignedInt(32) * 8);
            DemoPacketParser.ParsePacket(BitStream, this);
            BitStream.EndChunk();
        }

        /// <summary>
        /// Binds the events for entities. An Entity has many properties.
        /// You can subscribe to when an entity of a specific class is created,
        /// and then you can subscribe to updates of properties of this entity.
        /// This is a bit complex, but very fast.
        /// </summary>
        private void BindEntites()
        {
            BindTeamScores();
            BindBombSites();
            BindPlayers();
            BindWeapons();
            BindProjectiles();
            BindInfernos();
            BindGameRules();
        }

        private void BindTeamScores()
        {
            SendTableParser.FindByName("CCSTeam").OnNewEntity += (object sender, EntityCreatedEventArgs e) =>
            {
                string team = null;
                string teamName = null;
                string teamFlag = null;
                int teamID = -1;
                int score = 0;

                e.Entity.FindProperty("m_scoreTotal").IntReceived += (_, update) =>
                {
                    score = update.Value;
                };

                e.Entity.FindProperty("m_iTeamNum").IntReceived += (_, update) =>
                {
                    teamID = update.Value;

                    if (team == "CT")
                    {
                        ctID = teamID;
                        CTScore = score;
                        foreach (Player p in PlayerInformations.Where(a => a != null && a.TeamID == teamID))
                        {
                            p.Team = Team.CounterTerrorist;
                        }
                    }

                    if (team == "TERRORIST")
                    {
                        tID = teamID;
                        TScore = score;
                        foreach (Player p in PlayerInformations.Where(a => a != null && a.TeamID == teamID))
                        {
                            p.Team = Team.Terrorist;
                        }
                    }
                };

                e.Entity.FindProperty("m_szTeamname").StringRecieved += (_, recievedTeamName) =>
                {
                    team = recievedTeamName.Value;

                    if (recievedTeamName.Value == "CT")
                    {
                        CTScore = score;
                        CTClanName = teamName;

                        e.Entity.FindProperty("m_scoreTotal").IntReceived += (__, update) =>
                        {
                            int newScore = update.Value;
                            int oldScore = CTScore;
                            CTScore = newScore;

                            if (oldScore != newScore)
                            {
                                RaiseTeamScoreChange(new TeamScoreChangeEventArgs
                                {
                                    Team = Team.CounterTerrorist,
                                    OldScore = oldScore,
                                    NewScore = newScore,
                                });
                            }
                        };

                        if (teamID != -1)
                        {
                            ctID = teamID;
                            foreach (Player p in PlayerInformations.Where(a => a != null && a.TeamID == teamID))
                            {
                                p.Team = Team.CounterTerrorist;
                            }
                        }
                    }
                    else if (recievedTeamName.Value == "TERRORIST")
                    {
                        TScore = score;
                        TClanName = teamName;
                        e.Entity.FindProperty("m_scoreTotal").IntReceived += (__, update) =>
                        {
                            int newScore = update.Value;
                            int oldScore = TScore;
                            TScore = newScore;

                            if (oldScore != newScore)
                            {
                                RaiseTeamScoreChange(new TeamScoreChangeEventArgs
                                {
                                    Team = Team.Terrorist,
                                    OldScore = oldScore,
                                    NewScore = newScore,
                                });
                            }
                        };

                        if (teamID != -1)
                        {
                            tID = teamID;
                            foreach (Player p in PlayerInformations.Where(a => a != null && a.TeamID == teamID))
                            {
                                p.Team = Team.Terrorist;
                            }
                        }
                    }
                };

                e.Entity.FindProperty("m_szTeamFlagImage").StringRecieved += (_, recievedTeamFlag) =>
                {
                    teamFlag = recievedTeamFlag.Value;

                    if (team == "CT")
                    {
                        CTFlag = teamFlag;
                    }
                    else if (team == "TERRORIST")
                    {
                        TFlag = teamFlag;
                    }
                };

                e.Entity.FindProperty("m_szClanTeamname").StringRecieved += (_, recievedClanName) =>
                {
                    teamName = recievedClanName.Value;
                    if (team == "CT")
                    {
                        CTClanName = recievedClanName.Value;
                    }
                    else if (team == "TERRORIST")
                    {
                        TClanName = recievedClanName.Value;
                    }
                };
            };
        }

        private void BindPlayers()
        {
            SendTableParser.FindByName("CCSPlayer").OnNewEntity += (object sender, EntityCreatedEventArgs e) => HandleNewPlayer(e.Entity);

            SendTableParser.FindByName("CCSPlayerResource").OnNewEntity += (_, playerResources) =>
            {
                for (int i = 0; i < 64; i++)
                {
                    // This is passed as reference to the delegates
                    int iForTheMethod = i;
                    string iString = i.ToString().PadLeft(3, '0');

                    playerResources.Entity.FindProperty("m_szClan." + iString).StringRecieved += (sender, e) =>
                    {
                        additionalInformations[iForTheMethod].Clantag = e.Value;
                    };

                    playerResources.Entity.FindProperty("m_iPing." + iString).IntReceived += (sender, e) =>
                    {
                        additionalInformations[iForTheMethod].Ping = e.Value;
                    };

                    playerResources.Entity.FindProperty("m_iScore." + iString).IntReceived += (sender, e) =>
                    {
                        additionalInformations[iForTheMethod].Score = e.Value;
                    };

                    playerResources.Entity.FindProperty("m_iKills." + iString).IntReceived += (sender, e) =>
                    {
                        additionalInformations[iForTheMethod].Kills = e.Value;
                    };

                    playerResources.Entity.FindProperty("m_iDeaths." + iString).IntReceived += (sender, e) =>
                    {
                        additionalInformations[iForTheMethod].Deaths = e.Value;
                    };

                    playerResources.Entity.FindProperty("m_iAssists." + iString).IntReceived += (sender, e) =>
                    {
                        additionalInformations[iForTheMethod].Assists = e.Value;
                    };

                    playerResources.Entity.FindProperty("m_iMVPs." + iString).IntReceived += (sender, e) =>
                    {
                        additionalInformations[iForTheMethod].MVPs = e.Value;
                    };

                    playerResources.Entity.FindProperty("m_iTotalCashSpent." + iString).IntReceived += (sender, e) =>
                    {
                        additionalInformations[iForTheMethod].TotalCashSpent = e.Value;
                    };
                }
            };

            SendTableParser.FindByDTName("DT_CSPlayer").OnNewEntity += (object _, EntityCreatedEventArgs e) =>
            {
                e.Entity.FindProperty("m_bIsScoped").IntReceived += (__, value) =>
                {
                    // Assume if there's no existing PlayerInformation we can skip
                    if (PlayerInformations[e.Entity.ID - 1] == null)
                    {
                        return;
                    }

                    PlayerInformations[e.Entity.ID - 1].IsScoped = value.Value == 1;
                };
            };
        }

        private void HandleNewPlayer(Entity playerEntity)
        {
            Player p = null;
            if (PlayerInformations[playerEntity.ID - 1] != null)
            {
                p = PlayerInformations[playerEntity.ID - 1];
            }
            else
            {
                p = new Player
                {
                    SteamID = -1,
                    Name = "unconnected",
                };

                PlayerInformations[playerEntity.ID - 1] = p;
            }

            p.EntityID = playerEntity.ID;
            p.Entity = playerEntity;
            p.Position = new Vector();
            p.Velocity = new Vector();

            playerEntity.FindProperty("cslocaldata.m_vecOrigin").VectorRecieved += (sender, e) =>
            {
                p.Position.X = e.Value.X;
                p.Position.Y = e.Value.Y;
            };

            playerEntity.FindProperty("cslocaldata.m_vecOrigin[2]").FloatRecieved += (sender, e) =>
            {
                p.Position.Z = e.Value;
            };

            // Problem: Teams are networked after the players... How do we solve that?
            playerEntity.FindProperty("m_iTeamNum").IntReceived += (sender, e) =>
            {
                p.TeamID = e.Value;
                p.Team = TeamFromTeamID(e.Value);
            };

            playerEntity.FindProperty("m_iHealth").IntReceived += (sender, e) => p.HP = e.Value;

            playerEntity.FindProperty("m_ArmorValue").IntReceived += (sender, e) =>
            {
                // player bought kevlar OR maybe an assaultsuilt (vesthelm)
                // Since m_iAccount is updated first and m_bHasHelmet next, we store temporarily the value
                // of the last item bought (on m_iAccount change) to detect if he bought just a vest (650$) or a vesthelm
                // we raise this event only if the last item bought value is 650
                if (p.IsInBuyZone && p.Armor < 100 && e.Value == 100 && p.LastItemBoughtValue == 650)
                {
                    RaisePlayerBuyWeapon(new PlayerBuyEventArgs
                    {
                        Player = p,
                        Weapon = new Equipment("item_kevlar"),
                    });

                    p.LastItemBoughtValue = 0;
                }

                p.Armor = e.Value;
            };

            playerEntity.FindProperty("m_bHasDefuser").IntReceived += (sender, e) =>
            {
                bool hasDefuserNow = e.Value == 1;
                if (p.IsInBuyZone && !p.HasDefuseKit && hasDefuserNow)
                {
                    // player bought a defuser
                    RaisePlayerBuyWeapon(new PlayerBuyEventArgs
                    {
                        Player = p,
                        Weapon = new Equipment("item_defuser"),
                    });
                }

                p.HasDefuseKit = hasDefuserNow;
            };

            playerEntity.FindProperty("m_bHasHelmet").IntReceived += (sender, e) =>
            {
                bool hasHelmetNow = e.Value == 1;
                if (p.IsInBuyZone && !p.HasHelmet && hasHelmetNow)
                {
                    RaisePlayerBuyWeapon(new PlayerBuyEventArgs
                    {
                        Player = p,
                        Weapon = new Equipment("item_assaultsuit"),
                    });
                }

                p.HasHelmet = hasHelmetNow;
            };

            playerEntity.FindProperty("localdata.m_Local.m_bDucking").IntReceived += (sender, e) => p.IsDucking = e.Value == 1;

            playerEntity.FindProperty("m_iAccount").IntReceived += (sender, e) =>
            {
                int newMoney = e.Value;
                // WARN: happen before the weapon pick event in case of a buy
                // 1. Money change detected
                // 2. Weapon has been picked
                if (p.SteamID != -1 && p.Money != newMoney)
                {
                    RaisePlayerMoneyChange(new PlayerMoneyChangedEventArgs
                    {
                        Player = p,
                        OldAccount = p.Money,
                        NewAccount = newMoney,
                    });

                    p.LastItemBoughtValue = p.Money - newMoney;
                }

                p.Money = e.Value;
            };

            playerEntity.FindProperty("m_angEyeAngles[0]").FloatRecieved += (sender, e) => p.ViewDirectionX = e.Value;
            playerEntity.FindProperty("m_angEyeAngles[1]").FloatRecieved += (sender, e) => p.ViewDirectionY = e.Value;
            playerEntity.FindProperty("m_flFlashDuration").FloatRecieved += (sender, e) => p.FlashDuration = e.Value;

            playerEntity.FindProperty("localdata.m_vecVelocity[0]").FloatRecieved += (sender, e) => p.Velocity.X = e.Value;
            playerEntity.FindProperty("localdata.m_vecVelocity[1]").FloatRecieved += (sender, e) => p.Velocity.Y = e.Value;
            playerEntity.FindProperty("localdata.m_vecVelocity[2]").FloatRecieved += (sender, e) => p.Velocity.Z = e.Value;

            playerEntity.FindProperty("m_unCurrentEquipmentValue").IntReceived += (sender, e) => p.CurrentEquipmentValue = e.Value;
            playerEntity.FindProperty("m_unRoundStartEquipmentValue").IntReceived += (sender, e) => p.RoundStartEquipmentValue = e.Value;
            playerEntity.FindProperty("m_unFreezetimeEndEquipmentValue").IntReceived += (sender, e) => p.FreezetimeEndEquipmentValue = e.Value;

            playerEntity.FindProperty("m_bInBuyZone").IntReceived += (sender, e) =>
            {
                bool newValue = e.Value == 1;
                if (p.IsInBuyZone && !newValue)
                {
                    RaisePlayerLeftBuyZone(new PlayerLeftBuyZoneEventArgs
                    {
                        Player = p,
                    });
                }

                p.IsInBuyZone = newValue;
            };

            // Weapon attribution
            string weaponPrefix = "m_hMyWeapons.";
            if (playerEntity.Props.All(a => a.Entry.PropertyName != "m_hMyWeapons.000"))
            {
                weaponPrefix = "bcc_nonlocaldata.m_hMyWeapons.";
            }

            int[] cache = new int[MAXWEAPONS];
            for (int i = 0; i < MAXWEAPONS; i++)
            {
                // i is passed as reference to the delegate.
                int iForTheMethod = i;
                playerEntity.FindProperty(weaponPrefix + i.ToString().PadLeft(3, '0')).IntReceived += (sender, e) =>
                {
                    int index = e.Value & INDEX_MASK;
                    if (index != INDEX_MASK)
                    {
                        if (cache[iForTheMethod] != 0) //Player already has a weapon in this slot.
                        {
                            // Add to the weapons dropped queue that will be clear at the end of the tick
                            p.DroppedWeapons.Enqueue(p.rawWeapons[cache[iForTheMethod]]);

                            // Remove the player's weapon slot
                            _ = p.rawWeapons.Remove(cache[iForTheMethod]);
                            cache[iForTheMethod] = 0;
                        }

                        // Add the new weapon to player's slot
                        cache[iForTheMethod] = index;
                        Equipment weapon = Weapons[index];
                        weapon.Owner = p;
                        p.rawWeapons[index] = weapon;
                        p.PickedWeapons.Enqueue(weapon);
                    }
                    else
                    {
                        if (cache[iForTheMethod] != 0)
                        {
                            p.DroppedWeapons.Enqueue(p.rawWeapons[cache[iForTheMethod]]);

                            if (p.rawWeapons.ContainsKey(cache[iForTheMethod]))
                            {
                                // Is this necessary as we remove element from dict just after?
                                p.rawWeapons[cache[iForTheMethod]].Owner = null;
                            }
                        }

                        _ = p.rawWeapons.Remove(cache[iForTheMethod]);
                        cache[iForTheMethod] = 0;
                    }
                };
            }

            playerEntity.FindProperty("m_hActiveWeapon").IntReceived += (sender, e) => p.ActiveWeaponID = e.Value & INDEX_MASK;

            for (int i = 0; i < 32; i++)
            {
                int iForTheMethod = i;

                playerEntity.FindProperty("m_iAmmo." + i.ToString().PadLeft(3, '0')).IntReceived += (sender, e) =>
                {
                    p.AmmoLeft[iForTheMethod] = e.Value;
                };
            }
        }

        private void BindWeapons()
        {
            for (int i = 0; i < MAX_ENTITIES; i++)
            {
                Weapons[i] = new Equipment();
            }

            IEnumerable<ServerClass> WeaponServerClasses = SendTableParser.ServerClasses
                .Where(a => a.BaseClasses.Any(c => c.Name == "CWeaponCSBase"));

            foreach (ServerClass s in WeaponServerClasses)
            {
                s.OnNewEntity += HandleWeapon;
            }
        }

        private void HandleWeapon(object sender, EntityCreatedEventArgs e)
        {
            e.Entity.EntityLeft += (_, left) =>
            {
                Weapons[left.Entity.ID] = new Equipment();
            };

            Equipment equipment = Weapons[e.Entity.ID];
            equipment.EntityID = e.Entity.ID;
            equipment.AmmoInMagazine = -1;

            e.Entity.FindProperty("m_iClip1").IntReceived += (_, ammoUpdate) =>
            {
                equipment.AmmoInMagazine = ammoUpdate.Value - 1;
            };

            e.Entity.FindProperty("LocalWeaponData.m_iPrimaryAmmoType").IntReceived += (_, typeUpdate) =>
            {
                equipment.AmmoType = typeUpdate.Value;
            };

            e.Entity.FindProperty("m_AttributeManager.m_Item.m_iItemDefinitionIndex").IntReceived += (_, update) =>
            {
                // We use the item index definition to detect each weapons except for:
                // kevlar, helmet and defuser (detected from item names, see Equipment#MapEquipement())
                // This indexes are defined in the game file scripts/items/items_game.txt
                EquipmentMapping map = Equipment.Equipments.FirstOrDefault(eq => eq.ItemIndex == update.Value);
                if (map.ItemIndex == 0)
                {
                    Trace.WriteLine($"Unknown weapon index {update.Value} class {e.Class} {equipment.Weapon}");
                }
                else
                {
                    equipment.OriginalString = map.OriginalName;
                    equipment.Weapon = map.Element;
                }
            };

            e.Entity.FindProperty("m_hOwner").IntReceived += (_, update) =>
            {
                equipment.Owner = PlayingParticipants.FirstOrDefault(p => p.EntityID == (update.Value & INDEX_MASK));
            };

            e.Entity.FindProperty("m_hPrevOwner").IntReceived += (_, update) =>
            {
                equipment.PrevOwner = PlayingParticipants.FirstOrDefault(p => p.EntityID == (update.Value & INDEX_MASK));
            };
        }

        private void BindProjectiles()
        {
            // Grenade that has been thrown by player.
            IEnumerable<ServerClass> ProjectileServerClasses = SendTableParser.ServerClasses
                .Where(a => a.BaseClasses.Any(c => c.Name == "CBaseGrenade"));

            foreach (ServerClass s in ProjectileServerClasses)
            {
                s.OnNewEntity += HandleNewProjectile;
            }

            // "CBaseCSGrenade" // Grenades dropped by a dying player
        }

        private void HandleNewProjectile(object _, EntityCreatedEventArgs entityCreatedEvent)
        {
            Entity entity = entityCreatedEvent.Entity;

            Projectiles[entity.ID] = new Projectile
            {
                ServerClassName = entity.ServerClass.Name,
            };

            entity.FindProperty("m_hThrower").IntReceived += (__, e) =>
            {
                int ownerID = (e.Value & INDEX_MASK) - 1;
                if (ownerID >= PlayerInformations.Length)
                {
                    return;
                }

                Projectiles[e.Entity.ID].OwnerID = ownerID;
                Projectiles[e.Entity.ID].Owner = PlayerInformations[ownerID];
            };

            entity.FindProperty("m_hOwnerEntity").IntReceived += (__, e) =>
            {
                int ownerID = (e.Value & INDEX_MASK) - 1;
                if (ownerID >= PlayerInformations.Length)
                {
                    return;
                }

                Projectiles[e.Entity.ID].OwnerID = ownerID;
                Projectiles[e.Entity.ID].Owner = PlayerInformations[ownerID];
            };

            entity.FindProperty("m_cellbits").IntReceived += (__, e) => Projectiles[e.Entity.ID].CellBits = e.Value;
            entity.FindProperty("m_cellX").IntReceived += (__, e) => Projectiles[e.Entity.ID].CellX = e.Value;
            entity.FindProperty("m_cellY").IntReceived += (__, e) => Projectiles[e.Entity.ID].CellY = e.Value;
            entity.FindProperty("m_cellZ").IntReceived += (__, e) => Projectiles[e.Entity.ID].CellZ = e.Value;
            entity.FindProperty("m_vecOrigin").VectorRecieved += (__, e) => Projectiles[e.Entity.ID].VecOrigin = e.Value;

            entity.EntityLeft += (sender, e) => Projectiles[e.Entity.ID] = null;
        }

        private void BindGameRules()
        {
            SendTableParser.FindByName("CCSGameRulesProxy").OnNewEntity += (_, entityCreatedEvent) =>
            {
                Entity entity = entityCreatedEvent.Entity;
                entity.FindProperty("cs_gamerules_data.m_totalRoundsPlayed").IntReceived += (__, e) => CurrentRound = e.Value;

                // entity.FindProperty('cs_gamerules_data.m_gamePhase')
                // entity.FindProperty('cs_gamerules_data.m_bWarmupPeriod')
                // entity.FindProperty('cs_gamerules_data.m_bHasMatchStarted')
            };
        }

        private void BindBombSites()
        {
            SendTableParser.FindByName("CCSPlayerResource").OnNewEntity += (_, newResource) =>
            {
                newResource.Entity.FindProperty("m_bombsiteCenterA").VectorRecieved += (__, center) =>
                {
                    BombsiteACenter = center.Value;
                };

                newResource.Entity.FindProperty("m_bombsiteCenterB").VectorRecieved += (__, center) =>
                {
                    BombsiteBCenter = center.Value;
                };
            };

            SendTableParser.FindByName("CBaseTrigger").OnNewEntity += (s1, newResource) =>
            {
                BoundingBoxInformation trigger = new BoundingBoxInformation(newResource.Entity.ID);
                triggers.Add(trigger);

                newResource.Entity.FindProperty("m_Collision.m_vecMins").VectorRecieved += (s2, vector) =>
                {
                    trigger.Min = vector.Value;
                };

                newResource.Entity.FindProperty("m_Collision.m_vecMaxs").VectorRecieved += (s3, vector) =>
                {
                    trigger.Max = vector.Value;
                };
            };
        }

        // This is the actual fire, not the projectile.
        private void BindInfernos()
        {
            ServerClass inferno = SendTableParser.FindByName("CInferno");

            inferno.OnNewEntity += (s, infEntity) =>
            {
                infEntity.Entity.FindProperty("m_hOwnerEntity").IntReceived += (_, handleID) =>
                {
                    int playerEntityID = handleID.Value & INDEX_MASK;
                    if (playerEntityID < PlayerInformations.Length
                        && PlayerInformations[playerEntityID - 1] != null)
                    {
                        InfernoOwners[infEntity.Entity.ID] = PlayerInformations[playerEntityID - 1];
                    }
                };
            };

            inferno.OnDestroyEntity += (s, infEntity) => InfernoOwners.Remove(infEntity.Entity.ID);
        }

        internal Player PlayerFromPlayerID(int playerID)
        {
            return Players.ContainsKey(playerID) ? Players[playerID] : null;
        }

        internal Team TeamFromTeamID(int teamID)
        {
            if (teamID == tID)
            {
                return Team.Terrorist;
            }
            else if (teamID == ctID)
            {
                return Team.CounterTerrorist;
            }
            else
            {
                return Team.Spectate;
            }
        }

        #region EventCaller

        internal void RaiseMatchStarted()
        {
            MatchStarted?.Invoke(this, new MatchStartedEventArgs());
        }

        internal void RaiseRoundAnnounceMatchStarted()
        {
            RoundAnnounceMatchStarted?.Invoke(this, new RoundAnnounceMatchStartedEventArgs());
        }

        internal void RaiseWinPanelMatch()
        {
            WinPanelMatch?.Invoke(this, new WinPanelMatchEventArgs());
        }

        internal void RaiseRoundStart(RoundStartedEventArgs rs)
        {
            RoundStart?.Invoke(this, rs);
        }

        internal void RaiseRoundFinal()
        {
            RoundFinal?.Invoke(this, new RoundFinalEventArgs());
        }

        internal void RaiseLastRoundHalf()
        {
            LastRoundHalf?.Invoke(this, new LastRoundHalfEventArgs());
        }

        internal void RaiseRoundEnd(RoundEndedEventArgs re)
        {
            RoundEnd?.Invoke(this, re);
        }

        internal void RaiseRoundOfficiallyEnd()
        {
            RoundOfficiallyEnd?.Invoke(this, new RoundOfficiallyEndedEventArgs());
        }

        internal void RaiseRoundMVP(RoundMVPEventArgs re)
        {
            RoundMVP?.Invoke(this, re);
        }

        internal void RaiseFreezetimeEnded()
        {
            FreezetimeEnded?.Invoke(this, new FreezetimeEndedEventArgs());
        }

        internal void RaisePlayerKilled(PlayerKilledEventArgs kill)
        {
            PlayerKilled?.Invoke(this, kill);
        }

        internal void RaisePlayerHurt(PlayerHurtEventArgs hurt)
        {
            PlayerHurt?.Invoke(this, hurt);
        }

        internal void RaiseBlind(BlindEventArgs blind)
        {
            Blind?.Invoke(this, blind);
        }

        internal void RaisePlayerBind(PlayerBindEventArgs bind)
        {
            PlayerBind?.Invoke(this, bind);
        }

        internal void RaisePlayerDisconnect(PlayerDisconnectEventArgs bind)
        {
            PlayerDisconnect?.Invoke(this, bind);
        }

        internal void RaisePlayerTeam(PlayerTeamEventArgs args)
        {
            PlayerTeam?.Invoke(this, args);
        }

        internal void RaiseBotTakeOver(BotTakeOverEventArgs take)
        {
            BotTakeOver?.Invoke(this, take);
        }

        internal void RaiseWeaponFired(WeaponFiredEventArgs fire)
        {
            WeaponFired?.Invoke(this, fire);
        }

        internal void RaiseSmokeStart(SmokeEventArgs args)
        {
            SmokeNadeStarted?.Invoke(this, args);
            NadeReachedTarget?.Invoke(this, args);
        }

        internal void RaiseSmokeEnd(SmokeEventArgs args)
        {
            SmokeNadeEnded?.Invoke(this, args);
        }

        internal void RaiseDecoyStart(DecoyEventArgs args)
        {
            DecoyNadeStarted?.Invoke(this, args);
            NadeReachedTarget?.Invoke(this, args);
        }

        internal void RaiseDecoyEnd(DecoyEventArgs args)
        {
            DecoyNadeEnded?.Invoke(this, args);
        }

        internal void RaiseFireStart(FireEventArgs args)
        {
            FireNadeStarted?.Invoke(this, args);
            NadeReachedTarget?.Invoke(this, args);
        }

        internal void RaiseFireWithOwnerStart(FireEventArgs args)
        {
            FireNadeWithOwnerStarted?.Invoke(this, args);
            NadeReachedTarget?.Invoke(this, args);
        }

        internal void RaiseFireEnd(FireEventArgs args)
        {
            FireNadeEnded?.Invoke(this, args);
        }

        internal void RaiseFlashExploded(FlashEventArgs args)
        {
            FlashNadeExploded?.Invoke(this, args);
            NadeReachedTarget?.Invoke(this, args);
        }

        internal void RaiseGrenadeExploded(GrenadeEventArgs args)
        {
            ExplosiveNadeExploded?.Invoke(this, args);
            NadeReachedTarget?.Invoke(this, args);
        }

        internal void RaiseBombBeginPlant(BombEventArgs args)
        {
            BombBeginPlant?.Invoke(this, args);
        }

        internal void RaiseBombAbortPlant(BombEventArgs args)
        {
            BombAbortPlant?.Invoke(this, args);
        }

        internal void RaiseBombPlanted(BombEventArgs args)
        {
            BombPlanted?.Invoke(this, args);
        }

        internal void RaiseBombDefused(BombEventArgs args)
        {
            BombDefused?.Invoke(this, args);
        }

        internal void RaiseBombExploded(BombEventArgs args)
        {
            BombExploded?.Invoke(this, args);
        }

        internal void RaiseBombBeginDefuse(BombDefuseEventArgs args)
        {
            BombBeginDefuse?.Invoke(this, args);
        }

        internal void RaiseBombAbortDefuse(BombDefuseEventArgs args)
        {
            BombAbortDefuse?.Invoke(this, args);
        }

        internal void RaiseSayText(SayTextEventArgs args)
        {
            SayText?.Invoke(this, args);
        }

        internal void RaiseSayText2(SayText2EventArgs args)
        {
            SayText2?.Invoke(this, args);
        }

        internal void RaiseRankUpdate(RankUpdateEventArgs args)
        {
            RankUpdate?.Invoke(this, args);
        }

        internal void RaisePlayerLeftBuyZone(PlayerLeftBuyZoneEventArgs args)
        {
            PlayerLeftBuyZone?.Invoke(this, args);
        }

        internal void RaisePlayerMoneyChange(PlayerMoneyChangedEventArgs args)
        {
            PlayerMoneyChanged?.Invoke(this, args);
        }

        internal void RaisePlayerPickWeapon(PlayerPickWeaponEventArgs args)
        {
            PlayerPickWeapon?.Invoke(this, args);
        }

        internal void RaisePlayerDropWeapon(PlayerDropWeaponEventArgs args)
        {
            PlayerDropWeapon?.Invoke(this, args);
        }

        internal void RaisePlayerBuyWeapon(PlayerBuyEventArgs args)
        {
            PlayerBuy?.Invoke(this, args);
        }

        internal void RaiseConVarChange(ConVarChangeEventArgs args)
        {
            ConVarChange?.Invoke(this, args);
        }

        internal void RaiseTeamScoreChange(TeamScoreChangeEventArgs args)
        {
            TeamScoreChange?.Invoke(this, args);
        }
        #endregion

        /// <summary>
        /// Releases all resource used by the <see cref="DemoParser"/> object. This must be called or evil things (memory leaks) happen.
        /// Sorry for that - I've debugged and I don't know why this is, but I can't fix it somehow.
        /// This is bad, I know.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="DemoParser"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="DemoParser"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the <see cref="DemoInfo.DemoParser"/> so the garbage
        /// collector can reclaim the memory that the <see cref="DemoParser"/> was occupying.</remarks>
        public void Dispose()
        {
            BitStream.Dispose();

            foreach (Entity entity in Entities)
            {
                if (entity != null)
                {
                    entity.Leave();
                }
            }

            foreach (ServerClass serverClass in SendTableParser.ServerClasses)
            {
                serverClass.Dispose();
            }

            TickDone = null;
            BombAbortDefuse = null;
            BombAbortPlant = null;
            BombBeginDefuse = null;
            BombBeginPlant = null;
            BombDefused = null;
            BombExploded = null;
            BombPlanted = null;
            DecoyNadeEnded = null;
            DecoyNadeStarted = null;
            ExplosiveNadeExploded = null;
            FireNadeEnded = null;
            FireNadeStarted = null;
            FireNadeWithOwnerStarted = null;
            FlashNadeExploded = null;
            HeaderParsed = null;
            MatchStarted = null;
            NadeReachedTarget = null;
            PlayerKilled = null;
            RoundStart = null;
            SmokeNadeEnded = null;
            SmokeNadeStarted = null;
            WeaponFired = null;
            PlayerLeftBuyZone = null;
            PlayerMoneyChanged = null;
            PlayerBuy = null;

            Players.Clear();
        }
    }
}
