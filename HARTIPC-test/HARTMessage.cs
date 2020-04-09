using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HARTIPC_test
{
    class HARTMessage
    {
        bool master { get; set; }
        byte startDelimiter { get; set; }
        byte[] uniqueID { get; set; }
        byte address { get; set; }
        byte command { get; set; }
        byte byteCount { get; set; }
        byte[] status { get; set; }
        byte[] payload { get; set; }

        public HARTMessage(byte[] packet)
        {
            UInt16 nextByte;
            byte chkSumByte = 0x00;
            startDelimiter = packet[0];
            master = ((startDelimiter & (1 << 3 - 1)) == 0) ? true : false; //bit 3 indicates ACK or STX
            if ((startDelimiter & (1 << 8 - 1)) == 0)  //first bit indicates long or short address
            {
                //short address
                address = packet[1];
                nextByte = 2;
            }
            else
            {
                //long address
                uniqueID = packet.Skip(1).Take(5).ToArray();
                nextByte = 6;
            }
            command = packet[nextByte++];
            byteCount = packet[nextByte++];
            if (!master)
                status = packet.Skip(nextByte++).Take(2).ToArray();
            if (byteCount != 0x00)
                payload = packet.Skip(nextByte++).Take(byteCount).ToArray();
            foreach (byte b in packet.Take(packet.Length - 2))
            {
                chkSumByte ^= b;
            }
            if (packet[packet.Length-1] != chkSumByte)
                throw new Exception("Checksum mismatch");

        }
        public HARTMessage(ushort command, byte[] uniqueID)
        {
            master = true;
            startDelimiter = 0x82;
            this.uniqueID = uniqueID;
            this.command = (byte)command;
            this.payload = null;
        }
        HARTMessage(ushort command, byte address)
        {
            master = true;
            startDelimiter = 0x02;
            this.address = address;
            this.command = (byte)command;
            this.payload = payload;
        }
        HARTMessage(ushort command, byte[] uniqueID, byte[] payload)
        {
            master = true;
            startDelimiter = 0x82;
            this.uniqueID = uniqueID;
            this.command = (byte)command;
            this.payload = payload;
        }

        HARTMessage(ushort command, byte[] payload, byte address)
        {
            master = true;
            startDelimiter = 0x02;
            this.address = address;
            this.command = (byte)command;
            this.payload = payload;
        }
        public byte[] ToByteArray()
        {
            Queue<byte> packet = new Queue<byte>();
            packet.Enqueue(startDelimiter);
            if (uniqueID == null)
            {
                packet.Enqueue(address);
            }
            else
            {
                foreach(byte b in uniqueID)
                    packet.Enqueue(b);
            }
            packet.Enqueue(command);
            if (!master)
            {
                byteCount = (payload != null) ? (byte)(payload.Length+2) : (byte)0x00;
                packet.Enqueue(byteCount);
                packet.Enqueue(status[1]);
                packet.Enqueue(status[0]);
            }
            else
            {
                byteCount = (payload != null) ? (byte)(payload.Length) : (byte)0x00;
                packet.Enqueue(byteCount);
            }
            if (payload != null)
            {
                foreach (byte b in payload)
                    packet.Enqueue(b);
            }
            byte chkSumByte = 0x00;
            foreach (byte b in packet)
            {
                 chkSumByte ^= b;
            }
            packet.Enqueue(chkSumByte);
            return packet.ToArray();
        }

    }
}
