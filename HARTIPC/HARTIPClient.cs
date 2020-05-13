using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace HARTIPC
{
    class DataEntryEventArgs : EventArgs
    {
        public DataEntryEventArgs(HARTFrame frame)
        {
            Address = frame.GetAddress();
            Current = BitConverter.ToSingle(frame.GetPayload()[0..4].Reverse().ToArray());
            Variables = new List<Tuple<byte, float>>() { new Tuple<byte, float>(frame.GetPayload()[4], BitConverter.ToSingle(frame.GetPayload()[5..9].Reverse().ToArray())) };
            if (frame.GetPayload().Length >= 14)
                Variables.Add(new Tuple<byte, float>(frame.GetPayload()[9], BitConverter.ToSingle(frame.GetPayload()[10..14].Reverse().ToArray())));
            if (frame.GetPayload().Length >= 19)
                Variables.Add(new Tuple<byte, float>(frame.GetPayload()[14], BitConverter.ToSingle(frame.GetPayload()[15..19].Reverse().ToArray())));
            if (frame.GetPayload().Length >= 24)
                Variables.Add(new Tuple<byte, float>(frame.GetPayload()[19], BitConverter.ToSingle(frame.GetPayload()[20..24].Reverse().ToArray())));
        }
        public DateTime datetime = DateTime.UtcNow;
        public byte[] Address { get; set; }
        public float Current { get; set; }
        public List<Tuple<byte, float>> Variables { get; set; }
    }
    class HARTIPClient : IDisposable
    {
        TcpClient client;
        NetworkStream stream;
        IPEndPoint server { get; set; }
        ushort SequenceNumber { get; set; } = 1;
        bool IsConnected { get; set; }
        public event EventHandler<DataEntryEventArgs> DataEntryReceived;
        
        public HARTIPClient(IPEndPoint server)
        {
            this.client = new TcpClient();
            this.server = server;
            
        }

        public void Connect()
        {
            client.Connect(server.Address.ToString(), server.Port);
            stream = client.GetStream();
            IsConnected = true;
        }
        public bool Initiate(int timeout)
        {
            byte[] initiateTimeout = new byte[] { 0x01 };
            initiateTimeout = initiateTimeout.Concat(BitConverter.GetBytes(timeout).Reverse()).ToArray();
            var frame = new HARTIPFrame(SequenceNumber, messageID: MessageID.Initiate, payload: initiateTimeout);
            stream.Write(frame.Serialize());
            SequenceNumber++;
            var buffer = new Byte[13];
            stream.Read(buffer, 0, buffer.Length);
            var response = new HARTIPFrame(buffer);
            if (frame.GetPayload().SequenceEqual(response.GetPayload()))
                return true;
            else
                return false;
        }
        public bool KeepAlive()
        {
            var frame = new HARTIPFrame(SequenceNumber).Serialize();
            stream.Write(frame);
            var buffer = new Byte[256];
            stream.Read(buffer, 0, buffer.Length);
            var response = new HARTIPFrame(buffer).Serialize();
            frame[1] = 0x01;
            SequenceNumber++;
            if (frame.SequenceEqual(response))
                return true;
            else
                return false;
        }
        public HARTFrame PDU(HARTFrame pdu)
        {
            var frame = new HARTIPFrame(SequenceNumber, messageID: MessageID.PDU, payload: pdu.Serialize());
            stream.Write(frame.Serialize());
            var buffer = new Byte[256];
            stream.Read(buffer, 0, buffer.Length);
            var response = new HARTFrame(new HARTIPFrame(buffer).GetPayload());
            if (response.Command == 77)
                OnDataEntryReceived(new HARTFrame(response.GetPayload()[2..]));
            return response;
        }
        protected virtual void OnDataEntryReceived(HARTFrame frame)
        {
            DataEntryReceived?.Invoke(this, new DataEntryEventArgs(frame));
        }
        

        public void Dispose()
        {
            stream.Close();
            client.Close();
        }
    }
}
