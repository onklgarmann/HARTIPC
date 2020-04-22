using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HARTIPC
{
    class HARTIPClient : IDisposable
    {
        TcpClient client;
        NetworkStream stream;
        IPEndPoint server { get; set; }
        ushort SequenceNumber { get; set; } = 1;
        bool IsConnected { get; set; }
        
        public HARTIPClient(IPEndPoint server)
        {
            this.client = new TcpClient();
            this.server = server;
            
        }

        public void Connect()
        {
            client.Connect(server.Address.ToString(), server.Port);
            stream = client.GetStream();
        }
        public void Initiate(int timeout)
        {
            byte[] initiate60sTimeout = new byte[] { 0x01, 0x00, 0x09, 0x27, 0xc0 };
            HARTIPFrame frame = new HARTIPFrame(SequenceNumber, messageID: MessageID.Initiate);
            frame.Serialize(initiate60sTimeout);
        }

        public void Dispose()
        {
            stream.Close();
            client.Close();
        }
    }
}
