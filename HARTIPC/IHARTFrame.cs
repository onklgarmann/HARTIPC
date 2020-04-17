using System;
using System.Collections.Generic;
using System.Text;

namespace HARTIPC
{
    public interface IHARTFrame
    {
        AddressFormat AddressFormat { get; }
        FrameType FrameType { get; }
        int GetLength();
        byte Command { get; }
        byte[] GetAddress();
        byte[] ToArray();
    }
}
