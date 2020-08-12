﻿using System;
using System.IO;
using System.Threading;
using DemoInfo;

namespace Implementation
{
    class Program
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

            FileStream input = File.OpenRead(args[0]);
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