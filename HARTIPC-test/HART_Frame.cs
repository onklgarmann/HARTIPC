using System;
using System.Collections.Generic;
using System.Text;

namespace HARTIPC_test
{
    class HART_Frame
    {
        public List<byte> PDU { get; private set; }
        public HART_Frame(byte[] address, byte command) : this(false, true, address, command)
        {

        }
        public HART_Frame(byte[] address, byte command, byte[] payload) : this(false, true, address, command, payload)
        {

        }
        public HART_Frame(bool shortAddress, bool STX, byte[] address, byte command) : this(shortAddress, STX, address, command, null)
        {

        }
        public HART_Frame(bool shortAddress, bool STX, byte[] address, byte command, byte[] payload)
        {
            #region Validation
            //validate address length and payload length if frame is server response ACK
            if (!((shortAddress && address.Length == 1) || (!shortAddress && address.Length == 5)))
                throw new ArgumentException("invalid address");
            if (!STX && payload.Length < 2)
                throw new ArgumentException("payload too short for ACK-frame");
            #endregion
            //initialize PDU-list
            PDU = new List<byte> { 0x00 };
            //Modify start delimiter
            if (shortAddress)
                PDU[0] = STX ? (byte)0x02 : (byte)0x06;
            else
                PDU[0] = STX ? (byte)0x82 : (byte)0x86;
            //add address, 1 or 5 bytes
            PDU.AddRange(address);
            //add command, 1 byte.
            PDU.Add(command);
            //add bytecount and potential payload.
            if (payload != null)
                PDU.Add((byte)(PDU.Count + payload.Length));
            else
                PDU.Add(0x00);
            //add checksum, XOR all bytes.
            byte Checksum = 0x00;
            foreach (byte b in PDU)
                Checksum ^= b;
            PDU.Add(Checksum);
        }
        /* STX leser og setter bit 2 i første byte av PDU. Denne indikerer
         om pakken er master-slave(0) eller slave-master(1)*/
        public bool STX
        {
            get => (PDU[0] & (1 << 2)) == 0 ? true : false;
            set => _ = (value) ? PDU[0] |= 1 << 2 : PDU[0] &= 255 ^ (1 << 2);
        }
        /* ShortAddress leser og setter første bit i PDU som indikerer
         * "polling address"-format(0) eller "uniqueID"(1)-format.
         * Dette bestemmer om adressen i pakken tar 1 eller 5 bytes */
        public bool ShortAddress
        {
            get => (PDU[0] & (1 << 7)) == 0 ? true : false;
            set
            {
                if (value)
                {
                    PDU[0] |= 1 << 7;
                    Offset = 0x00;
                }
                else
                {
                    PDU[0] &= 255 ^ (1 << 7);
                    Offset = 0x04;
                }
            }
            
        }
        public byte Command { get => PDU[2 + Offset]; }
        private byte Offset { get; set; }
        public byte ByteCount { get => (byte)(PDU.Count - 1); }
        
        

    }
}
