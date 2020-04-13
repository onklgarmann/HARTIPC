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
                IPEndPoint server = new IPEndPoint(IPAddress.Parse("192.168.10.189"), 5094);
                Console.WriteLine(server);
                byte[] input = { 0x01, 0x00, 0x09, 0x27, 0xc0 };
                Console.WriteLine("input:\t{0}", BitConverter.ToString(input));
                
                HARTFrame frame = new HARTFrame(input);
                byte[] output = frame.ToArray();
                Console.WriteLine("output:\t{0}", BitConverter.ToString(output));
                Console.WriteLine(input.SequenceEqual(output));



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
