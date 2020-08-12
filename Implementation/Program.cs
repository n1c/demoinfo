using System;
using System.IO;
using System.Threading;
using DemoInfo;

namespace Implementation
{
    class MainClass
    {
        private static DemoParser _parser;

        public static void Main()
        {
            Console.WriteLine("Implementation booting");
            string path = "../../../../_demos/triumph-vs-teamone-m1-dust2.dem";
            if (!File.Exists(path))
            {
                Console.WriteLine("Demo doesn't exist!: " + path);
                Environment.Exit(1);
            }

            FileStream input = File.OpenRead(path);
            _parser = new DemoParser(input);
            _parser.HeaderParsed += (object sender, HeaderParsedEventArgs e) =>
            {
                Console.WriteLine($"Header parsed! {_parser.Map}, Frames:{_parser.Header.PlaybackFrames}, Ticks: {_parser.Header.PlaybackTicks}");
            };

            _parser.TickDone += (object sender, TickDoneEventArgs e) =>
            {
                if (_parser.CurrentTick % 10000 == 0)
                {
                    Console.WriteLine($"Progress: {Math.Floor(_parser.ParsingProgess * 100)}%");
                }
            };

            /*
            _parser.WeaponFired += (object sender, WeaponFiredEventArgs e) =>
            {
                Console.WriteLine($"WeaponFired! {e.Shooter.Name} ({e.Weapon.Weapon}) {e.Shooter.Position}");
            };
            */

            _parser.ParseHeader();
            _parser.ParseToEnd(CancellationToken.None);
            Console.WriteLine("FINISHED");
        }
    }
}