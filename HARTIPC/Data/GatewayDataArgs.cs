using System;
using System.Collections.Generic;
using System.Text;

namespace HARTIPC.Data
{
    /// <summary>
    /// Class to hold gateway information
    /// </summary>
    class GatewayDataArgs : EventArgs
    {
        /// <summary>
        /// GatewayID, if it exists
        /// </summary>
        public int GatewayID { get; set; }
        /// <summary>
        /// GatewayAddress
        /// </summary>
        public string GatewayAddress { get; set; }
        /// <summary>
        /// GatewayTag
        /// </summary>
        public string GatewayTag { get; set; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="GatewayAddress">string(10)</param>
        /// <param name="GatewayTag">string(32)</param>
        public GatewayDataArgs(string GatewayAddress, string GatewayTag)
        {
            this.GatewayAddress = GatewayAddress;
            this.GatewayTag = GatewayTag;
        }
             
    }
}
