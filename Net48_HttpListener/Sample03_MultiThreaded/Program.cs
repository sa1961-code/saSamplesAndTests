using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample03_MultiThreaded
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] myPrefixes = { "http://*:8001/" };
            MultiThreadedListener.SimpleListenerExample(myPrefixes);
        }
    }
}
