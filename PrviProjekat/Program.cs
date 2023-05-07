using System;
using System.Net;
using System.Text;
using System.Threading;

namespace PrviProjekat 
{
    internal class Program
    {
        static string port = "5000/";
        static string url = "http://localhost:" + port;
        static string root = "..\\..\\..\\..\\Root\\";

        static void Main(string[] args)
        {
            Console.WriteLine("Main thread...");

            string[] arguments = { url , root};

            Thread server = new Thread(() =>
            {
                WebServer server = new WebServer(arguments);
                server.Start();
            });

            server.Start();
        }
    }
}