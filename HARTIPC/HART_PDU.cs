using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HARTIPC_test
{
    //skriver om denne klassen 12.04.20
    class HART_PDU
    {
        public byte StartDelimiter { get; }
        byte[] Address { get; } = { 0x00 };
        public byte Command { get; }
        public byte ByteCount { get; }
        public byte[] Payload { get; }
        public UInt32 Length { get; }
        private bool ShortAddress { get; }
        public bool STX { get; set;  }
        private byte Checksum { get; set; } = 0x00;
        private byte ResponseCode { get; set; }
        private byte DeviceStatus { get; set; }
        public byte[] PDU{ get; }
        protected HART_PDU() => ByteCount = 0x00;

        /* Constructor med argument for polling eller uniqueID (shortAddress t/f), 
         * addresse(skal være 1 eller 5 bytes), og command som er 1 byte.
         * uten payload er PDU alltid STX
         */
        public HART_PDU(bool shortAddress, byte[] address, ushort command) : this(shortAddress, true, address, command, null) { }
        
        /* Constructor med argument for polling eller uniqueID (shortAddress t/f), 
         * er det STX eller ACK(bool STX).
         * addresse(skal være 1 eller 5 bytes), og command som er 1 byte.
         * payload er data, men ved ACK (STX = false) er to første bytes = response code og status
         * hvis STX | payload = minst 1 byte, hvis ACK | payload = minst to bytes.
         */
        public HART_PDU(bool shortAddress, bool STX, byte[] address, ushort command, byte[] payload)
        {
            //argument validation kanskje
            ShortAddress = shortAddress;
            if (ShortAddress)
                StartDelimiter = STX ? (byte)0x02 : (byte)0x06;
            else
                StartDelimiter = STX ? (byte)0x82 : (byte)0x86;
            Address = address;
            Command = (byte)command;
            if (!STX)
            {
                ByteCount = (byte)payload.Length;
                ResponseCode = payload[0];
                DeviceStatus = payload[1];
                Payload = payload;
            }
            PDU = PDU2Binary();
        }
        /*Constructor tar binary array, må være minst 9 bytes(header + checksum)
         * hvis ACK må den være minst 11 bytes.
         */
        public HART_PDU(byte[] binaryPDU)
        {
            ushort nextByte = 0;
            foreach (byte b in binaryPDU[0..^1])
                Checksum ^= b;
            StartDelimiter = binaryPDU[nextByte++];
            STX = (StartDelimiter & (1 << 3 - 1)) == 0 ? true : false;
            ShortAddress = (StartDelimiter & (1 << 8 - 1)) == 0 ? true : false;
            Console.WriteLine(binaryPDU.Length);
            if ((ShortAddress && binaryPDU.Length < 5) || (!ShortAddress && binaryPDU.Length < 9))
                throw new Exception("binaryPDU too short");
            else if (Checksum != binaryPDU[^1])
                throw new Exception("Checksum mismatch");
            if (ShortAddress)
                Address.Append(binaryPDU[nextByte++]);
            else
            {
                Address = binaryPDU[nextByte..5];
                nextByte += 5;
            }
            Command = binaryPDU[nextByte++];
            ByteCount = binaryPDU[nextByte++];
            if (!STX)
            {
                ResponseCode = binaryPDU[nextByte++];
                DeviceStatus = binaryPDU[nextByte++];
                foreach (byte b in binaryPDU[nextByte++..^1])
                    Payload.Append(b);
            }
            else if (binaryPDU.Length > 9)
            {
                foreach (byte b in binaryPDU[nextByte++..^1])
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
