using System;
using System.Collections.Generic;
using System.Linq;
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
    public class HARTFrame : IHARTFrame
    {
        private byte[] _Address;
        private byte _HeaderLength;
        private byte _StartDelimiter =>
           (this.AddressFormat, this.FrameType) switch
           {
               { AddressFormat: AddressFormat.Polling, FrameType: FrameType.STX } => 0x02,
               { AddressFormat: AddressFormat.Polling, FrameType: FrameType.ACK } => 0x06,
               { AddressFormat: AddressFormat.UniqueID, FrameType: FrameType.STX } => 0x82,
               { AddressFormat: AddressFormat.UniqueID, FrameType: FrameType.ACK } => 0x86,
               { AddressFormat: _, FrameType: _ } => throw new ArgumentNullException(nameof(_StartDelimiter)),
           };
        public AddressFormat AddressFormat { get; set; } = AddressFormat.Polling;
        public FrameType FrameType { get; set; } = FrameType.STX;  // always STX, ACK will always have payload.
        public byte Command { get; private set; }
        public byte ByteCount { get; protected set; } = 0x00;

        public HARTFrame(byte[] address, byte command)
        {
            if (ValidAddress(ref address))
                _Address = address;
            Command = command;
        }

        private bool ValidAddress(ref byte[] address)
        {
            if (address != null)
            {
                switch (address.Length)
                {
                    case 1:
                        AddressFormat = AddressFormat.Polling;
                        _HeaderLength = 0x04;
                        return true;
                    case 5:
                        AddressFormat = AddressFormat.UniqueID;
                        _HeaderLength = 0x04;
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(address));
                }
            }
            throw new ArgumentNullException(nameof(address));
        }
        public byte[] Serialize() 
        {
            return this.ToByteList().ToArray();
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
        public int GetLength() { return (_HeaderLength + ByteCount + 1); }
    }
    public class HARTFrameWithPayload : HARTFrame
    {
        internal byte[] _Payload;
        public byte Checksum { get; private set; } = 0x00;  // checksum = XOR of all bytes.
        public HARTFrameWithPayload(byte[] address, byte command, byte[] payload) : base(address, command)
        {
            if (payload != null)
            {
                _Payload = payload;
                ByteCount = (byte)(_Payload.Length);
                
            }
            else
                throw new ArgumentNullException(nameof(payload));
        }
        protected override List<byte> ToByteList()
        {
            List<byte> CompleteFrame = base.ToByteList();
            CompleteFrame.AddRange(_Payload);
            CalcChecksum();
            CompleteFrame.Add(Checksum);
            return CompleteFrame;
        }
        internal void CalcChecksum()
        {
            foreach(byte b in base.ToByteList())
            {
                Checksum ^= b;   
            }
            foreach (byte b in _Payload)
            {
                Checksum ^= b;
            }
        }
        public byte[] GetPayload() { return _Payload; }
    }
    public class HARTFrameACK : HARTFrameWithPayload
    {
        internal new byte[] _Payload;
        public byte ResponseCode { get; private set; }  // responsecode only in ACK-frames.
        public byte DeviceStatus { get; private set; }  // Device status only in ACK-frames
        public HARTFrameACK(byte[] address, byte command, byte[] payload) : base(address, command, payload)
        {
            FrameType = FrameType.ACK;
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));
            else if (payload.Length < 2)
                throw new ArgumentOutOfRangeException(nameof(payload));
            ResponseCode = payload[0];
            DeviceStatus = payload[1];
            _Payload = payload[2..^1];
        }
    }
}
