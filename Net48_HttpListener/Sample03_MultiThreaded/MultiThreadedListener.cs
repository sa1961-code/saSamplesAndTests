using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sample03_MultiThreaded
{
    class MultiThreadedListener
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

            var thr = new WorkerThread(context.Request, context.Response);
        }
    }
}
