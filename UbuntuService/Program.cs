using System;
using System.Threading;
using UbuntuService;

namespace Ubuntu
{
    class Program
    {
        static void Main(string[] args)
        {
            MasterClient masterClient = null;
            using (masterClient = new MasterClient())
            {
                while (true)
                {
                    Thread.Sleep(10000);
                    Console.WriteLine("service master client rabbitmq working...");
                }
            }
        }
    }
}
