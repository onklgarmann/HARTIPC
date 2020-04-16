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
                var input = new byte[] { 0x06, 0x80, 0x00, 0x18, 0x00, 0x50, 0xfe, 0x26, 0x4e, 0x05, 0x07, 0x05, 0x02, 0x0e,
            0x0c, 0x0b, 0x6f, 0xe4, 0x05, 0x04, 0x00, 0x02, 0x00, 0x00, 0x26, 0x00, 0x26, 0x84, 0x58, 0x00, 0x01 };
                var frame2 = new HARTFrame(input);

                Console.WriteLine(BitConverter.ToString(input));
                Console.WriteLine(BitConverter.ToString(frame2.ToArray()));
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
