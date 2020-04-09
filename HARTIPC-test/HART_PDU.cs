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
        private bool STX { get; }
        private byte Checksum { get; set; }
        public byte[] PDU{ get; }
        protected HART_PDU(){}
        public HART_PDU(bool shortAddress, bool STX, byte[] address, ushort command)
        {
            this.STX = STX;
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
            this.STX = STX;
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
            PDU = binaryPDU;
            Checksum = 0x00;
            foreach (byte b in PDU[0..^1])
                Checksum ^= b;
             if (PDU.Length < 9)
                throw new Exception("PDU too short");
            else if (Checksum != PDU[^1])
                throw new Exception("Checksum mismatch");
            StartDelimiter = PDU[0];
            STX = (StartDelimiter & (1 << 3 - 1)) == 0 ? true : false;
            ShortAddress = (StartDelimiter & (1 << 8 - 1)) == 0 ? true : false;
            if (ShortAddress)
                Address[0] = PDU[1];
            else
                Address = PDU[1..5];
            Command = PDU[6];
            ByteCount = PDU[7];
            if (PDU.Length > 9)
            {
                foreach (byte b in PDU[8..^1])
                    Payload.Append(b);
            }
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
