using System;
using System.Net;
using System.Text;
using System.Threading;

namespace PrviProjekat 
{
    internal class Program
    {
        static string port = "5000/";
        static string prefix = "http://localhost:" + port;
        static string root = "..\\..\\..\\..\\Root\\";

        static void Main(string[] args)
        {
            Console.WriteLine("Main thread...");

            string[] prefixes = { prefix , root};

            Thread server = new Thread(() =>
            {
                WebServer server = new WebServer(prefixes);
                server.Start();
            });

            server.Start();
        }
    }
}