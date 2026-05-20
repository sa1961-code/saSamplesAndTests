using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample01_Microsoft
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string[] myPrefixes = { "http://*:8001/" };
                Microsoft_Sample.SimpleListenerExample(myPrefixes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Press Enter to quit...");
            Console.ReadLine();
        }
    }
}
