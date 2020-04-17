using System;
using System.Collections.Generic;
using System.Text;

namespace HARTIPC
{
    interface IHARTIPFrame
    {
        byte Version { get; set; }
        MessageType MessageType { get; set; }
        MessageID MessageID { get; set; }
        byte StatusCode { get; set; }
        ushort SequenceNumber { get; set; }
        ushort ByteCount { get; set; }
    }
}
