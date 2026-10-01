using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace Net48_WindowsAccounts
{
    class Program
    {
        static void Main(string[] args)
        {
            ManagementObjectSearcher usersSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Account");
            ManagementObjectCollection users = usersSearcher.Get();

            foreach (var user in users)
            {
                string sid = user["SID"].ToString();

                if ((sid == "S-1-1-0") || (sid == "S-1-5-18") || (sid == "S-1-5-19") || (sid == "S-1-5-20"))
                {
                    Console.WriteLine(user["Name"]);
                    foreach (var p in user.Properties)
                    {
                        object v = p.Value;
                        if (v == null)
                        {
                            v = "(null)";
                        }
                        Console.WriteLine("\t" + p.Name + "\t" + v.ToString());
                    }
                    Console.WriteLine();
                }
            }

            Console.ReadLine();
        }
    }
}
