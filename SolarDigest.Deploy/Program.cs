using SolarDigest.Deploy.Stacks;
using System;

namespace SolarDigest.Deploy
{
    class Program
    {
        private const string CommandLineMessage = "Must pass '--data' or '--service' as a command line argument";

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine(CommandLineMessage);
                return;
            }

            var app = args[0].ToLower() switch
            {
                "--data" => DataStack.CreateApp(),
                "--service" => ServiceStack.CreateApp(),
                _ => null
            };

            if (app == null)
            {
                Console.WriteLine(CommandLineMessage);
                return;
            }

            _ = app.Synth();
        }
    }
}
