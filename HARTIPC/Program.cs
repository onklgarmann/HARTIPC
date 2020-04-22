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
                var binary = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x00, 0x08, 0xff, 0xff };
                HARTIPFrame ipframe = new HARTIPFrame(binary);
                Console.WriteLine(BitConverter.ToString(ipframe.SerializeFrame()));

                /*IPEndPoint server = new IPEndPoint(IPAddress.Parse("192.168.10.255"), 5094);
                Console.WriteLine(server);
                TcpClient client = new TcpClient();
                client.Connect("192.168.10.189", 5094);


                HARTIPFrame frame = new HARTIPFrame(1, messageID: MessageID.Initiate);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();
                Byte[] data = frame.Serialize(new byte[] { 0x01, 0x00, 0x09, 0x27, 0xc0 });

                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", BitConverter.ToString(data));

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                var header = new Byte[8];
                HARTDecoder decoder = new HARTDecoder();
                stream.Read(header, 0, header.Length);
                Console.WriteLine("Received: {0}", BitConverter.ToString(header));
                //IHARTFrame receivedFrame = decoder.Decode(ref data);

                //Console.WriteLine("Received: {0}", BitConverter.ToString(receivedFrame.Serialize()));

                // Close everything.
                stream.Close();
                client.Close();*/
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
