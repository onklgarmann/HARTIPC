using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace HARTIPC
{
    class Program
    {
        
        static void Main()
        {

            try
            {
                var input = new byte[] { 0x06, 0x80, 0x00, 0x18, 0x00, 0x50, 0xfe, 0x26, 0x4e, 0x05, 0x07, 0x05, 0x02, 0x0e,
            0x0c, 0x0b, 0x6f, 0xe4, 0x05, 0x04, 0x00, 0x02, 0x00, 0x00, 0x26, 0x00, 0x26, 0x84, 0x58, 0x00, 0x01 };
                var input2 = new byte[] { 0x02, 0x80, 0x00, 0x00, 0x82 };
                HARTFrame frame = new HARTFrame(input);
                HARTIPFrame ipframe = new HARTIPFrame(input);
                Console.WriteLine(BitConverter.ToString(ipframe.Serialize()));
                
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
