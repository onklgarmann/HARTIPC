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
                var input = new byte[] { 0x82, 0xA6, 0x4E, 0x0B, 0x6F, 0xE4, 0x4D, 0x0B, 0x00, 0x00, 0x05, 0x82, 0x91, 0x0E, 0x5D, 0x6B, 0x68, 0x01, 0x00, 0xEB };
                var input2 = new byte[] { 0x02, 0x80, 0x00, 0x00, 0x82 };
                HARTFrame frame = new HARTFrame(input);
                HARTFrame frame2 = new HARTFrame(new byte[] { 0xA6, 0x4E, 0x0B, 0x6F, 0xE4 }, 0x4D, new byte[] { 0x00, 0x00, 0x05, 0x82, 0x91, 0x0E, 0x5D, 0x6B, 0x68, 0x01, 0x00 });
                Console.WriteLine(BitConverter.ToString(input));
                Console.WriteLine(BitConverter.ToString(frame.Serialize()));
                Console.WriteLine(BitConverter.ToString(frame2.Serialize()));
                Console.WriteLine(frame2.GetLength());
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
