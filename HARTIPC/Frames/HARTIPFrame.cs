using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HARTIPC
{
    public enum MessageType : byte { Request = 0x00, Response = 0x01, Publish = 0x02, NAK = 0x0F } 
    public enum MessageID : byte { Initiate = 0x00, Close = 0x01, KeepAlive = 0x02, PDU = 0x03, Discovery = 0x80 }
    public class HARTIPFrame : IHARTIPFrame
    {
        public byte Version { get; set; }
        public MessageType MessageType { get; set; }
        public MessageID MessageID { get; set; }
        public byte StatusCode { get; set; }
        public ushort SequenceNumber { get; set; }
        public ushort ByteCount { get; set; } = 0x08;
        private byte[] _Payload = null;
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
            if (binary == null)
                throw new ArgumentNullException(nameof(binary));
            else if (binary.Length < 8)
                throw new ArgumentOutOfRangeException(nameof(binary));
            Version = binary[0];
            MessageType = (MessageType)binary[1];
            MessageID = (MessageID)binary[2];
            StatusCode = binary[3];
            byte[] seq = binary[4..6];
            byte[] btc = binary[6..8];
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(seq, 0, seq.Length);
                Array.Reverse(btc, 0, btc.Length);
            }
            SequenceNumber = BitConverter.ToUInt16(seq);
            ByteCount = BitConverter.ToUInt16(btc);
            if (binary.Length > 8)
                _Payload = binary[8..ByteCount];
        }
        public byte[] SerializeHeader()
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
        public byte[] SerializeFrame()
        {
            if (_Payload != null)
                return SerializeHeader().Concat(_Payload).ToArray();
            else
                throw new Exception();
        }
        public byte[] Serialize(byte[] binary)
        {
            if (binary != null)
            {
                _Payload = binary;
                ByteCount += (ushort)binary.Length;
                return this.SerializeHeader().Concat(binary).ToArray();
            }
            else
                throw new ArgumentNullException(nameof(binary));
        }
        public byte[] GetPayload() { return (byte[])_Payload.Clone(); }

    }
}
