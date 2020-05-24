using System;
using System.Collections.Generic;
using System.Text;

namespace HARTIPC.Data
{
    /// <summary>
    /// DeviceDataArgs holds information on a HART-device.
    /// Properties matched to database for easy insertion with Dapper
    /// </summary>
    class DeviceDataArgs : EventArgs
    {
        /// <summary>
        /// DeviceAddress holds devices long format address(5-bytes)
        /// </summary>
        public string DeviceAddress { get; set; }
        /// <summary>
        /// DeviceTag holds device tag <= 32 bytes
        /// </summary>
        public string DeviceTag { get; set; }
        /// <summary>
        /// GatewayID holds ID-number of parent gateway
        /// </summary>
        public int GatewayID { get; set; }
        /// <summary>
        /// DeviceDataArgs constructor.
        /// </summary>
        /// <param name="DeviceAddress">String(10)</param>
        /// <param name="DeviceTag">String(32)</param>
        /// <param name="GatewayID">int</param>
        public DeviceDataArgs(string DeviceAddress, string DeviceTag, int GatewayID)
        {
            this.DeviceAddress = DeviceAddress;
            this.DeviceTag = DeviceTag;
            this.GatewayID = GatewayID;
        }
    }
}
