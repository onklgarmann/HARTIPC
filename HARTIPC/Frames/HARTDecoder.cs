using System;
using System.Collections.Generic;
using System.Text;

namespace HARTIPC 
{ 
    public class HARTDecoder
    {
        private byte _Checksum { get; set; }
        private AddressFormat _AddressFormat { get; set; }
        private FrameType _FrameType { get; set; }
        private byte _Offset { get; set; }
        private byte _ByteCount { get; set; }
        public HARTDecoder() { }
        public IHARTFrame Decode(ref byte[] binary)
        {
            if (binary == null)
                throw new ArgumentNullException(nameof(binary));
            else if (binary.Length < 5)
                throw new ArgumentOutOfRangeException(nameof(binary));
            else if (!Enum.IsDefined(typeof(StartDelimiter), binary[0]))
                throw new Exception();
            _AddressFormat = ((binary[0] & (1 << 7)) == 0) ? AddressFormat.Polling : AddressFormat.UniqueID;
            _FrameType = ((binary[0] & (1 << 2)) == 0) ? FrameType.STX : FrameType.ACK;
            _Offset = (_AddressFormat == AddressFormat.UniqueID) ? (byte)0x04 : (byte)0x00;
            _ByteCount = binary[3 + _Offset];
            ValidChecksum(ref binary);
            switch (_FrameType)
            {
                case (FrameType.STX):
                    if (_ByteCount > 0)
                        return new HARTFrameWithPayload(binary[1..(2 + _Offset)], binary[2 + _Offset], binary[(4 + _Offset)..(4 + _Offset + _ByteCount + 1)]);
                    else
                        return new HARTFrame(binary[1..(2 + _Offset)], binary[2 + _Offset]);
                case (FrameType.ACK):
                    return new HARTFrameACK(binary[1..(2 + _Offset)], binary[2 + _Offset], binary[(4 + _Offset)..(4 + _Offset + _ByteCount + 1)]);
                default:
                    break;
            }
            return null;
        }   
        private void ValidChecksum(ref byte[] binary)
        {
            foreach (byte b in binary[0..(4 +_Offset+_ByteCount)])
            {
                _Checksum ^= b;
            }
            if (_Checksum != binary[(4 + _Offset + _ByteCount)])
                throw new Exception();
        }
        
    }
    
}
