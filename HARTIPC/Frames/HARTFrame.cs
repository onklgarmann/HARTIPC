using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace HARTIPC
{
    public interface IHARTFrame
    {
        public AddressFormat AddressFormat { get; }
        public FrameType FrameType { get; }
        public byte Command { get; }
        public byte ByteCount { get; }
        public byte[] GetAddress();
        public int GetLength();
        public byte[] Serialize();
    }
    public enum FrameType { STX, ACK };
    public enum AddressFormat { Polling, UniqueID }

    /// <summary>
    /// HARTFrame-class
    /// This object holds the binary info from a HART-frame. Serializable.
    /// </summary>
    public class HARTFrame : IHARTFrame, ISerializable
    {
        #region fields and properties
        /// <summary>
        /// Start delimiter, indicates AddressFormat and FrameType,
        /// bit 7 sets long(1) or short(0) address
        /// bit 0-2 sets master-to-slave(STX, 010), or slave-to-master(ACK, 110)
        /// burst(BACK, 001) is not implemented. 
        /// </summary>
        private byte _StartDelimiter =>
           (this.AddressFormat, this.FrameType) switch
           {
               { AddressFormat: AddressFormat.Polling, FrameType: FrameType.STX } => 0x02,
               { AddressFormat: AddressFormat.Polling, FrameType: FrameType.ACK } => 0x06,
               { AddressFormat: AddressFormat.UniqueID, FrameType: FrameType.STX } => 0x82,
               { AddressFormat: AddressFormat.UniqueID, FrameType: FrameType.ACK } => 0x86,
               { AddressFormat: _, FrameType: _ } => throw new ArgumentNullException(nameof(_StartDelimiter)),
           };
        /// <summary>
        /// Address in bytes, 1 or 5 bytes.
        /// </summary>
        private byte[] _Address;
        /// <summary>
        /// AddressFormat is either Polling(1 byte), or UniqueID(5 bytes)
        /// </summary>
        public AddressFormat AddressFormat { get; set; } = AddressFormat.UniqueID;
        /// <summary>
        /// FrameType is STX or ACK
        /// </summary>
        public FrameType FrameType { get; set; } = FrameType.STX;
        /// <summary>
        /// Command byte
        /// </summary>
        public byte Command { get; private set; }
        /// <summary>
        /// ByteCount represents the number of bytes between header and checksum.
        /// </summary>
        public byte ByteCount { get; protected set; } = 0x00;
        /// <summary>
        /// Response code indicates outgoing communications error
        /// ACK-frames only
        /// </summary>
        public byte ResponseCode { get; private set; }
        /// <summary>
        /// Status code indicates device status
        /// ACK-frames only
        /// </summary>
        public byte StatusCode { get; private set; }
        /// <summary>
        /// Payload, if there is one
        /// </summary>
        internal byte[] _Payload;
        /// <summary>
        /// Checksum is XOR of all bytes
        /// </summary>
        public byte Checksum { get; private set; }
        #endregion
        public HARTFrame(byte[] address, byte command, byte[] payload = null)
        {
            
        }

        private bool IsValid()
        {
            return true;
        }
        public byte[] Serialize()
        {
            List<byte> CompleteFrame = ToByteList();
            Checksum = 0x00;
            foreach (byte b in CompleteFrame)
                Checksum ^= b;
            CompleteFrame.Add(Checksum);
            return CompleteFrame.ToArray();
        }
        protected virtual List<byte> ToByteList()
        {
            List<byte> CompleteFrame = new List<byte>(); // use list to build array
            CompleteFrame.Add(_StartDelimiter);
            CompleteFrame.AddRange(_Address);
            CompleteFrame.Add(Command);
            CompleteFrame.Add(ByteCount);
            return CompleteFrame;
        }
        public byte[] GetAddress() { return (byte[])_Address.Clone(); }
        public int GetLength() { return (_Address.Length + ByteCount + 1); }
        public byte[] GetPayload() { return (byte[])_Payload.Clone(); }
    }
}
