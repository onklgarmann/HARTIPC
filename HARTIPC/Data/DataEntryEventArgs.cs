using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HARTIPC.Data
{
    /// <summary>
    /// Class to hold data ready for entry into DB.
    /// </summary>
    class DataEntryEventArgs : EventArgs
    {
        public DataEntryEventArgs(HARTFrame frame)
        {
            DeviceAddress = BitConverter.ToString(frame.GetAddress()).ToLower().Replace("-", string.Empty); 
            PVCurrent = BitConverter.ToSingle(frame.GetPayload()[0..4].Reverse().ToArray());
            if (float.IsNaN(PVCurrent)) { PVCurrent = -1; }
            PVUnit = frame.GetPayload()[4];
            PV = BitConverter.ToSingle(frame.GetPayload()[5..9].Reverse().ToArray());
            if (frame.GetPayload().Length >= 14)
            {
                SVUnit = frame.GetPayload()[9];
                SV = BitConverter.ToSingle(frame.GetPayload()[10..14].Reverse().ToArray());
            }
            if (frame.GetPayload().Length >= 19)
            {
                TVUnit = frame.GetPayload()[14];
                TV = BitConverter.ToSingle(frame.GetPayload()[15..19].Reverse().ToArray());
            }
            if (frame.GetPayload().Length >= 24)
            {
                TVUnit = frame.GetPayload()[19];
                TV = BitConverter.ToSingle(frame.GetPayload()[20..24].Reverse().ToArray());
            }
        }
        public DateTime MeasurementTime { get; set; } = DateTime.UtcNow;
        public string DeviceAddress { get; set; }
        public float PVCurrent { get; set; }
        public int PVUnit { get; set; }
        public float PV { get; set; }
        public int? SVUnit { get; set; } = null;
        public float? SV { get; set; } = null;
        public int? TVUnit { get; set; } = null;
        public float? TV { get; set; } = null;
        public int? QVUnit { get; set; } = null;
        public float? QV { get; set; } = null;
    }
}
