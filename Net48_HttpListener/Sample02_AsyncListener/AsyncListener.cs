using System;
using System.Net;
using System.Text;

namespace Sample02_AsyncListener
{
    class AsyncListener
    {
        private static HttpListener listener;

        public static void SimpleListenerExample(string[] prefixes)
        {
            listener = new HttpListener();
            foreach (string s in prefixes)
            {
                try
                {
                    listener.Prefixes.Add(s);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            try
            {
                listener.Start();
                Console.WriteLine("Warte auf Anfragen...");
                // Asynchronously waiting for the next request
                listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
            }
            catch (Exception e2)
            {
                Console.WriteLine(e2.Message);
            }

            // Keeping the Server Alive
            Console.ReadLine(); 
        }

        private static void ListenerCallback(IAsyncResult ar)
        {
            HttpListener listener = (HttpListener)ar.AsyncState;

            // Retrieve the context containing the request and response.
            HttpListenerContext context = listener.EndGetContext(ar);

            // Start the next asynchronous request while this one is being processed.
            listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);

            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            processRequest(request, response);
        }

        private static void processRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            StringBuilder Result = new StringBuilder();

            Result.AppendLine("Async_Sample");
            Result.AppendLine("HttpMethod     : " + request.HttpMethod);
            Result.AppendLine("Url            : " + request.Url.ToString());
            Result.AppendLine("RawUrl         : " + request.RawUrl);
            Result.AppendLine("ContentType    : " + request.ContentType);
            Result.AppendLine("UserAgent      : " + request.UserAgent);
            Result.AppendLine("ServiceName    : " + request.ServiceName);
            Result.AppendLine("UserHostAddress: " + request.UserHostAddress);
            Result.AppendLine("UserHostName   : " + request.UserHostName);
            Result.AppendLine("LocalEndPoint  : " + request.LocalEndPoint.Address);
            Result.AppendLine("RemoteEndPoint : " + request.RemoteEndPoint.Address);
            foreach (var qse in request.QueryString)
            {
                string qsName = qse.ToString();
                Result.AppendLine(qsName.PadRight(15) + ": " + request.QueryString[qsName]);
            }

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(Result.ToString());
            response.ContentType = "text/plain";
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    }
}
