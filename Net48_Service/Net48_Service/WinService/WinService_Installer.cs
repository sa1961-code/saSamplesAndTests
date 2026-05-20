using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration.Install;
using System.ServiceProcess;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Win32;

namespace Net48_Service.WinService
{
    [RunInstaller(true)]
    public class WinService_Installer : Installer
    {
        #region Installer

        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller processInstaller;

        public WinService_Installer()
        {
            serviceInstaller = new ServiceInstaller();
            serviceInstaller.ServiceName = Program.ServiceName;
            serviceInstaller.Description = Program.ServiceName;
            serviceInstaller.DisplayName = Program.ServiceName;
            serviceInstaller.StartType   = ServiceStartMode.Manual;
            //this.serviceInstaller.DelayedAutoStart = true;
            Installers.Add(serviceInstaller);

            processInstaller = new ServiceProcessInstaller();
            processInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            processInstaller.Password = null;
            processInstaller.Username = null;
            Installers.Add(processInstaller);
        }

        #endregion

        #region self installer

        private static string ExePath => Assembly.GetExecutingAssembly().Location;

        public static bool InstallMe()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { ExePath });

                // get ImagePath from registry
                var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + Program.ServiceName, true);
                string ImagePath = key.GetValue("ImagePath").ToString();

                // add Instance parameter to ImagePath
                ImagePath += " /s " + Program.InstanceName;
                key.SetValue("ImagePath", ImagePath, RegistryValueKind.ExpandString);
                key.Close();

                Console.WriteLine(ImagePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            Console.WriteLine(Program.ServiceName + " is installed.");
            return true;
        }

        public static bool UninstallMe()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { "/u", ExePath });
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            Console.WriteLine(Program.ServiceName + " is removed.");
            return true;
        }

        #endregion
    }
}
