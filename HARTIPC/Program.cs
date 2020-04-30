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
        
        static void Main(string[] args)
        {

            try
            {
                ushort sequenceNumber = 2;
                IPEndPoint server = new IPEndPoint(IPAddress.Parse("192.168.10.255"), 5094);
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
                //Console.WriteLine("Received: {0}", BitConverter.ToString(header));
                Timer timer = new Timer();
                timer.Interval = 60000;
                timer.Elapsed += (sender, e) => Timer_Elapsed(sender, e, ref stream, ref sequenceNumber);
                timer.AutoReset = true;
                timer.Enabled = true;
                
                // Close everything.

                do
                {
                    while (!Console.KeyAvailable)
                    {
                        // Do something
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
                frame = new HARTIPFrame(1, messageID: MessageID.Close);
                data = frame.SerializeHeader();
                stream.Write(data, 0, data.Length);
                stream.Read(header, 0, header.Length);
                do 
                {
                    stream.Read(header,0,header.Length);
                } while (stream.DataAvailable);
                
                client.Close();
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
        

        private static void Timer_Elapsed(Object source, ElapsedEventArgs e, ref NetworkStream stream, ref ushort sequenceNumber)
        {
            HARTIPFrame frame = new HARTIPFrame(sequenceNumber, messageID: MessageID.KeepAlive);
            Byte[] data = frame.SerializeHeader();
            stream.Write(data, 0, data.Length);
            sequenceNumber++;
            frame = new HARTIPFrame(sequenceNumber, messageID: MessageID.PDU);
            HARTFrame PDU = new HARTFrame(new byte[]{ 0x00 }, 3);
            data = frame.Serialize(PDU.Serialize());
            stream.Write(data, 0, data.Length);
            Console.WriteLine("Sent: {0}", BitConverter.ToString(data));
            sequenceNumber++;
        }
    }
}
