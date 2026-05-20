using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Net48_Service.WinService
{
    internal static class WinService_Controller
    {
        static public bool StopRequested => m_StopRequested;
        static volatile bool m_StopRequested = false;

        static volatile public int m_ThreadCount = 0;

        static public bool Start()
        {
            m_StopRequested = false;
            m_ThreadCount = 0;
            Program.Log("Service start pending...");
            // todo start first thread here
            Program.Log("Service started.");
            return true;
        }

        static public bool Stop()
        {
            Program.Log("Service (" + m_ThreadCount.ToString() + " threads) stop pending...");
            m_StopRequested = true;
            while (m_ThreadCount > 0)
            {
                Thread.Sleep(1);
            }
            Program.Log("Service stopped.");
            return true;
        }
    }
}
