using HARTIPC.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Timers;

namespace HARTIPC
{
    
    class HARTIPClient : IDisposable
    {
        TcpClient client;
        NetworkStream stream;
        IPEndPoint server { get; set; }
        ushort SequenceNumber { get; set; } = 1;
        public event EventHandler<DataEntryEventArgs> DataEntryReceivedEvent;
        public event EventHandler<GatewayDataArgs> NewGatewayEvent;
        public event EventHandler<DeviceDataArgs> NewDeviceEvent;
        public byte[] GatewayAddress { get; set; }
        public string GatewayTag { get; set; }
        public int timeout { get; set; }
        public int pollingInterval { get; set; }
        private Timer KeepAliveTimer;
        private Timer DataTimer;
        public List<Tuple<byte[], string>> Devices { get; set; } = new List<Tuple<byte[], string>>();
        public HARTIPClient(IPEndPoint server, int timeout, int pollingInterval)
        {
            this.client = new TcpClient();
            this.server = server;
            this.timeout = timeout;
            this.pollingInterval = pollingInterval;
        }
        public void Start()
        {
            Connect();
            if (Initiate(timeout))
            {
                MapNetwork();
                SetTimers(timeout, pollingInterval);
                OnDataTimerEvent(null, null);
                while (KeepAliveTimer.Enabled == true)
                {

                }
            }
        }
        private void SetTimers(int timeout, int pollingInterval)
        {
            KeepAliveTimer = new Timer(timeout/10);
            KeepAliveTimer.Elapsed += new ElapsedEventHandler(OnKeepAliveTimerEvent);
            KeepAliveTimer.AutoReset = false;
            KeepAliveTimer.Start();

            DataTimer = new Timer(pollingInterval);
            DataTimer.Elapsed += new ElapsedEventHandler(OnDataTimerEvent);
            DataTimer.AutoReset = false;
            DataTimer.Start();
        }
        public void Connect()
        {
            client.Connect(server.Address.ToString(), server.Port);
            stream = client.GetStream();
        }
        private bool Initiate(int timeout)
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
        private bool KeepAlive()
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
        private HARTFrame PDU(HARTFrame pdu)
        {
            var frame = new HARTIPFrame(SequenceNumber, messageID: MessageID.PDU, payload: pdu.Serialize());
            stream.Write(frame.Serialize());
            var buffer = new Byte[256];
            stream.Read(buffer, 0, buffer.Length);
            var response = new HARTFrame(new HARTIPFrame(buffer).GetPayload());
            SequenceNumber++;
            return response;
        }
        private void MapNetwork()
        {
            var frame0 = new HARTFrame(new byte[] { 0x00 }, 0);
            var response0 = PDU(frame0);
            GatewayAddress = HARTFrame.GetAddress(response0.GetPayload()[1..3], response0.GetPayload()[9..12]);
            var frame20 = new HARTFrame(GatewayAddress, 20);
            var response20 = PDU(frame20);
            GatewayTag = Encoding.ASCII.GetString(response20.GetPayload()).TrimEnd('\0');
            OnNewGatewayEvent(new GatewayDataArgs(BitConverter.ToString(GatewayAddress).ToLower().Replace("-", string.Empty), GatewayTag));
            var frame74 = new HARTFrame(GatewayAddress, 74);
            var response74 = PDU(frame74);
            var deviceCount = BitConverter.ToInt16(response74.GetPayload()[3..5].Reverse().ToArray());
            for (ushort i = 1; i < deviceCount; i++)
            {
                var frame84 = new HARTFrame(GatewayAddress, 84, BitConverter.GetBytes(i).Reverse().ToArray());
                var response84 = PDU(frame84);
                var DeviceAddress = HARTFrame.GetAddress(response84.GetPayload()[6..11]);
                var DeviceTag = Encoding.ASCII.GetString(response84.GetPayload())[12..44].TrimEnd('\0');
                OnNewDeviceEvent(new DeviceDataArgs(BitConverter.ToString(DeviceAddress).ToLower().Replace("-", string.Empty), DeviceTag, 1));
                Devices.Add(new Tuple<byte[], string>(DeviceAddress, DeviceTag));
            }
        }
        protected virtual void OnNewGatewayEvent(GatewayDataArgs e)
            => NewGatewayEvent?.Invoke(this, e);
        protected virtual void OnNewDeviceEvent(DeviceDataArgs e)
            => NewDeviceEvent?.Invoke(this, e);
        protected virtual void OnDataEntryReceivedEvent(HARTFrame frame) 
            => DataEntryReceivedEvent?.Invoke(this, new DataEntryEventArgs(frame));
        private void OnDataTimerEvent(object sender, ElapsedEventArgs e)
        {
            foreach (var device in Devices)
            {
                List<byte> payload = new List<byte>(new byte[] { 0x00, 0x00, 0x05, 0x82 });
                payload.AddRange(device.Item1);
                payload.AddRange(new byte[] { 0x03, 0x00 });
                var frame = new HARTFrame(GatewayAddress, 77, payload.ToArray());
                var response = PDU(frame);
                if (response.Command == 77)
                {
                    var innerHart = new HARTFrame(response.GetPayload()[2..]);
                    if (innerHart.Command == 3)
                        OnDataEntryReceivedEvent(innerHart);
                }
                if (response.ResponseCode == 0x00)
                    DataTimer.Start();
            }
            
        }
        private void OnKeepAliveTimerEvent(object sender, ElapsedEventArgs e)
        {
            if (KeepAlive())
                KeepAliveTimer.Start();
            else
            {
                KeepAliveTimer.Stop();
                Dispose();
            }
        }


        public void Dispose()
        {
            client.Close();
        }
    }
}
