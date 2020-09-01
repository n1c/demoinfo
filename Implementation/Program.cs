using System;
using System.IO;
using System.Threading.Tasks;
using DemoInfo;

namespace Implementation
{
    internal class Program
    {
        private static DemoParser _parser;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide a path as first param.");
                Environment.Exit(1);
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Demo doesn't exist!: " + args[0]);
                Environment.Exit(1);
            }

            Console.WriteLine("Going to parse: " + args[0]);

            FileStream input = File.OpenRead(args[0]);
            _parser = new DemoParser(input);
            _parser.HeaderParsed += HeaderParsed;

            _parser.TickDone += TickDone;
            // _parser.PlayerKilled += PlayerKilled;
            // _parser.WeaponFired += WeaponFired;

            _parser.ParseHeader();
            Task t = _parser.ParseToEnd();
            // _parser.CancelParsing();
            t.Wait();

            Console.WriteLine("FINISHED");
        }

        private static void HeaderParsed(object sender, HeaderParsedEventArgs e)
        {
            Console.WriteLine($"Header parsed! {e.Header.MapName}, Frames:{e.Header.PlaybackFrames}, Ticks: {e.Header.PlaybackTicks}");
        }

        private static void TickDone(object sender, TickDoneEventArgs e)
        {
            if (e.CurrentTick % 10000 == 0)
            {
                Console.WriteLine($"Progress: {Math.Floor(e.ParsingProgress * 100)}%");
            }
        }

        private static void PlayerKilled(object sender, PlayerKilledEventArgs e)
        {
            if (e.ThroughSmoke || e.NoScope || e.AttackerBlind)
            {
                Console.WriteLine($"PlayerKilled ThroughSmoke:{e.ThroughSmoke} NoScope:{e.NoScope} AttackerBlind:{e.AttackerBlind}");
            }
        }

        private static void WeaponFired(object sender, WeaponFiredEventArgs e)
        {
            Console.WriteLine($"WeaponFired! {e.Shooter.Name} ({e.Weapon.Weapon}) {e.Shooter.Position}");
        }
    }
}