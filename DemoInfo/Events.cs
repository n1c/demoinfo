using System;

namespace DemoInfo
{
    public class HeaderParsedEventArgs : EventArgs
    {
        public DemoHeader Header { get; private set; }

        public HeaderParsedEventArgs(DemoHeader header)
        {
            Header = header;
        }
    }

    public class TickDoneEventArgs : EventArgs
    {
        public int CurrentTick { get; set; }
        public float ParsingProgress { get; set; }
    }

    public class MatchStartedEventArgs : EventArgs
    {
    }

    public class RoundAnnounceMatchStartedEventArgs : EventArgs
    {
    }

    public class RoundEndedEventArgs : EventArgs
    {
        public RoundEndReason Reason { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// The winning team. Spectate for everything that isn't CT or T.
        /// </summary>
        public Team Winner;
    }

    public class RoundOfficiallyEndedEventArgs : EventArgs
    {
    }

    public class RoundMVPEventArgs : EventArgs
    {
        public Player Player { get; set; }
        public RoundMVPReason Reason { get; set; }
    }

    public class RoundStartedEventArgs : EventArgs
    {
        public int TimeLimit { get; set; }
        public int FragLimit { get; set; }
        public string Objective { get; set; }
    }

    public class WinPanelMatchEventArgs : EventArgs
    {
    }

    public class RoundFinalEventArgs : EventArgs
    {
    }

    public class LastRoundHalfEventArgs : EventArgs
    {
    }

    public class FreezetimeEndedEventArgs : EventArgs
    {
    }

    public class PlayerTeamEventArgs : EventArgs
    {
        public Player Swapped { get; internal set; }
        public Team NewTeam { get; internal set; }
        public Team OldTeam { get; internal set; }
        public bool Silent { get; internal set; }
        public bool IsBot { get; internal set; }
    }

    public class PlayerKilledEventArgs : EventArgs
    {
        public Equipment Weapon { get; internal set; }
        public Player Victim { get; internal set; }
        public Player Killer { get; internal set; }
        public Player Assister { get; internal set; }
        public int PenetratedObjects { get; internal set; }
        public bool Headshot { get; internal set; }
        public bool AttackerBlind { get; internal set; }
        public bool NoScope { get; internal set; }
        public bool ThroughSmoke { get; internal set; }
        public bool AssistedFlash { get; internal set; }
    }

    public class BotTakeOverEventArgs : EventArgs
    {
        public Player Taker { get; internal set; }
    }

    public class WeaponFiredEventArgs : EventArgs
    {
        public Equipment Weapon { get; internal set; }
        public Player Shooter { get; internal set; }
    }

    public class NadeEventArgs : EventArgs
    {
        public Vector Position { get; internal set; }
        public EquipmentElement NadeType { get; internal set; }
        public Player ThrownBy { get; internal set; }

        internal NadeEventArgs()
        {
        }

        internal NadeEventArgs(EquipmentElement type)
        {
            NadeType = type;
        }
    }

    public class FireEventArgs : NadeEventArgs
    {
        public FireEventArgs() : base(EquipmentElement.Incendiary)
        {
        }
    }

    public class SmokeEventArgs : NadeEventArgs
    {
        public SmokeEventArgs() : base(EquipmentElement.Smoke)
        {
        }
    }

    public class DecoyEventArgs : NadeEventArgs
    {
        public DecoyEventArgs() : base(EquipmentElement.Decoy)
        {
        }
    }

    public class FlashEventArgs : NadeEventArgs
    {
        public FlashEventArgs() : base(EquipmentElement.Flash)
        {
        }
    }

    public class GrenadeEventArgs : NadeEventArgs
    {
        public GrenadeEventArgs() : base(EquipmentElement.HE)
        {
        }
    }

    public class BombEventArgs : EventArgs
    {
        public Player Player { get; set; }
        public char Site { get; set; }
    }

    public class BombDefuseEventArgs : EventArgs
    {
        public Player Player { get; set; }
        public bool HasKit { get; set; }
    }

    public class PlayerHurtEventArgs : EventArgs
    {
        /// <summary>
        /// The hurt player
        /// </summary>
        public Player Player { get; set; }

        /// <summary>
        /// The attacking player
        /// </summary>
        public Player Attacker { get; set; }

        /// <summary>
        /// Remaining health points of the player
        /// </summary>
        public int Health { get; set; }

        /// <summary>
        /// Remaining armor points of the player
        /// </summary>
        public int Armor { get; set; }

        /// <summary>
        /// The Weapon used to attack.
        /// Note: This might be not the same as the raw event
        /// we replace "hpk2000" with "usp-s" if the attacker
        /// is currently holding it - this value is originally
        /// networked "wrong". By using this property you always
        /// get the "right" weapon
        /// </summary>
        /// <value>The weapon.</value>
        public Equipment Weapon { get; set; }

        /// <summary>
        /// The original "weapon"-value from the event.
        /// Might be wrong for USP, CZ and M4A1-S
        /// </summary>
        /// <value>The weapon string.</value>
        public string WeaponString { get; set; }

        /// <summary>
        /// The damage done to the players health
        /// </summary>
        public int HealthDamage { get; set; }

        /// <summary>
        /// The damage done to the players armor
        /// </summary>
        public int ArmorDamage { get; set; }

        /// <summary>
        /// Where the Player was hit.
        /// </summary>
        /// <value>The hitgroup.</value>
        public Hitgroup Hitgroup { get; set; }
    }

    public class BlindEventArgs : EventArgs
    {
        public Player Player { get; set; }
        public Player Attacker { get; set; }
        public float? FlashDuration { get; set; }
    }

    public class PlayerBindEventArgs : EventArgs
    {
        public Player Player { get; set; }
    }

    public class PlayerDisconnectEventArgs : EventArgs
    {
        public Player Player { get; set; }
    }

    /// <summary>
    /// Occurs when the server use the "say" command
    /// I don't know the purpose of IsChat and IsChatAll because they are everytime false
    /// </summary>
    public class SayTextEventArgs : EventArgs
    {
        /// <summary>
        /// Should be everytime 0 as it's a message from the server
        /// </summary>
        public int EntityIndex { get; set; }

        /// <summary>
        /// Message sent by the server
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Everytime false as the message is public
        /// </summary>
        public bool IsChat { get; set; }

        /// <summary>
        /// Everytime false as the message is public
        /// </summary>
        public bool IsChatAll { get; set; }
    }

    /// <summary>
    /// Occurs when a player use the say command
    /// Not sure about IsChat and IsChatAll, GOTV doesn't record chat team so this 2 bool are every time true
    /// </summary>
    public class SayText2EventArgs : EventArgs
    {
        /// <summary>
        /// The player who sent the message
        /// </summary>
        public Player Sender { get; set; }

        /// <summary>
        /// The message sent
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Not sure about it, maybe it's to indicate say_team or say
        /// </summary>
        public bool IsChat { get; set; }

        /// <summary>
        /// true if the message is for all players ?
        /// </summary>
        public bool IsChatAll { get; set; }
    }

    /// <summary>
    /// Occurs when the server display a player rank
    /// It occurs only with Valve demos, at the end of a Matchmaking.
    /// So for a 5v5 match there will be 10 events trigerred
    /// </summary>
    public class RankUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Player's SteamID64
        /// </summary>
        public long SteamId { get; set; }

        /// <summary>
        /// Player's rank at the beginning of the match
        /// </summary>
        public int RankOld { get; set; }

        /// <summary>
        /// Player's rank the end of the match
        /// </summary>
        public int RankNew { get; set; }

        /// <summary>
        /// Number of win that the player have
        /// </summary>
        public int WinCount { get; set; }

        /// <summary>
        /// Number of rank the player win / lost between the beggining and the end of the match
        /// </summary>
        public float RankChange { get; set; }
    }
}
