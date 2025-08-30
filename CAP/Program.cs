using System;
using EA_DB_Editor;

namespace CAP
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                var values = (ProfileId[])Enum.GetValues(typeof(ProfileId));
                foreach( var value in values)
                {
                    Console.WriteLine($"{value}");
                }
                return;
            }

            if (args.Length == 2)
            {
                var  a=CAPGen.GetPlayer(Enum.Parse<ProfileId>(args[0]));
                var b = CAPGen.GetPlayer(Enum.Parse<ProfileId>(args[1]));
                a.Combine(b);
                a.Show();
                return;
            }

            foreach (var arg in args)
            {
                var value = Enum.Parse<ProfileId>(arg);
               var player= CAPGen.GetPlayer(value);
                player.Show();
            }
        }
    }
}
