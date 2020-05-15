using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Data.SqlClient;
using System.Timers;

namespace HARTIPC
{
    class Program
    {
        static void Main()
        {
            using (var client = new HARTIPClient(IPEndPoint.Parse(ConfigurationManager.AppSettings["Socket"]), 600000, 30000))
            {
                var dbAgent = new DBAgent(ConfigurationManager.ConnectionStrings["HARTdata"].ToString());
                client.DataEntryReceivedEvent += dbAgent.OnDataEntryReceivedEvent;
                //client.NewGatewayEvent += dbAgent.OnNewGatewayEvent;
                client.NewDeviceEvent += dbAgent.OnNewDeviceEvent;
                client.Start();

                while (true) { }
            }
            Console.WriteLine("Press Enter to continue...");
            Console.Read();
        
        
        
        }
        


    }
}
