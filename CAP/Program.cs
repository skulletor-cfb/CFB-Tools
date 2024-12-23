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

            foreach(var arg in args)
            {
                var value = Enum.Parse<ProfileId>(arg);
                CAPGen.GetPlayer(value);
            }
        }
    }
}
