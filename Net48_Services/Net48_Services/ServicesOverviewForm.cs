using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//using System.Configuration.Install;
using System.ServiceProcess;
//using System.ComponentModel;
//using System.Reflection;
using Microsoft.Win32;


namespace Net48_Services
{
    public partial class ServicesOverviewForm : Form
    {
        Timer m_UpdateTimer;

        public ServicesOverviewForm()
        {
            InitializeComponent();

            var services = ServiceController.GetServices();
            foreach (var service in services)
            {
                listBox1.Items.Add(new ServiceRef(service));
            }
            listBox1.SelectedIndex = 0;

            m_UpdateTimer = new Timer();
            m_UpdateTimer.Interval = 1000;
            m_UpdateTimer.Tick += M_UpdateTimer_Tick;
            m_UpdateTimer.Start();
        }

        /// <summary>
        /// updates the properties in background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M_UpdateTimer_Tick(object sender, EventArgs e)
        {
            m_UpdateTimer.Stop();
            UpdateProperties();
            m_UpdateTimer.Start();
        }

        void UpdateProperties()
        {
            ServiceController sc = GetSelectedService();
            if (sc == null)
            {
                textBoxServiceName.Text = "";
                textBoxDisplayName.Text = "";
                textBoxStatus.Text = "";
                textBoxStartType.Text = "";
                buttonChange.Enabled = false;
            }
            else
            { 
                textBoxServiceName.Text = sc.ServiceName;
                textBoxDisplayName.Text = sc.DisplayName;
                textBoxStatus.Text = sc.Status.ToString();
                textBoxStartType.Text = sc.StartType.ToString();

                switch (sc.Status)
                {
                    case ServiceControllerStatus.Stopped:
                        buttonChange.Enabled = true;
                        break;
                    case ServiceControllerStatus.Paused:
                        buttonChange.Enabled = sc.CanPauseAndContinue;
                        break;
                    case ServiceControllerStatus.Running:
                        buttonChange.Enabled = sc.CanStop;
                        break;
                    default:
                        buttonChange.Enabled = false;
                        break;
                }
            }
        }

        /// <summary>
        /// changes the service status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonChange_Click(object sender, EventArgs e)
        {
            ServiceController sc = GetSelectedService();
            if (sc != null)
            {
                buttonChange.Enabled = false;
                switch (sc.Status)
                {
                    case ServiceControllerStatus.Stopped:
                        sc.Start();
                        break;
                    case ServiceControllerStatus.Paused:
                        sc.Continue();
                        break;
                    case ServiceControllerStatus.Running:
                        sc.Stop();
                        break;
                }
            }
        }

        ServiceController GetSelectedService()
        {
            ServiceRef sr = (ServiceRef)(listBox1.SelectedItem);
            return sr.GetService();
        }
    }

    class ServiceRef
    {
        public string ServiceName;
        public string DisplayName;

        public ServiceRef(ServiceController sc)
        {
            ServiceName = sc.ServiceName;
            DisplayName = sc.DisplayName;
        }

        /// <summary>
        /// Retrieve the title as an item in a list box.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return DisplayName;
        }

        /// <summary>
        /// Determines the ServiceController corresponding to the ServiceName field.
        /// </summary>
        /// <returns></returns>
        public ServiceController GetService()
        {
            var services = ServiceController.GetServices();
            foreach (var service in services)
            {
                if (service.ServiceName == ServiceName)
                {
                    return service;
                }
            }
            return null;
        }
    }
}
