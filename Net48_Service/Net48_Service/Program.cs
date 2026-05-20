using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Security.AccessControl;

namespace Net48_Service
{
    class Program
    {
        static void Main(string[] args)
        {
            Launch_Action ToLaunch = Launch_Action.Launch_Console;

            // parse args
            foreach (string arg in args)
            {
                if (arg[0] == '-' || arg[0] == '/')
                {
                    switch (arg.Substring(1).ToLowerInvariant())
                    {
                        case "i":
                            ToLaunch = Launch_Action.Install;
                            break;
                        case "u":
                            ToLaunch = Launch_Action.Uninstall;
                            break;
                        case "c":
                            ToLaunch = Launch_Action.Launch_Console;
                            break;
                        case "s":
                            ToLaunch = Launch_Action.Launch_Service;
                            break;
                    }
                }
                else
                {
                    m_InstanceName = arg;
                }
            }

            // process action
            switch (ToLaunch)
            {
                case Launch_Action.Install:
                    Net48_Service.WinService.WinService_Installer.InstallMe();
                    break;
                case Launch_Action.Uninstall:
                    Net48_Service.WinService.WinService_Installer.UninstallMe();
                    break;
                case Launch_Action.Launch_Console:
                    Launch_Console();
                    break;
                case Launch_Action.Launch_Service:
                    Launch_Service();
                    break;
            }
        }

        public static string ServiceBaseName => "Net48_Service";

        public static string InstanceName => m_InstanceName;
        static string m_InstanceName = "default";

        public static string ServiceName => ServiceBaseName + "(" + InstanceName + ")";

        public static string DataFolderName
        {
            get
            {
                if (m_DataFolderName == null)
                {
                    m_DataFolderName = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)).FullName;
                    m_DataFolderName = Path.Combine(m_DataFolderName, ServiceBaseName);
                    if (!Directory.Exists(m_DataFolderName))
                        Directory.CreateDirectory(m_DataFolderName);
                    m_DataFolderName = Path.Combine(m_DataFolderName, InstanceName);
                    if (!Directory.Exists(m_DataFolderName))
                        Directory.CreateDirectory(m_DataFolderName);
                }
                return m_DataFolderName;
            }
        }
        static string m_DataFolderName = null;

        public static string LogFileName
        {
            get
            {
                if (m_LogFileName == null)
                {
                    m_LogFileName = Path.Combine(DataFolderName, ServiceBaseName) + ".log";
                }
                return m_LogFileName;
            }
        }
        static string m_LogFileName = null;

        enum Launch_Action
        {
            Launch_Console, Launch_Service, Install, Uninstall
        }

        static void Launch_Service()
        {
            var myService = new WinService.WinService_Service();
            try
            {
                System.ServiceProcess.ServiceBase.Run(myService);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void Launch_Console()
        {
            if (WinService.WinService_Controller.Start())
            { 
                Console.WriteLine(DataFolderName);
                Console.Write("Press Enter to quit...");
                Console.ReadLine();
                WinService.WinService_Controller.Stop();
            }
        }

        static public void Log(string bsToLog)
        {
            DateTime now = DateTime.Now;
            string textToLog = now.ToShortDateString() + " " + now.ToShortTimeString() + " " + ServiceName + ": " + bsToLog;

            while (m_bLogWriting)
            {
                Thread.Sleep(0);
            }
            m_bLogWriting = true;

            try
            {
                FileStream fs = new FileStream(LogFileName, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter m_streamWriter = new StreamWriter(fs);
                m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                m_streamWriter.WriteLine(textToLog);
                m_streamWriter.Flush();
                m_streamWriter.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(textToLog + "\r\n" + e.Message);
            }

            m_bLogWriting = false;
        }
        static bool m_bLogWriting = false;
    }
}
