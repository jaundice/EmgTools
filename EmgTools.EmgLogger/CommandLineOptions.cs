using System.IO.Ports;
using CommandLine;
using CommandLine.Text;

namespace EmgTools.EmgLogger
{
    internal class CommandLineOptions
    {
        [Option('o', "OutputFile", Required = true, HelpText = "Path to output file. File will be overwritten")]
        public string FileName { get; set; }
        [Option('p', "Port", Required = true, HelpText = "Serial port to use.")]
        public string Port { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return string.Format("{0}\r\nAvailable Serial Ports:\r\n{1}", HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current)), string.Join("\r\n", SerialPort.GetPortNames()));
        }
    }
}