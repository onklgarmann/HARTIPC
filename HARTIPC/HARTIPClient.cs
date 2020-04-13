using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HARTIPC_test
{
    class HARTIPClient
    {
        TcpClient client;
        IPEndPoint server;
        NetworkStream stream;
        HARTIPWrapper message;
        public HARTIPClient(IPEndPoint server)
        {
            this.client = new TcpClient();
            this.server = server;
            this.message = new HARTIPWrapper();
        }

        public void Connect()
        {
            client.Connect(server);
            stream = client.GetStream();
            stream.Write(message.Initiate(), 0, message.Length);
        }

    }
}
