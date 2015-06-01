using System;
using System.IO;
using System.Threading;
using CommandLine;
using EmgTools.IO.OlimexShield;

namespace EmgTools.EmgLogger
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CommandLineOptions opts = new CommandLineOptions();

            if (Parser.Default.ParseArguments(args, opts))
            {
                var shield = new OlimexEkgEmgShield(opts.Port);

                using (var outFile = File.Open(opts.FileName, FileMode.Create))
                using (var writer = new StreamWriter(outFile))
                {
                    writer.WriteLine("Epoch, S1, S2, S3, S4, S5, S6");
                    shield.DataReceived += (o, e) =>
                    {
                        writer.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}", e.Epoch, e.Message[0], e.Message[1], e.Message[2], e.Message[3], e.Message[4], e.Message[5]);
                    };

                    shield.Open();

                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                    shield.Close();
                }
            }
           
        }
    }
}