using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HARTIPC_test
{
    class HARTIP_Encoder
    {
        public ushort Version { get; private set; }
        ushort Identifier { get; set; }
        ushort Status { get; set; }
        UInt32 ByteCount { get; set; }
        UInt32 SequenceNumber { get; set; }


        ArrayList header = new ArrayList();
        public HARTIP_Encoder()
        {
            Version = 0x01;
            Status = 0x00;
        }
        public byte[] SessionInitiate(int InactivityCloseTimer = 600000)
        {

            SequenceNumber = 1;
            byte[] header = { 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x0d, 0x01 };
            byte[] timeout = BitConverter.GetBytes(InactivityCloseTimer);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(timeout);
            SequenceNumber++;
            return header.Concat(timeout).ToArray();
        }
        public byte[] KeepAlive()
        {
            byte[] header = { 0x01, 0x00, 0x02, 0x00 };
            byte[] sNumber = BitConverter.GetBytes(SequenceNumber);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(sNumber);
            byte[] bCount = { 0x00, 0x08 };
            SequenceNumber++;
            return header.Concat(sNumber.Concat(bCount)).ToArray();
        }
        public byte[] PDU(HART_PDU payload)
        {
            byte[] header = { 0x01, 0x00, 0x03, 0x00 };
            byte[] sNumber = BitConverter.GetBytes(SequenceNumber);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(sNumber);
            byte[] bCount = BitConverter.GetBytes(8+payload.Length);
            
            SequenceNumber++;
            return header.Concat(sNumber.Concat(bCount)).ToArray();
        }
        public byte[] SessionClose()
        {
            byte[] header = { 0x01, 0x00, 0x01, 0x00 };
            byte[] sNumber = BitConverter.GetBytes(SequenceNumber);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(sNumber);
            byte[] bCount = { 0x00, 0x08 };
            SequenceNumber = 1;
            return header.Concat(sNumber.Concat(bCount)).ToArray();
        }
    }
}
