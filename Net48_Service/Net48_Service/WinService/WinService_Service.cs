using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net48_Service.WinService
{
    class WinService_Service : System.ServiceProcess.ServiceBase
    {
        public WinService_Service() : base()
        {
            this.ServiceName = Program.ServiceName;
        }

        /// <summary>
        /// Set things in motion so your service can do its work.
        /// </summary>
        protected override void OnStart(string[] args)
        {
            WinService_Controller.Start();
        }

        /// <summary>
        /// Stop this service.
        /// </summary>
        protected override void OnStop()
        {
            WinService_Controller.Stop();
        }
    }
}
