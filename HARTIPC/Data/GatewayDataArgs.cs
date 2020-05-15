using System;
using System.Collections.Generic;
using System.Text;

namespace HARTIPC.Data
{
    class GatewayDataArgs : EventArgs
    {
        public string GatewayAddress { get; set; }
        public string GatewayTag { get; set; }
        public GatewayDataArgs(string GatewayAddress, string GatewayTag)
        {
            this.GatewayAddress = GatewayAddress;
            this.GatewayTag = GatewayTag;
        }
             
    }
}
