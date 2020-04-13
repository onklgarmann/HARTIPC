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
        
        public HARTIPClient(IPEndPoint server)
        {
            this.client = new TcpClient();
            this.server = server;
            
        }

        public void Connect()
        {
            client.Connect(server);
            stream = client.GetStream();
            
        }

    }
}
