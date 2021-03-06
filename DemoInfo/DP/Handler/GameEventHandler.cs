﻿using System;
using System.Collections.Generic;
using System.Linq;

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
            {
                parser.GEH_Descriptors[d.EventId] = d;
            }
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
            {
                return;
            }

            Dictionary<string, object> data;
            GameEventList.Descriptor eventDescriptor = descriptors[rawEvent.EventId];

            if (parser.Players.Count == 0 && eventDescriptor.Name != "player_connect")
            {
                return;
            }

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
                        Player = parser.PlayerFromPlayerID((int)data["userid"]),
                        Reason = (RoundMVPReason)data["reason"],
                    });

                    break;
                case "bot_takeover":
                    data = MapData(eventDescriptor, rawEvent);
                    parser.RaiseBotTakeOver(new BotTakeOverEventArgs
                    {
                        Taker = parser.PlayerFromPlayerID((int)data["userid"]),
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
                    fireEndArgs.ThrownBy = parser.InfernoOwners[(int)fireEndData["entityid"]];
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
                case "bomb_beginplant":
                case "bomb_abortplant":
                case "bomb_planted":
                case "bomb_defused":
                case "bomb_exploded":
                    HandleBomb(rawEvent, parser, eventDescriptor);
                    break;
                case "bomb_begindefuse":
                    data = MapData(eventDescriptor, rawEvent);
                    parser.RaiseBombBeginDefuse(new BombDefuseEventArgs
                    {
                        Player = parser.PlayerFromPlayerID((int)data["userid"]),
                        HasKit = (bool)data["haskit"],
                    });
                    break;
                case "bomb_abortdefuse":
                    HandleAbortDefuse(rawEvent, parser, eventDescriptor);
                    break;
                case "announce_phase_end":
                case "bomb_dropped":
                case "bomb_pickup":
                case "buytime_ended":
                case "cs_pre_restart":
                case "cs_round_start_beep":
                case "cs_round_final_beep":
                case "cs_win_panel_round":
                case "endmatch_cmm_start_reveal_items":
                case "hltv_chase":
                case "hltv_fixed":
                case "hltv_status":
                case "item_equip":
                case "item_pickup":
                case "item_remove":
                case "other_death":
                case "player_connect_full":
                case "player_falldamage":
                case "player_footstep":
                case "player_spawn":
                case "player_jump":
                case "round_announce_warmup":
                case "round_announce_match_point":
                case "round_time_warning":
                case "round_prestart":
                case "round_poststart":
                case "server_cvar":
                case "weapon_reload":
                case "weapon_zoom":
                    // NOOP
                    break;
                default:
                    Console.WriteLine("Unhandled: " + eventDescriptor.Name);
                    break;
            }
        }

        private static void HandleBomb(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);
            BombEventArgs bombEventArgs = new BombEventArgs
            {
                Player = parser.PlayerFromPlayerID((int)data["userid"]),
            };

            int site = (int)data["site"];
            if (site == parser.BombsiteAIndex)
            {
                bombEventArgs.Site = 'A';
            }
            else if (site == parser.BombsiteBIndex)
            {
                bombEventArgs.Site = 'B';
            }
            else
            {
                BoundingBoxInformation relevantTrigger = parser.triggers.Single(a => a.Index == site);
                if (relevantTrigger.Contains(parser.BombsiteACenter))
                {
                    bombEventArgs.Site = 'A';
                    parser.BombsiteAIndex = site;
                }
                else
                {
                    bombEventArgs.Site = 'B';
                    parser.BombsiteBIndex = site;
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

            parser.RaisePlayerDisconnect(new PlayerDisconnectEventArgs
            {
                Player = parser.PlayerFromPlayerID((int)data["userid"]),
                Reason = (string)data["reason"],
            });

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
                _ = parser.Players.Remove(toDelete);
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
                // IsFakePlayer = (bool)data["bot"],
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
                Hitgroup = (Hitgroup)(int)data["hitgroup"],
                Weapon = new Equipment((string)data["weapon"]),
            };

            if (hurt.Attacker != null && hurt.Attacker.ActiveWeapon != null)
            {
                // In case of grenade attacks, attacker's active weapon is not his grenade at this state
                if (hurt.Weapon == null || (hurt.Weapon != null && hurt.Weapon.Class != EquipmentClass.Grenade))
                {
                    hurt.Weapon = hurt.Attacker.ActiveWeapon;
                }
            }

            parser.RaisePlayerHurt(hurt);
        }

        private static void HandleAbortDefuse(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);
            Player player = parser.PlayerFromPlayerID((int)data["userid"]);
            parser.RaiseBombAbortDefuse(new BombDefuseEventArgs
            {
                Player = player,
                HasKit = player.HasDefuseKit
            });
        }

        private static void HandlePlayerBlind(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);
            if (!parser.Players.ContainsKey((int)data["userid"]))
            {
                return;
            }

            Player blindPlayer = parser.PlayerFromPlayerID((int)data["userid"]);
            if (blindPlayer != null && blindPlayer.Team != Team.Spectate)
            {
                parser.RaiseBlind(new BlindEventArgs
                {
                    Player = blindPlayer,
                    Attacker = data.ContainsKey("attacker") ? parser.PlayerFromPlayerID((int)data["attacker"]) : null,
                    FlashDuration = data.ContainsKey("blind_duration") ? (float?)data["blind_duration"] : null,
                });
            }
        }

        private static void HandlePlayerDeath(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);
            PlayerKilledEventArgs kill = new PlayerKilledEventArgs
            {
                Victim = parser.PlayerFromPlayerID((int)data["userid"]),
                Killer = parser.PlayerFromPlayerID((int)data["attacker"]),
                Assister = parser.PlayerFromPlayerID((int)data["assister"]),
                Weapon = new Equipment((string)data["weapon"], (string)data["weapon_itemid"]),
                PenetratedObjects = (int)data["penetrated"],
                Headshot = (bool)data["headshot"],
                AttackerBlind = (bool)data["attackerblind"],
                NoScope = (bool)data["noscope"],
                ThroughSmoke = (bool)data["thrusmoke"],
            };

            if (data.ContainsKey("assistedflash"))
            {
                kill.AssistedFlash = (bool)data["assistedflash"];
            }

            if (kill.Killer != null && kill.Killer.ActiveWeapon != null)
            {
                // In case of grenade kills, killer's active weapon is not his grenade at this state
                if (kill.Weapon == null || (kill.Weapon != null && kill.Weapon.Class != EquipmentClass.Grenade))
                {
                    kill.Weapon = kill.Killer.ActiveWeapon;
                    kill.Weapon.SkinID = (string)data["weapon_itemid"];
                }
            }

            parser.RaisePlayerKilled(kill);
        }

        private static void HandleWeaponFire(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);
            WeaponFiredEventArgs fire = new WeaponFiredEventArgs
            {
                Shooter = parser.PlayerFromPlayerID((int)data["userid"]),
                Weapon = new Equipment((string)data["weapon"])
            };

            if (fire.Shooter != null && fire.Shooter.ActiveWeapon != null)
            {
                fire.Weapon = fire.Shooter.ActiveWeapon;
            }
            else
            {
                // Should not happen?
                fire.Weapon = new Equipment((string)data["weapon"]);
            }

            parser.RaiseWeaponFired(fire);
        }

        private static void HandlePlayerTeam(GameEvent rawEvent, DemoParser parser, GameEventList.Descriptor eventDescriptor)
        {
            Dictionary<string, object> data = MapData(eventDescriptor, rawEvent);

            PlayerTeamEventArgs playerTeamEvent = new PlayerTeamEventArgs
            {
                OldTeam = parser.TeamFromTeamID((int)data["oldteam"]),
                NewTeam = parser.TeamFromTeamID((int)data["team"]),
                Swapped = parser.PlayerFromPlayerID((int)data["userid"]),
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
                Winner = parser.TeamFromTeamID((int)data["winner"]),
                Message = (string)data["message"],
            };

            parser.RaiseRoundEnd(roundEnd);
        }

        private static T FillNadeEvent<T>(Dictionary<string, object> data, DemoParser parser) where T : NadeEventArgs, new()
        {
            T nade = new T();

            int entityID = (int)data["entityid"];
            if (parser.Projectiles[entityID] != null)
            {
                Projectile nadeEntity = parser.Projectiles[entityID];
                nade.ThrownBy = nadeEntity.Owner;
            }
            // Is this userid - 1?
            else if (data.ContainsKey("userid") && parser.Players.ContainsKey((int)data["userid"]))
            {
                nade.ThrownBy = parser.Players[(int)data["userid"]];
            }
            else if (typeof(T).Equals(typeof(FireEventArgs)))
            {
                if (parser.InfernoOwners.ContainsKey(entityID))
                {
                    nade.ThrownBy = parser.InfernoOwners[entityID];
                }
            }
            /*
            else if (parser.Entities[(int)data["entityid"]] != null)
            {
                Console.WriteLine("Parser Entity exists " + parser.Entities[(int)data["entityid"]]);
            }
            else
            {
                Console.WriteLine("No thrower for " + typeof(T) + " :: " + entityID);
            }
            */

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
            {
                data.Add(eventDescriptor.Keys[i].Name, rawEvent.Keys[i]);
            }

            return data;
        }

        private static long GetCommunityID(string steamID)
        {
            long authServer = Convert.ToInt64(steamID.Substring(8, 1));
            long authID = Convert.ToInt64(steamID.Substring(10));
            return 76561197960265728 + (authID * 2) + authServer;
        }
    }
}
