using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PrviProjekat
{
    internal class WebServer
    {
        private HttpListener listener;
        private string root;
        private string listenUrl;
        Cache ImageCache;

        public WebServer(string[] arg)
        {
            Console.WriteLine("Server thread started.");

            this.listenUrl = arg[0]; 
            this.root = arg[1];

            ImageCache = new Cache();
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
                    if (imageName == "")
                    {
                        HandleError(response, "");
                    }
                    else
                    {
                        if (fileExt == ".jpg" || fileExt == ".jpeg" || fileExt == ".png")
                        {
                            returnImage(response, imageName);
                            Console.WriteLine("Request succesfully processed.");
                        }
                        else
                        {
                            HandleError(response, "type");
                        }
                    }
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);    
            }
            finally
            {
                Console.WriteLine("Finshed with the request.\n");
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
                        ret = "<h2>Error - bad request.</h2>";
                        res.StatusCode = (int)HttpStatusCode.BadRequest;
                        res.StatusDescription = "Bad request.";
                        Console.WriteLine("Error - bad request.");

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
                byte[] buf;
                //provera da li je u kesu

                    if (ImageCache.GetImageFromCache(imageName, out buf))
                    {
                        Console.WriteLine("Requested image is in cache.");
                    }
                    else
                    {
                        buf = File.ReadAllBytes(path);
                        ImageCache.AddImageToCache(imageName, buf);
                        Console.WriteLine("Requested image is not in cache. Adding it to cache.");

                    }

                res.StatusCode = (int)HttpStatusCode.OK;
                res.StatusDescription = "Status OK";
                res.Headers.Set("Content-Type", "image/jpg");
                using Stream output = res.OutputStream;
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
