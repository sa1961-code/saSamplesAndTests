using System;
using System.Net;
using System.Web;
using System.Collections.Generic;
using System.Text;

namespace Sample03_MultiThreaded
{
    class WorkerThread
    {
        public HttpListenerRequest Request => m_Request;
        public HttpListenerRequest m_Request = null;

        public HttpListenerResponse Response => m_Response;
        public HttpListenerResponse m_Response = null;

        public string UrlLocation => m_UrlLocation;
        string m_UrlLocation = null;

        public Dictionary<string, string> UrlParameters => m_UrlParameters;
        Dictionary<string, string> m_UrlParameters = null;

        public string UrlFileName
        {
            get
            {
                if (m_UrlFileName == null)
                {
                    m_UrlFileName = HttpUtility.UrlDecode(UrlLocation).Replace("/", "\\");
                    foreach (char c in "*<>:\"|?")
                        if (m_UrlFileName.IndexOf(c) >= 0)
                            m_UrlFileName = m_UrlFileName.Replace(c, '_');
                }
                return m_UrlFileName;
            }
        }
        string m_UrlFileName = null;

        public string UrlFileExtension => System.IO.Path.GetExtension(UrlFileName).ToLowerInvariant();

        public WorkerThread(HttpListenerRequest paramRequest, HttpListenerResponse paramResponse)
        {
            m_Request = paramRequest;
            m_Response = paramResponse;
            System.Threading.Thread thr = new System.Threading.Thread(_ThreadProc_);
            // thr.SetApartmentState(ApartmentState.MTA);
            thr.Start(this);
        }

        /// <summary>
        /// _ThreadProc_ is required for technical reasons to call the ProcessRequest function.
        /// </summary>
        /// <param name="o"></param>
        static void _ThreadProc_(object o)
        {
            ((WorkerThread)o).ProcessRequest();
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        void ProcessRequest()
        {
            ParseRequestUrl();

            // generate some stuff as response
            StringBuilder Result = new StringBuilder();

            Result.AppendLine("Thread_Sample");
            Result.AppendLine("HttpMethod     : " + Request.HttpMethod);
            Result.AppendLine("Url            : " + Request.Url.ToString());
            Result.AppendLine("RawUrl         : " + Request.RawUrl);
            Result.AppendLine("ContentType    : " + Request.ContentType);
            Result.AppendLine("UserAgent      : " + Request.UserAgent);
            Result.AppendLine("ServiceName    : " + Request.ServiceName);
            Result.AppendLine("UserHostAddress: " + Request.UserHostAddress);
            Result.AppendLine("UserHostName   : " + Request.UserHostName);
            Result.AppendLine("LocalEndPoint  : " + Request.LocalEndPoint.Address);
            Result.AppendLine("RemoteEndPoint : " + Request.RemoteEndPoint.Address);
            Result.AppendLine("UrlFileName    : " + UrlFileName);
            Result.AppendLine("UrlFileExt     : " + UrlFileExtension);
            Result.AppendLine();

            Result.AppendLine("Headers");
            if (Request.Headers != null)
            {
                foreach (var h in Request.Headers)
                {
                    Result.AppendLine(h.ToString().PadRight(15) + ": " + Request.Headers[h.ToString()]);
                }
            }
            Result.AppendLine();

            Result.AppendLine("Parameters");
            if (UrlParameters != null)
            {
                foreach (var p in UrlParameters)
                {
                    Result.AppendLine(p.Key.PadRight(15) + ": " + p.Value);
                }
            }
            Result.AppendLine();


            // send response
            try
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(Result.ToString());
                Response.ContentType = "text/plain";
                Response.ContentLength64 = buffer.Length;
                System.IO.Stream output = Response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch(Exception)
            {

            }
        }

        /// <summary>
        /// Split the request URL into the relative address and parameters.
        /// </summary>
        void ParseRequestUrl()
        {
            string rawUrl = Request.RawUrl;
            while (rawUrl.IndexOf("//") >= 0)
            {
                rawUrl = rawUrl.Replace("//", "/");
            }

            int urlSplitPos = rawUrl.IndexOf('?');
            if (urlSplitPos < 0)
            {
                // no parameters send ?
                m_UrlLocation = rawUrl;
                return;
            }

            m_UrlLocation = rawUrl.Substring(0, urlSplitPos);
            if (urlSplitPos == (rawUrl.Length - 1))
            {
                // empthy parameters ?
                return;
            }

            // store parameters to dictionary
            var RawParameters = rawUrl.Substring(urlSplitPos + 1).Split('&');
            m_UrlParameters = new Dictionary<string, string>();
            foreach(var RawParam in RawParameters)
            {
                int paramSplitPos = RawParam.IndexOf('=');
                if (paramSplitPos < 0)
                {
                    m_UrlParameters.Add(RawParam, "");
                }
                else if (paramSplitPos == (RawParam.Length - 1))
                {
                    m_UrlParameters.Add(RawParam.Substring(0, paramSplitPos), "");
                }
                else
                {
                    string paramValue = RawParam.Substring(paramSplitPos + 1);
                    m_UrlParameters.Add(RawParam.Substring(0, paramSplitPos), HttpUtility.UrlDecode(paramValue));
                }
            }
        }
    }
}
