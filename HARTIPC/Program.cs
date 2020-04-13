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
                UdpClient udp = new UdpClient();
                udp.Send(new byte[] { 0x01, 0x00, 0x80, 0x00, 0x00, 0x01, 0x00, 0x18, 0x4C, 0x0F, 0x38, 0xAC, 0x48, 0xAE, 0x49, 0x35, 0xB6, 0x89, 0x8F, 0x21, 0xF8, 0x5F, 0xC0, 0x30 }, 24, server);



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
