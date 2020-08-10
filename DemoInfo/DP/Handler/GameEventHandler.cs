using DemoInfo.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DemoInfo.DP.Handler
{
    /// <summary>
    /// This class manages all GameEvents for a demo-parser.
    /// </summary>
    public static class GameEventHandler
    {
        public static void HandleGameEventList(IEnumerable<GameEventList.Descriptor> gel, DemoParser parser)
        {
            parser.GEH_Descriptors = new Dictionary<int, GameEventList.Descriptor>();
            foreach (GameEventList.Descriptor d in gel)
                parser.GEH_Descriptors[d.EventId] = d;
        }

        /// <summary>
        /// Apply the specified rawEvent to the parser.
        /// </summary>
        /// <param name="rawEvent">The raw event.</param>
        /// <param name="parser">The parser to mutate.</param>
        public static void Apply(GameEvent rawEvent, DemoParser parser)
        {
            Dictionary<int, GameEventList.Descriptor> descriptors = parser.GEH_Descriptors;

            if (descriptors == null)
                return;

            Dictionary<string, object> data;
            GameEventList.Descriptor eventDescriptor = descriptors[rawEvent.EventId];

            if (parser.Players.Count == 0 && eventDescriptor.Name != "player_connect")
                return;

            switch (eventDescriptor.Name)
            {
                case "round_start":
                    data = MapData(eventDescriptor, rawEvent);

                    parser.RaiseRoundStart(new RoundStartedEventArgs()
                    {
                        TimeLimit = (int)data["timelimit"],
                        FragLimit = (int)data["fraglimit"],
                        Objective = (string)data["objective"]
                    });
                    break;
                case "cs_win_panel_match":
                    parser.RaiseWinPanelMatch();
                    break;
                case "round_announce_final":
                    parser.RaiseRoundFinal();
                    break;
                case "round_announce_last_round_half":
                    parser.RaiseLastRoundHalf();
                    break;
                case "round_end":
                    HandleRoundEnd(rawEvent, parser, eventDescriptor);
                    break;
                case "round_officially_ended":
                    parser.RaiseRoundOfficiallyEnd();
                    break;
                case "round_mvp":
                    data = MapData(eventDescriptor, rawEvent);
                    parser.RaiseRoundMVP(new RoundMVPEventArgs
                    {
                        Player = PlayerFromPlayerID(parser, (int)data["userid"]),
                        Reason = (RoundMVPReason)data["reason"],
                    });

                    break;
                case "bot_takeover":
                    data = MapData(eventDescriptor, rawEvent);
                    parser.RaiseBotTakeOver(new BotTakeOverEventArgs
                    {
                        Taker = PlayerFromPlayerID(parser, (int)data["userid"]),
                    });

                    break;
                case "begin_new_match":
                    parser.RaiseMatchStarted();
                    break;
                case "round_announce_match_start":
                    parser.RaiseRoundAnnounceMatchStarted();
                    break;
                case "round_freeze_end":
                    parser.RaiseFreezetimeEnded();
                    break;
                case "weapon_fire":
                    HandleWeaponFire(rawEvent, parser, eventDescriptor);
                    break;
                case "player_death":
                    HandlePlayerDeath(rawEvent, parser, eventDescriptor);
                    break;
                case "player_hurt":
                    HandlePlayerHurt(rawEvent, parser, eventDescriptor);
                    break;
                case "player_blind":
                    HandlePlayerBlind(rawEvent, parser, eventDescriptor);
                    break;
                case "flashbang_detonate":
                    parser.RaiseFlashExploded(FillNadeEvent<FlashEventArgs>(MapData(eventDescriptor, rawEvent), parser));
                    break;
                case "hegrenade_detonate":
                    parser.RaiseGrenadeExploded(FillNadeEvent<GrenadeEventArgs>(MapData(eventDescriptor, rawEvent), parser));
                    break;
                case "decoy_started":
                    parser.RaiseDecoyStart(FillNadeEvent<DecoyEventArgs>(MapData(eventDescriptor, rawEvent), parser));
                    break;
                case "decoy_detonate":
                    parser.RaiseDecoyEnd(FillNadeEvent<DecoyEventArgs>(MapData(eventDescriptor, rawEvent), parser));
                    break;
                case "smokegrenade_detonate":
                    parser.RaiseSmokeStart(FillNadeEvent<SmokeEventArgs>(MapData(eventDescriptor, rawEvent), parser));
                    break;
                case "smokegrenade_expired":
                    parser.RaiseSmokeEnd(FillNadeEvent<SmokeEventArgs>(MapData(eventDescriptor, rawEvent), parser));
                    break;
                case "inferno_startburn":
                    Dictionary<string, object> fireData = MapData(eventDescriptor, rawEvent);
                    FireEventArgs fireArgs = FillNadeEvent<FireEventArgs>(fireData, parser);
                    Tuple<int, FireEventArgs> fireStarted = new Tuple<int, FireEventArgs>((int)fireData["entityid"], fireArgs);
                    parser.GEH_StartBurns.Enqueue(fireStarted);
                    parser.RaiseFireStart(fireArgs);
                    break;
                case "inferno_expire":
                    Dictionary<string, object> fireEndData = MapData(eventDescriptor, rawEvent);
                    FireEventArgs fireEndArgs = FillNadeEvent<FireEventArgs>(fireEndData, parser);
                    int entityID = (int)fireEndData["entityid"];
                    fireEndArgs.ThrownBy = parser.InfernoOwners[entityID];
                    parser.RaiseFireEnd(fireEndArgs);
                    break;
                case "player_connect":
                    HandlePlayerConnect(rawEvent, parser, eventDescriptor);
                    break;
                case "player_disconnect":
                    HandlePlayerDisconnect(rawEvent, parser, eventDescriptor);
                    break;
                case "player_team":
                    HandlePlayerTeam(rawEvent, parser, eventDescriptor);
                    break;
                case "bomb_beginplant": //When the bomb is starting to get planted
                case "bomb_abortplant": //When the bomb planter stops planting the bomb
                case "bomb_planted": //When the bomb has been planted
                case "bomb_defused": //When the bomb has been defused
                case "bomb_exploded": //When the bomb has exploded
                    HandleBomb(rawEvent, parser, eventDescriptor);
                    break;
                case "bomb_begindefuse":
                    data = MapData(eventDescriptor, rawEvent);
                    parser.RaiseBombBeginDefuse(new BombDefuseEventArgs
                    {
                        Player = PlayerFromPlayerID(parser, (int)data["userid"]),
                        HasKit = (bool)data["haskit"],
                    });
                    break;
                case "bomb_abortdefuse":
                    HandleAbortDefuse(rawEvent, parser, eventDescriptor);
                    break;
            }

            // @TODO: player jump

            //if (eventDescriptor.Name != "player_footstep") {
            //	Console.WriteLine (eventDescriptor.Name);
            //}
        }

        private static void HandleBomb(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);
            var bombEventArgs = new BombEventArgs
            {
                Player = PlayerFromPlayerID(parser, (int)data["userid"]),
            };

            int site = (int)data["site"];
            if (site == parser.bombsiteAIndex)
            {
                bombEventArgs.Site = 'A';
            }
            else if (site == parser.bombsiteBIndex)
            {
                bombEventArgs.Site = 'B';
            }
            else
            {
                var relevantTrigger = parser.triggers.Single(a => a.Index == site);
                if (relevantTrigger.Contains(parser.bombsiteACenter))
                {
                    bombEventArgs.Site = 'A';
                    parser.bombsiteAIndex = site;
                }
                else
                {
                    bombEventArgs.Site = 'B';
                    parser.bombsiteBIndex = site;
                }
            }

            switch (eventDescriptor.Name)
            {
                case "bomb_beginplant":
                    parser.RaiseBombBeginPlant(bombEventArgs);
                    break;
                case "bomb_abortplant":
                    parser.RaiseBombAbortPlant(bombEventArgs);
                    break;
                case "bomb_planted":
                    parser.RaiseBombPlanted(bombEventArgs);
                    break;
                case "bomb_defused":
                    parser.RaiseBombDefused(bombEventArgs);
                    break;
                case "bomb_exploded":
                    parser.RaiseBombExploded(bombEventArgs);
                    break;
            }
        }

        private static void HandlePlayerDisconnect(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);
            PlayerDisconnectEventArgs disconnect = new PlayerDisconnectEventArgs
            {
                Player = PlayerFromPlayerID(parser, (int)data["userid"]),
            };

            parser.RaisePlayerDisconnect(disconnect);

            int toDelete = (int)data["userid"];
            for (int i = 0; i < parser.RawPlayers.Length; i++)
            {
                if (parser.RawPlayers[i] != null && parser.RawPlayers[i].UserID == toDelete)
                {
                    parser.RawPlayers[i] = null;
                    break;
                }
            }

            if (parser.Players.ContainsKey(toDelete))
            {
                parser.Players.Remove(toDelete);
            }
        }

        private static void HandlePlayerConnect(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);
            PlayerInfo player = new PlayerInfo
            {
                UserID = (int)data["userid"],
                Name = (string)data["name"],
                GUID = (string)data["networkid"],
                XUID = (string)data["networkid"] == "BOT" ? 0 : GetCommunityID((string)data["networkid"]),
                //IsFakePlayer = (bool)data["bot"],
            };

            int index = (int)data["index"];
            parser.RawPlayers[index] = player;
        }

        private static void HandlePlayerHurt(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);
            PlayerHurtEventArgs hurt = new PlayerHurtEventArgs
            {
                Player = parser.Players.ContainsKey((int)data["userid"]) ? parser.Players[(int)data["userid"]] : null,
                Attacker = parser.Players.ContainsKey((int)data["attacker"]) ? parser.Players[(int)data["attacker"]] : null,
                Health = (int)data["health"],
                Armor = (int)data["armor"],
                HealthDamage = (int)data["dmg_health"],
                ArmorDamage = (int)data["dmg_armor"],
                Hitgroup = (Hitgroup)((int)data["hitgroup"]),
                Weapon = new Equipment((string)data["weapon"], "")
            };

            if (hurt.Attacker != null && hurt.Weapon.Class != EquipmentClass.Grenade && hurt.Attacker.Weapons.Any())
            {
                hurt.Weapon = hurt.Attacker.ActiveWeapon;
            }

            parser.RaisePlayerHurt(hurt);
        }

        private static void HandleAbortDefuse(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);
            Player player = PlayerFromPlayerID(parser, (int)data["userid"]);
            parser.RaiseBombAbortDefuse(new BombDefuseEventArgs
            {
                Player = player,
                HasKit = player.HasDefuseKit
            });
        }

        private static void HandlePlayerBlind(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);
            if (!parser.Players.ContainsKey((int)data["userid"])) return;

            Player blindPlayer = PlayerFromPlayerID(parser, (int)data["userid"]);
            if (blindPlayer != null && blindPlayer.Team != Team.Spectate)
            {
                parser.RaiseBlind(new BlindEventArgs
                {
                    Player = blindPlayer,
                    Attacker = data.ContainsKey("attacker") ? PlayerFromPlayerID(parser, (int)data["attacker"]) : null,
                    FlashDuration = data.ContainsKey("blind_duration") ? (float?)data["blind_duration"] : null,
                });
            }
        }

        private static void HandlePlayerDeath(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);
            PlayerKilledEventArgs kill = new PlayerKilledEventArgs
            {
                Victim = PlayerFromPlayerID(parser, (int)data["userid"]),
                Killer = PlayerFromPlayerID(parser, (int)data["attacker"]),
                Assister = PlayerFromPlayerID(parser, (int)data["assister"]),
                Headshot = (bool)data["headshot"],
                Weapon = new Equipment((string)data["weapon"], (string)data["weapon_itemid"]),
                PenetratedObjects = (int)data["penetrated"],
                // @TODO: Handle new noscope/smoke/etc?
            };

            if (data.ContainsKey("assistedflash"))
                kill.AssistedFlash = (bool)data["assistedflash"];

            if (kill.Killer != null
                && kill.Weapon.Class != EquipmentClass.Grenade
                && kill.Weapon.Weapon != EquipmentElement.Revolver
                && kill.Weapon.Weapon != EquipmentElement.World
                && kill.Killer.Weapons.Any()
            )
            {
                /*
                #if DEBUG
                if (kill.Weapon.Weapon != kill.Killer.ActiveWeapon.Weapon)
                    throw new InvalidDataException();
                #endif
                */
                kill.Weapon = kill.Killer.ActiveWeapon;
            }

            parser.RaisePlayerKilled(kill);
        }

        private static void HandleWeaponFire(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);
            WeaponFiredEventArgs fire = new WeaponFiredEventArgs
            {
                Shooter = PlayerFromPlayerID(parser, (int)data["userid"]),
                Weapon = new Equipment((string)data["weapon"])
            };

            if (fire.Shooter != null && fire.Shooter.ActiveWeapon != null && fire.Weapon.Class != EquipmentClass.Grenade)
            {
                fire.Weapon = fire.Shooter.ActiveWeapon;
            }

            // @TODO: Grenade throw event?

            parser.RaiseWeaponFired(fire);
        }

        private static Team TeamFromTeamID(DemoParser parser, int teamID)
        {
            Team t = Team.Spectate;
            if (teamID == parser.tID)
                t = Team.Terrorist;
            else if (teamID == parser.ctID)
                t = Team.CounterTerrorist;

            return t;
        }

        private static Player PlayerFromPlayerID(DemoParser parser, int playerID)
        {
            return parser.Players.ContainsKey(playerID) ? parser.Players[playerID] : null;

            /*
            Victim = parser.Players.ContainsKey((int)data["userid"]) ? parser.Players[(int)data["userid"]] : null,
                Killer = parser.Players.ContainsKey((int)data["attacker"]) ? parser.Players[(int)data["attacker"]] : null,
                Assister = parser.Players.ContainsKey((int)data["assister"]) ? parser.Players[(int)data["assister"]] : null
            */
        }


        private static void HandlePlayerTeam(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);

            PlayerTeamEventArgs playerTeamEvent = new PlayerTeamEventArgs
            {
                OldTeam = TeamFromTeamID(parser, (int)data["oldteam"]),
                NewTeam = TeamFromTeamID(parser, (int)data["team"]),
                Swapped = PlayerFromPlayerID(parser, (int)data["userid"]),
                IsBot = (bool)data["isbot"],
                Silent = (bool)data["silent"]
            };

            parser.RaisePlayerTeam(playerTeamEvent);
        }

        private static void HandleRoundEnd(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);

            RoundEndedEventArgs roundEnd = new RoundEndedEventArgs()
            {
                Reason = (RoundEndReason)data["reason"],
                Winner = TeamFromTeamID(parser, (int)data["winner"]),
                Message = (string)data["message"],
            };

            parser.RaiseRoundEnd(roundEnd);
        }

        private static T FillNadeEvent<T>(Dictionary<string, object> data, DemoParser parser) where T : NadeEventArgs, new()
        {
            var nade = new T();

            if (data.ContainsKey("userid") && parser.Players.ContainsKey((int)data["userid"]))
                nade.ThrownBy = parser.Players[(int)data["userid"]];

            nade.Position = new Vector
            {
                X = (float)data["x"],
                Y = (float)data["y"],
                Z = (float)data["z"],
            };

            return nade;
        }

        private static Dictionary<string, object> MapData(GameEventList.Descriptor eventDescriptor, GameEvent rawEvent)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();

            for (int i = 0; i < eventDescriptor.Keys.Length; i++)
                data.Add(eventDescriptor.Keys[i].Name, rawEvent.Keys[i]);

            return data;
        }

        private static long GetCommunityID(string steamID)
        {
            long authServer = Convert.ToInt64(steamID.Substring(8, 1));
            long authID = Convert.ToInt64(steamID.Substring(10));
            return (76561197960265728 + (authID * 2) + authServer);
        }
    }
}
