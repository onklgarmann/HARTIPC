using System;
using System.Collections;
using System.Collections.Generic;
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
                byte[] payload = { 0x06, 0x80, 0x00, 0x18, 0x00, 0x50, 0xfe, 0x26, 0x4e, 0x05, 0x07, 0x05, 0x02, 0x0e,
            0x0c, 0x0b, 0x6f, 0xe4, 0x05, 0x04, 0x00, 0x02, 0x00, 0x00, 0x26, 0x00, 0x26, 0x84, 0x58 };
                Console.WriteLine(BitConverter.ToString(payload));
                HARTDecoder decoder = new HARTDecoder();
                IHARTFrame frame = decoder.Decode(ref payload);
                HARTFrameACK frame2 = new HARTFrameACK(new byte[] { 0x80 }, 0x00, new byte[] { 0x00, 0x50, 0xfe, 0x26, 0x4e, 0x05, 0x07, 0x05, 0x02, 0x0e,
            0x0c, 0x0b, 0x6f, 0xe4, 0x05, 0x04, 0x00, 0x02, 0x00, 0x00, 0x26, 0x00, 0x26, 0x84});
                Console.WriteLine(BitConverter.ToString(frame2.Serialize()));
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
