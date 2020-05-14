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
                using (var client = new HARTIPClient(IPEndPoint.Parse(ConfigurationManager.AppSettings["Socket"])))
                {
                    var address = new byte[] { 0xa6, 0x4e, 0x0b, 0x6f, 0xe4 };
                    var type = new byte[] { 0x11, 0x0e };
                    var id = new byte[] { 0x00, 0x5d, 0x6d, 0x68 };
                    
                    var dbAgent = new DBAgent();
                    client.DataEntryReceived += dbAgent.OnDataEntryReceived;
                    
                    

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
