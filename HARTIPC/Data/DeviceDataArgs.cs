using System;
using System.Collections.Generic;
using System.Text;

namespace HARTIPC.Data
{
    class DeviceDataArgs : EventArgs
    {
        public string DeviceAddress { get; set; }
        public string DeviceTag { get; set; }
        public int GatewayID { get; set; }
        public DeviceDataArgs(string DeviceAddress, string DeviceTag, int GatewayID)
        {
            this.DeviceAddress = DeviceAddress;
            this.DeviceTag = DeviceTag;
            this.GatewayID = GatewayID;
        }
    }
}
