using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace HARTIPC
{
    /// <summary>
    /// Predefined type to indicate message flow
    /// </summary>
    public enum FrameType { 
        /// <summary>
        /// Start of transmission, master-to-slave.
        /// </summary>
        STX, 
        /// <summary>
        /// Acknowledge message, slave-to-master.
        /// </summary>
        ACK };
    /// <summary>
    /// Predefined type to indicate short address(1 byte), or unique ID(long address, 5 bytes).
    /// </summary>
    public enum AddressFormat { 
        /// <summary>
        /// Polling address, 1 byte
        /// </summary>
        Polling, 
        /// <summary>
        /// Unique ID, long address, 5 bytes
        /// </summary>
        UniqueID }

    /// <summary>
    /// HARTFrame-class
    /// This object holds the binary info from a HART-frame. Serializable.
    /// </summary>
    public class HARTFrame
    {
        #region fields and properties
        /// <value>
        /// Start delimiter, indicates AddressFormat and FrameType,
        /// bit 7 sets long(1) or short(0) address
        /// bit 0-2 sets master-to-slave(STX, 010), or slave-to-master(ACK, 110)
        /// burst(BACK, 001) is not implemented. 
        /// </value>
        private byte _StartDelimiter =>
           (this.AddressFormat, this.FrameType) switch
           {
               { AddressFormat: AddressFormat.Polling, FrameType: FrameType.STX } => 0x02,
               { AddressFormat: AddressFormat.Polling, FrameType: FrameType.ACK } => 0x06,
               { AddressFormat: AddressFormat.UniqueID, FrameType: FrameType.STX } => 0x82,
               { AddressFormat: AddressFormat.UniqueID, FrameType: FrameType.ACK } => 0x86,
               { AddressFormat: _, FrameType: _ } => throw new ArgumentNullException(nameof(_StartDelimiter)),
           };
           
        /// <value>
        /// Address in bytes, 1 or 5 bytes.
        /// </value>
        private byte[] _Address;
        /// <value>
        /// AddressFormat is either Polling(1 byte), or UniqueID(5 bytes)
        /// </value>
        public AddressFormat AddressFormat { get; set; } = AddressFormat.UniqueID;
        /// <value>
        /// FrameType is STX or ACK
        /// </value>
        public FrameType FrameType { get; set; } = FrameType.STX;
        /// <value>
        /// Command byte
        /// </value>
        public byte Command { get; private set; }
        /// <value>
        /// ByteCount represents the number of bytes between header and checksum.
        /// </value>
        public byte ByteCount { get; protected set; } = 0x00;
        /// <value>
        /// Response code indicates outgoing communications error
        /// ACK-frames only
        /// </value>
        public byte ResponseCode { get; private set; }
        /// <value>
        /// Status code indicates device status
        /// ACK-frames only
        /// </value>
        public byte StatusCode { get; private set; }
        /// <value>
        /// Payload, if there is one
        /// </value>
        internal byte[] _Payload;
        /// <value>
        /// Checksum is XOR of all bytes
        /// </value>
        public byte Checksum { get; private set; }
        #endregion
        /// <summary>
        /// Constructor for HARTFrame object from values
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when arguments are out of range</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when non-nullable argumentes are null</exception>
        /// <param name="address">Byte array with 1 or 5 bytes.</param>
        /// <param name="command">Single byte</param>
        /// <param name="payload">Optional byte array</param>
        public HARTFrame(byte[] address, byte command = 0x03, byte[] payload = null)
        {
            // input validation
            if (address == null)
                throw new ArgumentNullException(nameof(address));
            else if (address.Length != 1 && address.Length != 5)
                throw new ArgumentOutOfRangeException(nameof(address));
            _Address = address;
            if (address.Length == 1)
                AddressFormat = AddressFormat.Polling;
            Command = command;
            if (payload != null)
            { 
                _Payload = payload; ByteCount = (byte)payload.Length; 
            }
            Checksum = CalculateChecksum();
        }
        /// <summary>
        /// Constructor for HARTFrame from byte array
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="binary"/> length is less than 8</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="binary"/> is null</exception>
        /// <param name="binary">Byte array, min. length 8</param>
        public HARTFrame(byte[] binary)
        {
            // input validation
            if (binary == null)
                throw new ArgumentNullException(nameof(binary));
            
            // pull AddressFormat and FrameType from first byte.
            AddressFormat = ((binary[0] & (1 << 7)) == 0) ? AddressFormat.Polling : AddressFormat.UniqueID;
            FrameType = ((binary[0] & (1 << 2)) == 0) ? FrameType.STX : FrameType.ACK;
            var offset = (AddressFormat == AddressFormat.Polling) ? 0 : 4;
            // input validation
            if (binary.Length < (5 + offset))
                throw new ArgumentOutOfRangeException(nameof(binary));
            _Address = binary[1..(2 + offset)];
            Command = binary[2 + offset];
            ByteCount = binary[3 + offset];
            if (4 + offset + ByteCount > binary.Length)
                throw new Exception(nameof(ByteCount));
            if (ByteCount >= 2 && FrameType == FrameType.ACK)
            {
                ResponseCode = binary[4 + offset];
                StatusCode = binary[5 + offset];
                _Payload = binary[(6 + offset)..(4 + offset + ByteCount)];
            }
            else if (ByteCount > 0)
                _Payload = binary[(4 + offset)..(4 + offset + ByteCount)];
            Checksum = ((4 + offset + ByteCount) >= binary.Length) ? CalculateChecksum() : binary[4 + offset + ByteCount];
            if (CalculateChecksum() != Checksum)
                throw new Exception(nameof(Checksum));
        }
        /// <summary>
        /// Calculates XOR of all bytes in frame
        /// </summary>
        /// <returns>Single byte</returns>
        private byte CalculateChecksum()
        {
            byte chksum = 0x00;
            chksum ^= _StartDelimiter;
            foreach (byte b in _Address)
                chksum ^= b;
            chksum ^= Command;
            chksum ^= ByteCount;
            chksum ^= ResponseCode;
            chksum ^= StatusCode;
            if (_Payload != null)
            {
                foreach (byte b in _Payload)
                    chksum ^= b;
            }
            return chksum;
        }
        /// <summary>
        /// Formats complete frame as a byte array.
        /// </summary>
        /// <returns>HARTFrame as a byte array</returns>
        public byte[] Serialize()
        {
            List<byte> CompleteFrame = new List<byte>(); // use list to build array
            CompleteFrame.Add(_StartDelimiter);
            CompleteFrame.AddRange(_Address);
            CompleteFrame.Add(Command);
            CompleteFrame.Add(ByteCount);
            if (FrameType == FrameType.ACK)
            {
                CompleteFrame.Add(ResponseCode);
                CompleteFrame.Add(StatusCode);
            }
            if (_Payload != null)
                CompleteFrame.AddRange(_Payload);
            CompleteFrame.Add(Checksum);
            return CompleteFrame.ToArray();
        }
        /// <summary>
        /// Gets address
        /// </summary>
        /// <returns>Byte array</returns>
        public byte[] GetAddress() { return (byte[])_Address.Clone(); }
        /// <summary>
        /// Gets length
        /// </summary>
        /// <returns>Integer</returns>
        public int GetLength() { return (4 + ByteCount + _Address.Length); }
        /// <summary>
        /// Get data payload
        /// </summary>
        /// <returns>Byte array</returns>
        public byte[] GetPayload() { return (byte[])_Payload.Clone(); }
        /// <summary>
        /// Sets 2 MSBs to 10 in combined DeviceType and DeviceID
        /// </summary>
        /// <param name="DeviceType">2 bytes</param>
        /// <param name="DeviceID">3 bytes</param>
        /// <returns>5-byte array</returns>
        public static byte[] GetAddress(byte[] DeviceType, byte[] DeviceID)
        {
            byte[] address = new byte[5];
            DeviceType[0] |= 1 << 7;
            DeviceType[0] |= 0 << 6;
            address[0] = DeviceType[0];
            address[1] = DeviceType[1];
            address[2] = DeviceID[0];
            address[3] = DeviceID[1];
            address[4] = DeviceID[2];
            return address;
        }
        public static byte[] GetAddress(byte[] Address)
        {
            Address[0] |= 1 << 7;
            Address[0] |= 0 << 6;
            return Address;
        }
    }
}
