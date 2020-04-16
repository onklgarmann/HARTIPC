﻿using System;
using System.Collections.Generic;

namespace HARTIPC
{
    public enum FrameType { STX, ACK };
    public enum AddressFormat { Polling, UniqueID }
    public class HARTFrame
    {
        private byte[] _Address; // Address of frame, 1 or 5 bytes in array.
        private byte _HeaderLength { get; set; } = 0x04; // Offset +=4 if frame is uniqueID;
        private byte[] _Payload;  // potential payload.
        private byte _Offset { get; set; }
        #region Properties
        public AddressFormat AddressFormat { get; set; }
        public FrameType FrameType { get; set; }
        public byte StartDelimiter { get; private set; } // first byte of frame, 4 values possible.  sets STX, ShortAddress/Offset.
        public byte Command { get; private set; } // Command, 0-255.  Will limit options when scope is decided.
        public int ByteCount { get; private set; } = 0x00;  // ByteCount = number of bytes between ByteCount and Checksum
        public byte? ResponseCode { get; private set; }  // responsecode only in ACK-frames.
        public byte? DeviceStatus { get; private set; }  // Device status only in ACK-frames
        public byte Checksum { get; private set; } = 0x00;  // checksum = XOR of all bytes.

        #endregion
        public HARTFrame(byte[] address, byte command, AddressFormat addressFormat = AddressFormat.UniqueID, FrameType frameType = FrameType.STX, byte[] payload = null) // complete constructor with payload;
        {
            #region Validation
            // validate address length and payload length if frame is server response ACK
            AddressFormat = addressFormat;
            FrameType = frameType;
            if (address == null)
                throw new ArgumentNullException(nameof(address));
            if (!((AddressFormat == AddressFormat.Polling && address.Length == 1) || (AddressFormat == AddressFormat.UniqueID && address.Length == 5)))
                throw new ArgumentException("invalid address");
            if (FrameType == FrameType.ACK && (payload == null || payload.Length < 2))
                throw new ArgumentException("payload too short for ACK-frame");
            #endregion
            // set StartDelimiter and Offset from frametype and address format
            if (AddressFormat == AddressFormat.Polling)
                StartDelimiter = FrameType == FrameType.STX ? (byte)0x02 : (byte)0x06;
            else
            {
                StartDelimiter = FrameType == FrameType.STX ? (byte)0x82 : (byte)0x86;
                _Offset += 0x04;
                _HeaderLength += 0x04;
            }
            // Set Address and Command directly from input
            _Address = address;
            Command = command;
            // Set ByteCount based on frame type and payload length.
            ByteCount = (payload != null) ? payload.Length : 0x00;
            if (FrameType == FrameType.ACK)
            {
                ResponseCode = payload[0];
                DeviceStatus = payload[1];
                if (payload.Length > 2)
                    _Payload = payload[2..^1];
            }
            else if (payload != null)
                _Payload = payload;

        }
        public HARTFrame(byte[] binary)
        {
            //validate input
            if (binary == null)
                throw new ArgumentNullException(nameof(binary));
            else if (binary.Length < 5)
                throw new ArgumentOutOfRangeException(nameof(binary));
            // set StartDelimiter directly.
            StartDelimiter = binary[0];
            // set STX and ShortAddress based on StartDelimiter
            SetTypeAndFormat();
            
            // Validate minimum input length
            if ((AddressFormat == AddressFormat.Polling && binary.Length < 5) || (AddressFormat == AddressFormat.UniqueID && binary.Length < 9))
                throw new Exception("binaryPDU too short");

            // set Offset if using uniqueID;
            if (AddressFormat == AddressFormat.UniqueID)
            { _HeaderLength += 0x04; _Offset += 0x04; }
            // read address, command, and bytecount.
            _Address = binary[1..(2 + _Offset)];
            Command = binary[(2 + _Offset)];
            ByteCount = binary[(3 + _Offset)];

            if (ByteCount > 0)
            {
                foreach (byte b in binary[0..(_HeaderLength + ByteCount)])
                    Checksum ^= b;
                if (binary[(_HeaderLength + ByteCount)] != Checksum)
                    throw new Exception("Checksum mismatch");
            }
            // read potential response code and device status if ACK. read payload.
            if (FrameType == FrameType.STX)
                _Payload = binary[_HeaderLength..(_HeaderLength + ByteCount)];
            else if (FrameType == FrameType.ACK)
            {
                ResponseCode = binary[_HeaderLength];
                DeviceStatus = binary[_HeaderLength+1];
                _Payload = binary[(_HeaderLength+2)..(_HeaderLength + ByteCount)];
            }
        }
        #region Methods        
        public byte[] ToArray()
        {
            Checksum = 0x00; // reset checksum
            List<byte> CompleteFrame = new List<byte>(); // use list to build array
            CompleteFrame.Add(StartDelimiter);
            foreach (byte b in _Address)
                CompleteFrame.Add(b);
            CompleteFrame.Add(Command);
            CompleteFrame.Add((byte)ByteCount);
            // if response code is set, assume ACK and add response code and device status to array.
            if (ResponseCode != null)
            {
                CompleteFrame.Add((byte)ResponseCode);
                CompleteFrame.Add((byte)DeviceStatus);
            }
            // add payload if it exists.
            if (_Payload != null)
                foreach (byte b in _Payload)
                    CompleteFrame.Add(b);
            // Compute checksum and add to list.
            foreach (byte b in CompleteFrame)
                Checksum ^= b;
            CompleteFrame.Add(Checksum);
            return CompleteFrame.ToArray();
        }
        public byte[] GetAddress()
        {
            return (byte[])_Address.Clone();
        }
        public byte[] GetPayload()
        {
            return (byte[])_Payload.Clone();
        }
        private void SetTypeAndFormat()
        {
            AddressFormat = ((StartDelimiter & (1 << 7)) == 0) ? AddressFormat.Polling : AddressFormat.UniqueID;
            FrameType = ((StartDelimiter & (1 << 2)) == 0) ? FrameType.STX : FrameType.ACK;
        }
        public int GetLength()
        {
            return (_HeaderLength + ByteCount + 1);
        }
        #endregion


    }
}
