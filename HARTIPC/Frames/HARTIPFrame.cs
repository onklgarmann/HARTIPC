using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HARTIPC
{
    /// <summary>
    /// MessageTypes represents HART-IP flow-control with one byte.
    /// </summary>
    public enum MessageType : int { 
        /// <summary>
        /// Request-message = 0
        /// </summary>
        Request = 0x00, 
        /// <summary>
        /// Response-message = 1
        /// </summary>
        Response = 0x01, 
        /// <summary>
        /// Publish-message = 2
        /// </summary>
        Publish = 0x02, 
        /// <summary>
        /// NAK-message = 15
        /// </summary>
        NAK = 0x0F } 
    /// <summary>
    /// MessageID representes HART-IP-frames function with one byte
    /// </summary>
    public enum MessageID : int { 
        /// <summary>
        /// Initiate is sent to open a connection.
        /// </summary>
        Initiate = 0x00, 
        /// <summary>
        /// Close is sent to close the connection.
        /// </summary>
        Close = 0x01, 
        /// <summary>
        /// KeepAlives are sent regularly to keep the connection open.
        /// </summary>
        KeepAlive = 0x02, 
        /// <summary>
        /// PDU indicates that a HART-frame is attached as data.
        /// </summary>
        PDU = 0x03, 
        /// <summary>
        /// Discovery is sent as a broadcast on the network. Not implemented.
        /// </summary>
        Discovery = 0x80 }
    /// <summary>
    /// Represents a HART-IP-frame with or without data payload.
    /// </summary>
    public class HARTIPFrame //: IHARTIPFrame
    {
        /// <summary>
        /// HART-IP version, single byte
        /// </summary>
        public byte Version { get; set; }
        /// <summary>
        /// MessageType, predefined type
        /// </summary>
        public MessageType MessageType { get; set; }
        /// <summary>
        /// MessageID, predefined type
        /// </summary>
        public MessageID MessageID { get; set; }
        /// <summary>
        /// StatusCode, single byte
        /// </summary>
        public byte StatusCode { get; set; }
        /// <summary>
        /// SequenceNumber, 2 bytes
        /// </summary>
        public ushort SequenceNumber { get; set; }
        /// <summary>
        /// Bytecount, 2 bytes.  Default 8 for header without payload.
        /// </summary>
        public ushort ByteCount { get; set; } = 0x08;
        /// <summary>
        /// Optional payload, byte array.
        /// </summary>
        private byte[] _Payload;
        /// <summary>
        /// Constructor for <c>HARTIPFrame</c> that takes all values with defaults set up for KeepAlive-frame.
        /// </summary>
        /// <param name="sequenceNumber">ushort</param>
        /// <param name="version">single byte</param>
        /// <param name="messageType">MessageType</param>
        /// <param name="messageID">MessageID</param>
        /// <param name="statusCode">single byte</param>
        /// <param name="payload">optional byte array</param>
        public HARTIPFrame(ushort sequenceNumber, byte version = 0x01, MessageType messageType = MessageType.Request, MessageID messageID = MessageID.KeepAlive, byte statusCode = 0x00, byte[] payload = null)
        {
            Version = version;
            MessageType = messageType;
            MessageID = messageID;
            StatusCode = statusCode;
            SequenceNumber = sequenceNumber;
            if (payload != null)
            {
                // Add optional payload and increase byte count
                _Payload = payload;
                ByteCount += (ushort)payload.Length;
            }   
        }
        /// <summary>
        /// Constructor for HARTIPFrame-class. Populates values from byte array.
        /// </summary>
        /// <param name="binary">byte array</param>
        public HARTIPFrame(byte[] binary)
        {
            // Input validation
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
            // Convert two bytes to unsigned short and account for endianess.
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(seq, 0, seq.Length);
                Array.Reverse(btc, 0, btc.Length);
            }
            SequenceNumber = BitConverter.ToUInt16(seq);
            ByteCount = BitConverter.ToUInt16(btc);
            if (ByteCount > binary.Length)
                throw new ArgumentOutOfRangeException(nameof(binary));
            // Stash overflowing bytes in payload, but only 
            else if (binary.Length > 8)
                _Payload = binary[8..ByteCount];
        }
        /// <summary>
        /// Gets the first 8 bytes of the HART-IP frame.
        /// </summary>
        /// <returns>HART-IP header as byte array</returns>
        public byte[] GetHeader()
        {
            List<byte> Header = new List<byte> { Version, (byte)MessageType, (byte)MessageID, StatusCode };
            byte[] seq = BitConverter.GetBytes(SequenceNumber);
            byte[] btc = BitConverter.GetBytes(ByteCount);
            // Convert ushorts to byte arrays, account for endianness
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(seq, 0, seq.Length);
                Array.Reverse(btc, 0, btc.Length);
            }
            Header.AddRange(seq);
            Header.AddRange(btc);
            return Header.ToArray();
        }
        /// <summary>
        /// Serializes entire HART-IP frame.
        /// </summary>
        /// <returns>HART-IP frame as byte array</returns>
        public byte[] Serialize()
        {
            if (_Payload != null)
            {
                return this.GetHeader().Concat(GetPayload()).ToArray();
            }
            else
                return this.GetHeader();
        }
        /// <summary>
        /// Gets data payload from HART-IP frame.
        /// </summary>
        /// <returns>Payload as byte array</returns>
        public byte[] GetPayload() { return (byte[])_Payload.Clone(); }

    }
}
