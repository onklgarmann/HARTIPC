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

                byte[] input = new byte[] { 0x82, 0xa6, 0x4e, 0x0b, 0x6f, 0xe4, 0x00, 0x00, 0xea };
                HARTMessage message = new HARTMessage(input);
                HARTMessage manualMessage = new HARTMessage(0, new byte[] { 0xa6, 0x4e, 0x0b, 0x6f, 0xe4 });
                Console.WriteLine("input : {0}", BitConverter.ToString(input));
                Console.WriteLine("output: {0}", BitConverter.ToString(message.ToByteArray()));
                Console.WriteLine("manual: {0}", BitConverter.ToString(manualMessage.ToByteArray()));




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
