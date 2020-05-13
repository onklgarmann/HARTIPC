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

            try
            {
                
                var IP = IPEndPoint.Parse(ConfigurationManager.AppSettings["Socket"]);
                using (var client = new HARTIPClient(IP))
                {
                    var address = new byte[] { 0xa6, 0x4e, 0x0b, 0x6f, 0xe4 };
                    client.Connect();
                    client.Initiate(600000);
                    var dbAgent = new DBAgent();
                    client.DataEntryReceived += dbAgent.OnDataEntryReceived;
                    var frame = new HARTFrame(address: address, 0x4D, new byte[] { 0x00, 0x00, 0x05, 0x82, 0x91, 0x0e, 0x5d, 0x6b, 0x68, 0x03, 0x00 });
                    client.PDU(frame);
                    

                }
                
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }
        

        
    }
}
