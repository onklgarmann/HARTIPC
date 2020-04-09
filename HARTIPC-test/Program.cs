using System;
using System.Net;
using System.Net.Sockets;

namespace HARTIPC_test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                
                IPEndPoint server = new IPEndPoint(IPAddress.Parse("192.168.10.189"), 5094);
                Console.WriteLine(server);


                HARTMessage message = new HARTMessage((byte)0x01, new byte[] { 0xa6, 0x4e, 0x0b, 0x6f, 0xe4 });
                Console.WriteLine(BitConverter.ToString(message.ToByteArray()));

                byte b = 0b1000_0010;
                Console.WriteLine((b & (1 << 3 - 1)) != 0);
                
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
