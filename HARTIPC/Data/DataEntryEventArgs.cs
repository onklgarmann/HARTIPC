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
        /// <summary>
        /// Constructor for DataEntryEventArgs.  Takes HARTFrame object as argument.
        /// Populates properties from frame.  Matched to table fields in database for easy entry with Dapper
        /// </summary>
        /// <param name="frame">HARTFrame</param>
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
        /// <summary>
        /// Time of received measurement
        /// </summary>
        public DateTime MeasurementTime { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Address of measurement device
        /// </summary>
        public string DeviceAddress { get; set; }
        /// <summary>
        /// Devices analog current 4-20 mA
        /// </summary>
        public float PVCurrent { get; set; }
        /// <summary>
        /// Unit of primary variable
        /// </summary>
        public int PVUnit { get; set; }
        /// <summary>
        /// Primary Variable
        /// </summary>
        public float PV { get; set; }
        /// <summary>
        /// Unit of secondary variable, optional
        /// </summary>
        public int? SVUnit { get; set; } = null;
        /// <summary>
        /// Secondary Variable, optional
        /// </summary>
        public float? SV { get; set; } = null;
        /// <summary>
        /// Unit of tertiary variable, optional
        /// </summary>
        public int? TVUnit { get; set; } = null;
        /// <summary>
        /// Teritary Variable, optional
        /// </summary>
        public float? TV { get; set; } = null;
        /// <summary>
        /// Unit of quaternary variable, optional
        /// </summary>
        public int? QVUnit { get; set; } = null;
        /// <summary>
        /// Quaternary Variable, optional
        /// </summary>
        public float? QV { get; set; } = null;
    }
}
