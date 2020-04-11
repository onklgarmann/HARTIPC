using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HARTIPC_test
{
    class HART_PDU
    {
        public byte StartDelimiter { get; }
        byte[] Address { get; }
        public byte Command { get; }
        public byte ByteCount { get; }
        public byte[] Payload { get; }
        public UInt32 Length { get; }
        private bool ShortAddress { get; }
        public bool STX { get; set;  }
        private byte Checksum { get; set; }
        private byte ResponseCode { get; set; }
        private byte DeviceStatus { get; set; }
        public byte[] PDU{ get; }
        protected HART_PDU(){}
        public HART_PDU(bool shortAddress, bool STX, byte[] address, ushort command)
        {
            ShortAddress = shortAddress;
            if (ShortAddress)
                StartDelimiter = STX ? (byte)0x02 : (byte)0x06;
            else
                StartDelimiter = STX ? (byte)0x82 : (byte)0x86;
            Address = address;
            Command = (byte)command;
            ByteCount = 0x00;
            Checksum = 0x00;
            PDU = PDU2Binary();
        }
        public HART_PDU(bool shortAddress, bool STX, byte[] address, ushort command, ref byte[] payload)
        {
            ShortAddress = shortAddress;
            if (ShortAddress)
                StartDelimiter = STX ? (byte)0x02 : (byte)0x06;
            else
                StartDelimiter = STX ? (byte)0x82 : (byte)0x86;
            Address = address;
            Command = (byte)command;
            ByteCount = (byte)payload.Length;
            Payload = payload;
            Checksum = 0x00;
            PDU = PDU2Binary();
        }
        public HART_PDU(byte[] binaryPDU)
        {
            Checksum = 0x00;
            foreach (byte b in binaryPDU[0..^1])
                Checksum ^= b;
             if (binaryPDU.Length < 9)
                throw new Exception("binaryPDU too short");
            else if (Checksum != binaryPDU[^1])
                throw new Exception("Checksum mismatch");
            StartDelimiter = binaryPDU[0];
            STX = (StartDelimiter & (1 << 3 - 1)) == 0 ? true : false;
            ShortAddress = (StartDelimiter & (1 << 8 - 1)) == 0 ? true : false;
            if (ShortAddress)
                Address[0] = binaryPDU[1];
            else
                Address = binaryPDU[1..5];
            Command = binaryPDU[6];
            ByteCount = binaryPDU[7];
            if (!STX)
            {
                ResponseCode = binaryPDU[8];
                DeviceStatus = binaryPDU[9];
                foreach (byte b in binaryPDU[10..^1])
                    Payload.Append(b);
            }
            else if (binaryPDU.Length > 9)
            {
                foreach (byte b in binaryPDU[8..^1])
                    Payload.Append(b);
            }
            PDU = binaryPDU;
        }
        private byte[] PDU2Binary()
        {
            List<byte> pdu = new List<byte>();
            pdu.Add(StartDelimiter);
            foreach (byte b in Address)
                pdu.Add(b);
            pdu.Add(Command);
            pdu.Add(ByteCount);
            if (ByteCount > 0)
            {
                foreach (byte b in Payload)
                    pdu.Add(b);
            }
            foreach (byte b in pdu)
                Checksum ^= b;
            pdu.Add(Checksum);
            
            return pdu.ToArray();
        }
        
    }
}
