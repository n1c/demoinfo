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
            Console.WriteLine("Implementation booting. " + Directory.GetCurrentDirectory());
            string path = "triumph-vs-teamone-m1-dust2.dem";
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("That demo path doesn't exist! " + path);
            }

            FileStream input = File.OpenRead(path);
            _parser = new DemoParser(input);
            _parser.HeaderParsed += (object sender, HeaderParsedEventArgs e) =>
            {
                Console.WriteLine("Header parsed! Map is: " + _parser.Map);
            };

            _parser.WeaponFired += (object sender, WeaponFiredEventArgs e) =>
            {
                Console.WriteLine($"WeaponFired! {e.Shooter.Name} ({e.Weapon.Weapon}) {e.Shooter.Position}");
                // source.CancelAfter();
            };

            _parser.ParseHeader();
            _parser.ParseToEnd(CancellationToken.None);
            Console.WriteLine("FINISHED");
        }
    }
}