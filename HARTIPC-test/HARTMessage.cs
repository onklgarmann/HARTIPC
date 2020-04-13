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
        byte responseCode { get; set; }
        byte status { get; set; }
        byte[] payload { get; set; }
        byte chkSumByte = 0x00;

        public HARTMessage(byte[] packet)
        {
            UInt16 nextByte;
            
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
            {
                responseCode = packet[nextByte++];
                status = packet[nextByte++];
                if (byteCount != 0x00)
                    payload = packet.Skip(nextByte++).Take(byteCount-2).ToArray();
            }
            else
                payload = packet.Skip(nextByte++).Take(byteCount).ToArray();
               
            foreach (byte b in packet.Take(packet.Length - 1))
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
            if ((startDelimiter & (1 << 8 - 1)) == 0)
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
                packet.Enqueue(responseCode);
                packet.Enqueue(status);
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
        public void Print()
        {
            Console.WriteLine("Delimiter: \t{0}", startDelimiter.ToString("X2"));
            Console.WriteLine("Address: \t{0}", BitConverter.ToString(uniqueID.ToArray()));
            Console.WriteLine("Command: \t{0}", command.ToString("X2"));
            Console.WriteLine("Byte Count: \t{0}", byteCount.ToString("X2"));
            Console.WriteLine("Response Code: \t{0}", responseCode.ToString("X2"));
            Console.WriteLine("Status: \t{0}", status.ToString("X2"));
            Console.WriteLine("Payload: \t{0}", BitConverter.ToString(payload.ToArray()));
            Console.WriteLine("Checksum: \t{0}", chkSumByte.ToString("X2"));
        }

    }
}
