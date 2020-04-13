using System;
using System.Collections.Generic;
using System.Text;

namespace HARTIPC
{
    public enum MessageType : byte { Request = 0x00, Response = 0x01, Publish = 0x02, NAK = 0x0F } 
    public enum MessageID : byte { Initiate = 0x00, Close = 0x01, KeepAlive = 0x02, PDU = 0x03, Discovery = 0x80 }
    class HARTIPFrame
    {
        public byte Version { get; set; }
        public MessageType MessageType { get; set; }
        public MessageID MessageID { get; set; }
        public byte StatusCode { get; set; }
        public ushort SequenceNumber { get; set; }
        public ushort ByteCount { get; set; }
        public HARTFrame HARTFrame { get; set; }
        public HARTIPFrame(ushort sequenceNumber, byte version = 0x01, MessageType messageType = MessageType.Request, MessageID messageID = MessageID.KeepAlive, byte statusCode = 0x00, HARTFrame frame = null)
        {
            if (messageID == MessageID.PDU && frame == null)
                throw new ArgumentException("HARTFrame cannot be null while MessageID is PDU(0x03)");
            Version = version;
            MessageType = messageType;
            MessageID = messageID;
            StatusCode = statusCode;
            SequenceNumber = sequenceNumber;
            this.HARTFrame = frame;
            ByteCount = (frame == null) ? (ushort)8 : (ushort)(8 + frame.ByteCount);
        }
        public byte[] ToArray()
        {
            List<byte> CompleteFrame = new List<byte>() { Version, (byte)MessageType, (byte)MessageID, StatusCode };
            CompleteFrame.AddRange(BitConverter.GetBytes(SequenceNumber));
            CompleteFrame.AddRange(BitConverter.GetBytes(ByteCount));
            switch (MessageID)
            {
                case MessageID.Initiate:
                    CompleteFrame.AddRange(new byte[] { 0x01, 0x00, 0x09, 0x27, 0xC0 });
                    break;
                case MessageID.Discovery:
                    CompleteFrame.AddRange(new byte[] { 0x4C, 0x0F, 0x38, 0xAC, 0x48, 0xAE, 0x49, 0x35, 0xB6, 0x89, 0x8F, 0x21, 0xF8, 0x5F, 0xC0, 0x3 });
                    break;
                case MessageID.PDU:
                    CompleteFrame.AddRange(HARTFrame.ToArray());
                    break;
            }
                
            return CompleteFrame.ToArray();
        }
    }
}
