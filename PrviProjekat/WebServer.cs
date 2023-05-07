using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PrviProjekat
{
    internal class WebServer
    {
        private HttpListener listener;
        private string root;
        private string listenUrl;
        public WebServer(string[] prefixes)
        {
            Console.WriteLine("Server thread started.");

            this.listenUrl = prefixes[0]; 
            this.root = prefixes[1];
        }

        public void Start()
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(listenUrl);
                listener.Start();
                Console.WriteLine("Server is listening.");

                while (listener.IsListening)
                {
                    HttpListenerContext context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(HandleRequest,context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Stop()
        {
            Console.WriteLine("Server thread stopped.");
        }
        private void HandleRequest(object? context)
        {
            try
            {
                HttpListenerContext localContext = context as HttpListenerContext;
                HttpListenerRequest request = localContext.Request;
                string url = request.Url.ToString();
                Console.WriteLine($"Received request at {url}.");

                HttpListenerResponse response = localContext.Response;
                if (request.HttpMethod != "GET")
                {
                    HandleError(response, "method");
                }
                else
                {
                    string fileExt = Path.GetExtension(url);
                    string imageName = Path.GetFileName(url);

                    if (fileExt == ".jpg" || fileExt == ".jpeg" || fileExt == ".png")
                    {
                        returnImage(response, imageName);
                    }
                    else
                    {
                        HandleError(response, "type");
                    }
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);    
            }
            finally
            {
                Console.WriteLine("Request is processed.");
            }
        }
        private void HandleError(HttpListenerResponse res, string error)
        {
            string ret = "";
            if (error == "method")
            {
                //vraca se informacija o gresci
                ret = "<h2>Error - only GET request is valid.</h2>";
                res.StatusCode = (int)HttpStatusCode.BadRequest;
                res.StatusDescription = "Bad request";
                Console.WriteLine("Error - only GET request is valid.");
            }
            else
            {
                if (error == "type")
                {
                    ret = "<h2>Error - invalid type of file requested.</h2>";
                    res.StatusCode = (int)HttpStatusCode.NotAcceptable;
                    res.StatusDescription = "Not acceptable.";
                    Console.WriteLine("Error - invalid type of file requested.");
                }
                else
                {
                    if (error == "notfound")
                    {
                        ret = "<h2>Error - requested file is not available.</h2>";
                        res.StatusCode = (int)HttpStatusCode.NotFound;
                        res.StatusDescription = "Not found.";
                        Console.WriteLine("Error - requested file is not available.");

                    }
                    else
                    {
                        ret = "<h2>Loš zahtev.</h2>";
                        res.StatusCode = (int)HttpStatusCode.BadRequest;
                        res.StatusDescription = "Bad request.";
                    }
                }
            }

            res.Headers.Set("Content-Type", "text/html");
            byte[] buf = Encoding.UTF8.GetBytes(ret);
            using Stream output = res.OutputStream;
            res.ContentLength64 = buf.Length;
            output.Write(buf, 0, buf.Length);
            output.Close();
        }
        private void returnImage(HttpListenerResponse res, string imageName)
        {

            string path = Directory.GetFiles(root, imageName, SearchOption.AllDirectories).FirstOrDefault();
            if (path!=null)
            {
                res.StatusCode = (int)HttpStatusCode.OK;
                res.StatusDescription = "Status OK";
                res.Headers.Set("Content-Type", "image/jpg");

                using Stream output = res.OutputStream;
                byte[] buf = File.ReadAllBytes(path);
                res.ContentLength64 = buf.Length;
                output.Write(buf, 0, buf.Length);
                output.Close();
            }
            else 
            {
                HandleError(res, "notfound");
            }

        }
    }
}
