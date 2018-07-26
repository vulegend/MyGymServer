using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Description;

namespace HookappServer
{
    class Program
    {
        private static WCFServer _wcfServer;

        private class Options
        {
            [Option('p', "port", Required = true, DefaultValue = 8080, HelpText = "Socket connection port")]
            public int Port { get; set; }

            [Option('m', "maxconnections", Required = true, DefaultValue = 50000, HelpText = "Max number of sockets")]
            public int Maxconnection { get; set; }

            [Option('i', "Ip", Required = false, HelpText = "Socket connection ip")]
            public string Ipaddress { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this,
                  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

        private class Commands
        {
            [Option('c', "clear", HelpText = "Clear console window")]
            public bool Clear { get; set; }

            [Option('i', "info", HelpText = "List server information")]
            public bool Info { get; set; }

            [Option('e', "exit", HelpText = "Exits the server in a safe way")]
            public bool Exit { get; set; }

            [Option('p', "print", HelpText = "Args: on / off - Enable/Disable the server print messages")]
            public string Print { get; set; }

            [Option('d', "delete", HelpText = "Deletes all database data")]
            public bool Delete { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this,
                  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

        static void Main(string[] args)
        {
            Console.Title = $"MyGym Server";

            Print.Success("WCF up and running");

            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                _wcfServer = new WCFServer();
                if (!_wcfServer.Setup())
                {
                    Console.WriteLine("Failed to start server.");
                }
                else
                {
                    _wcfServer.Start();
                    Console.WriteLine($"MyGym Server running.");
                    Console.WriteLine("Type --help to see available commands.");
                    Console.WriteLine("--------------------------------------\n");
                    while (_wcfServer.GetState() == CommunicationState.Opened)
                    {
                        string input = Console.ReadLine();
                        var commands = new Commands();
                    }
                }
            }

            Console.WriteLine("Press any key to exit ...");
            Console.ReadKey();
        }
    }
}
