using System;
using System.Collections.Generic;

namespace HARTIPC
{
    public enum FrameType { STX, ACK };
    public enum AddressFormat { Polling, UniqueID }
    class HARTFrame
    {
        #region Properties
        public AddressFormat AddressFormat { get; set; }
        public FrameType FrameType { get; set; }
        private byte Offset { get; set; } = 0x00; // Offset +=4 if frame is uniqueID;
        public byte StartDelimiter { get; private set; } // first byte of frame, 4 values possible.  sets STX, ShortAddress/Offset.
        public byte[] Address { get; private set; } // Address of frame, 1 or 5 bytes in array.
        public byte Command { get; private set; } // Command, 0-255.  Will limit options when scope is decided.
        public int ByteCount { get; private set; } = 0x00;  // ByteCount = number of bytes between ByteCount and Checksum
        public byte? ResponseCode { get; private set; }  // responsecode only in ACK-frames.
        public byte? DeviceStatus { get; private set; }  // Device status only in ACK-frames
        public byte[] Payload { get; private set; }  // potential payload.
        public byte Checksum { get; private set; } = 0x00;  // checksum = XOR of all bytes.
        #endregion
        public HARTFrame( byte[] address, byte command, AddressFormat addressFormat = AddressFormat.UniqueID, FrameType frameType = FrameType.STX, byte[] payload = null) // complete constructor with payload;
        {
            #region Validation
            // validate address length and payload length if frame is server response ACK
            if (!((AddressFormat == AddressFormat.Polling && address.Length == 1) || (AddressFormat == AddressFormat.UniqueID && address.Length == 5)))
                throw new ArgumentException("invalid address");
            if (FrameType == FrameType.ACK && payload.Length < 2)
                throw new ArgumentException("payload too short for ACK-frame");
            #endregion
            // set StartDelimiter and Offset from STX and ShortAddress
            AddressFormat = addressFormat;
            FrameType = frameType;
            if (AddressFormat == AddressFormat.Polling)
                StartDelimiter = FrameType == FrameType.STX ? (byte)0x02 : (byte)0x06;
            else
            {
                StartDelimiter = FrameType == FrameType.STX ? (byte)0x82 : (byte)0x86;
                Offset = 0x04;
            }
            // Set Address and Command directly from input
            Address = address;
            Command = command;
            // Set ByteCount based on frame type and payload length.
            ByteCount = (payload != null) ? payload.Length : 0x00;
            if (FrameType == FrameType.ACK)
            {
                ResponseCode = payload[0];
                DeviceStatus = payload[1];
                if (payload.Length > 2)
                    Payload = payload[2..^1];
            }
            else if (payload != null)
                Payload = payload;

        }
        public HARTFrame(byte[] binary)
        {
            // calculate checksum and validate against input
            foreach (byte b in binary[0..^1])
                Checksum ^= b;
            if (binary[^1] != Checksum)
                throw new Exception("Checksum mismatch");
            // set StartDelimiter directly.
            StartDelimiter = binary[0];
            // set STX and ShortAddress based on StartDelimiter
            AddressFormat = ((StartDelimiter & (1 << 7)) == 0) ? AddressFormat.Polling : AddressFormat.UniqueID;
            FrameType = ((StartDelimiter & (1 << 2)) == 0) ? FrameType.STX : FrameType.ACK;
            // Validate minimum input length
            if ((AddressFormat == AddressFormat.Polling && binary.Length < 5) || (AddressFormat == AddressFormat.UniqueID && binary.Length < 9))
                throw new Exception("binaryPDU too short");
            // set Offset if using uniqueID;
            if (AddressFormat == AddressFormat.UniqueID)
                Offset = 0x04;
            // read address, command, and bytecount.
            Address = binary[1..(2 + Offset)];
            Command = binary[(2 + Offset)];
            ByteCount = binary[(3 + Offset)];
            // read potential response code and device status if ACK. read payload.
            if (FrameType == FrameType.STX)
                Payload = binary[(4 + Offset)..((4 + Offset) + (ByteCount))];
            else if (FrameType == FrameType.ACK)
            {
                ResponseCode = binary[5 + Offset];
                DeviceStatus = binary[6 + Offset];
                Payload = binary[(7 + Offset)..(ByteCount + 1)];
            }
        }
        #region Methods        
        public byte[] ToArray()
        {
            Checksum = 0x00; // reset checksum
            List<byte> CompleteFrame = new List<byte>(); // use list to build array
            CompleteFrame.Add(StartDelimiter);
            foreach (byte b in Address)
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
            if (Payload != null)
                foreach (byte b in Payload)
                    CompleteFrame.Add(b);
            // Compute checksum and add to list.
            foreach (byte b in CompleteFrame)
                Checksum ^= b;
            CompleteFrame.Add(Checksum);
            return CompleteFrame.ToArray();
        }
        /* print properties for debug purposes.
        public void Print()
        {
            Console.WriteLine("STX:\t\t\t{0}", STX);
            Console.WriteLine("ShortAddress:\t\t{0}", ShortAddress);
            Console.WriteLine("Offset:\t\t\t{0}", Offset.ToString("X2"));
            Console.WriteLine("StartDelimiter:\t\t{0}", StartDelimiter.ToString("X2"));
            Console.WriteLine("Address:\t\t{0}", BitConverter.ToString(Address));
            Console.WriteLine("Command:\t\t{0}", Command.ToString("X2"));
            Console.WriteLine("ByteCount:\t\t{0}", ByteCount);
            Console.WriteLine("ResponseCode:\t\t{0}", ResponseCode.ToString());
            Console.WriteLine("DeviceStatus:\t\t{0}", DeviceStatus.ToString());
            Console.WriteLine("Payload:\t\t{0}", BitConverter.ToString(Payload));
            Console.WriteLine("Checksum:\t\t{0}", Checksum.ToString("X2"));
        }*/
        #endregion


    }
}
