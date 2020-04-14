using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace HARTIPC
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //test
                IPEndPoint server = new IPEndPoint(IPAddress.Parse("192.168.10.255"), 5094);
                Console.WriteLine(server);
                //var frame = new HARTFrame(new byte[] { 0x00 }, (byte)0x00);
                var frame2 = new HARTFrame(new byte[] { 0x86, 0xA6, 0x4E, 0x0B, 0x6F, 0xE4, 0x4D, 0x0B, 0x00, 0x00, 0x05, 0x82, 0x91, 0x0E, 0x5D, 0x6B, 0x68, 0x01, 0x00, 0xEF });
                Console.WriteLine(frame2.GetType());



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
