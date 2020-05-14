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
        public event EventHandler<DataEntryEventArgs> DataEntryReceived;
        public byte[] GatewayAddress { get; set; }
        public string GatewayName { get; set; }
        List<Tuple<byte[], string>> Devices { get; set; } = new List<Tuple<byte[], string>>();
        public HARTIPClient(IPEndPoint server)
        {
            this.client = new TcpClient();
            this.server = server;
            Connect();
            if (Initiate(600000))
            {
                MapNetwork();
            }
        }

        public void Connect()
        {
            client.Connect(server.Address.ToString(), server.Port);
            stream = client.GetStream();
            
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
            SequenceNumber++;
            if (response.Command == 77)
            {
                var innerHart = new HARTFrame(response.GetPayload()[2..]);
                if (innerHart.Command == 3)
                    OnDataEntryReceived(innerHart);
            }
            return response;
        }
        private void MapNetwork()
        {
            var frame0 = new HARTFrame(new byte[] { 0x00 }, 0);
            var response0 = PDU(frame0);
            GatewayAddress = HARTFrame.GetAddress(response0.GetPayload()[1..3], response0.GetPayload()[9..12]);
            var frame20 = new HARTFrame(GatewayAddress, 20);
            var response20 = PDU(frame20);
            GatewayName = Encoding.ASCII.GetString(response20.GetPayload()).TrimEnd('\0');
            var frame74 = new HARTFrame(GatewayAddress, 74);
            var response74 = PDU(frame74);
            var deviceCount = BitConverter.ToInt16(response74.GetPayload()[3..5].Reverse().ToArray())-1;
            for (ushort i = 1; i <= deviceCount; i++)
            {
                var frame84 = new HARTFrame(GatewayAddress, 84, BitConverter.GetBytes(i).Reverse().ToArray());
                var response84 = PDU(frame84);
                Devices.Add(new Tuple<byte[], string>(HARTFrame.GetAddress(response84.GetPayload()[6..11]), Encoding.ASCII.GetString(response84.GetPayload())[12..44].TrimEnd('\0')));
            }
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
