using System;
using System.Net;
using System.IO;

namespace WebServer
{
    class Program
    {
        private static bool _appIsRunning = true;

        static void Main()
        {
            using var server = new HttpServer();
            server.Start();

            while (_appIsRunning)
            {
                HandleCommand(Console.ReadLine()?.ToLower(), server);
            }
        }

        private static void HandleCommand(string? command, HttpServer server)
        {
            switch (command)
            {
                case "start":
                    server.Start();
                    break;

                case "restart":
                    server.Stop();
                    server.Start();
                    break;

                case "stop":
                    server.Stop();
                    break;

                case "status":
                    Console.WriteLine(server.Status.ToString());
                    break;

                case "exit":
                    _appIsRunning = false;
                    break;
            }
        }
    }
}