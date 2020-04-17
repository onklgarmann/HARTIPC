using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HARTIPC
{
    public enum MessageType : byte { Request = 0x00, Response = 0x01, Publish = 0x02, NAK = 0x0F } 
    public enum MessageID : byte { Initiate = 0x00, Close = 0x01, KeepAlive = 0x02, PDU = 0x03, Discovery = 0x80 }
    public class HARTIPFrame 
    {
        public byte Version { get; set; }
        public MessageType MessageType { get; set; }
        public MessageID MessageID { get; set; }
        public byte StatusCode { get; set; }
        public ushort SequenceNumber { get; set; }
        public ushort ByteCount { get; set; } = 0x08;
        public IHARTFrame HARTFrame { get; set; }
        public HARTIPFrame(ushort sequenceNumber, byte version = 0x01, MessageType messageType = MessageType.Request, MessageID messageID = MessageID.KeepAlive, byte statusCode = 0x00)
        {
            Version = version;
            MessageType = messageType;
            MessageID = messageID;
            StatusCode = statusCode;
            SequenceNumber = sequenceNumber;
        }
        public HARTIPFrame(byte[] binary)
        {
            throw new NotImplementedException();
        }
        public byte[] Serialize()
        {
            List<byte> Header = new List<byte> { Version, (byte)MessageType, (byte)MessageID, StatusCode };
            byte[] seq = BitConverter.GetBytes(SequenceNumber);
            byte[] btc = BitConverter.GetBytes(ByteCount);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(seq, 0, seq.Length);
                Array.Reverse(btc, 0, btc.Length);
            }
            Header.AddRange(seq);
            Header.AddRange(btc);
            return Header.ToArray();
        }
        public byte[] Serialize(IHARTFrame frame)
        {
            if (frame != null)
            {
                ByteCount = (ushort)frame.GetLength();
                return this.Serialize().Concat(frame.Serialize()).ToArray();
            }
            else
                throw new ArgumentNullException(nameof(frame));
        }
        public byte[] Serialize(byte[] binary)
        {
            if (binary != null)
            {
                ByteCount = (ushort)binary.Length;
                byte[] CompleteFrame = Serialize();
                return this.Serialize().Concat(binary).ToArray();
            }
            else
                throw new ArgumentNullException(nameof(binary));
        }

    }
}
