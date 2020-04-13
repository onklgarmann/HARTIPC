using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HARTIPC_test
{
    class HARTIPWrapper
    {
        public short sequenceNumber = 0;
        private short byteCount;

        public HARTIPWrapper()
        {

        }


        public byte[] WrapMessage(byte[] message)
        {
            sequenceNumber++;
            this.byteCount = (short)(8 + message.Length);
            return Header(1, 0, 3, 0, sequenceNumber, byteCount).Concat(message).ToArray();
        }
        public byte[] KeepAlive()
        {
            sequenceNumber++;
            this.byteCount = 8;
            return Header(1, 0, 2, 0, sequenceNumber, byteCount);
        }

        public byte[] Initiate()
        {
            sequenceNumber++;
            this.byteCount = 13;
            byte[] init = { 0x01, 0x00, 0x09, 0x27, 0xc0 }; // 01: Primary master, + 600000ms timeout
            return Header(1, 0, 0, 0, sequenceNumber, byteCount).Concat(init).ToArray();
        }

        public byte[] Close()
        {
            sequenceNumber++;
            this.byteCount = 8;
            return Header(1, 0, 1, 0, sequenceNumber, byteCount);
        }

        
        
        private byte [] Header( byte mVersion, byte mType, byte mID, byte mStatus, short mSequence, short mByteCount )
        {
            Queue<byte> header = new Queue<byte>();
            header.Enqueue(mVersion);
            header.Enqueue(mType);
            header.Enqueue(mID);
            header.Enqueue(mStatus);
            byte[] sequence = BitConverter.GetBytes(sequenceNumber);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(sequence);
            header.Enqueue(sequence[0]);
            header.Enqueue(sequence[1]);
            byte[] count = BitConverter.GetBytes(byteCount);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(count);
            header.Enqueue(count[0]);
            header.Enqueue(count[1]);
            return header.ToArray();
        }
        public int Length => byteCount;
    }
}
