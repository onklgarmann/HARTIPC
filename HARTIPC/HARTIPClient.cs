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
    /// <summary>
    /// HARTIPClient sets up TCP-connection to server and wraps and unwraps HART in HART-IP.
    /// 
    /// </summary>
    class HARTIPClient : IDisposable
    {
        /// <summary>
        /// TCP client
        /// </summary>
        TcpClient client;
        /// <summary>
        /// Network stream. Obtained from TCP client
        /// </summary>
        NetworkStream stream;
        /// <summary>
        /// IP address of HART-IP-server
        /// </summary>
        IPEndPoint server { get; set; }
        /// <summary>
        /// HART-IP sequence number
        /// </summary>
        ushort SequenceNumber { get; set; } = 1;
        /// <summary>
        /// Eventhandler for data entry
        /// </summary>
        public event EventHandler<DataEntryEventArgs> DataEntryReceivedEvent;
        /// <summary>
        /// Eventhandler for new gateway detected
        /// </summary>
        public event EventHandler<GatewayDataArgs> NewGatewayEvent;
        /// <summary>
        /// Eventhandler for ne device event
        /// </summary>
        public event EventHandler<DeviceDataArgs> NewDeviceEvent;
        /// <summary>
        /// Gateway address
        /// </summary>
        public byte[] GatewayAddress { get; set; }
        /// <summary>
        /// Gateway TAG - up to 32 bytes
        /// </summary>
        public string GatewayTag { get; set; }
        /// <summary>
        /// timout in milliseconds for keep-alive timer
        /// </summary>
        public int timeout { get; set; }
        /// <summary>
        /// polling interval for data requests
        /// </summary>
        public int pollingInterval { get; set; }
        /// <summary>
        /// Timer for sending keep-alives
        /// </summary>
        private Timer KeepAliveTimer;
        /// <summary>
        /// Timer for data requests.
        /// </summary>
        private Timer DataTimer;
        /// <summary>
        /// List to hold Devices
        /// </summary>
        public List<Tuple<byte[], string>> Devices { get; set; } = new List<Tuple<byte[], string>>();
        /// <summary>
        /// Constructor for client
        /// </summary>
        /// <param name="server">IPEndpoint</param>
        /// <param name="timeout">int</param>
        /// <param name="pollingInterval">int</param>
        public HARTIPClient(IPEndPoint server, int timeout, int pollingInterval)
        {
            this.client = new TcpClient();
            this.server = server;
            this.timeout = timeout;
            this.pollingInterval = pollingInterval;
        }
        /// <summary>
        /// Start the client
        /// </summary>
        public void Start()
        {
            Connect();
            if (Initiate(timeout))
            {
                // if connection is accepted, map the network. start timers and app
                MapNetwork();
                SetTimers(timeout, pollingInterval);
                OnDataTimerEvent(null, null);
                //keep looping until keep-alive timer is no longer set.
                while (KeepAliveTimer.Enabled == true)
                {

                }
            }
        }
        /// <summary>
        /// Set timers and events.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="pollingInterval"></param>
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
        /// <summary>
        /// Connect to socket
        /// </summary>
        public void Connect()
        {
            client.Connect(server.Address.ToString(), server.Port);
            stream = client.GetStream();
        }
        /// <summary>
        /// send initiate with timeout in ms
        /// </summary>
        /// <param name="timeout">int</param>
        /// <returns></returns>
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
                return true;  // return true if intiate is reciprocated
            else
                return false;
        }
        /// <summary>
        /// send keep-alive
        /// </summary>
        /// <returns>bool</returns>
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
                return true; //return true if answered
            else
                return false;
        }
        /// <summary>
        /// Send PDU to server
        /// </summary>
        /// <param name="pdu">HARTFrame</param>
        /// <returns>HARTFrame</returns>
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
        /// <summary>
        /// Map the network
        /// </summary>
        private void MapNetwork()
        {
            var frame0 = new HARTFrame(new byte[] { 0x00 }, 0);
            var response0 = PDU(frame0);
            GatewayAddress = HARTFrame.GetAddress(response0.GetPayload()[1..3], response0.GetPayload()[9..12]);
            var frame20 = new HARTFrame(GatewayAddress, 20);
            var response20 = PDU(frame20);
            GatewayTag = Encoding.ASCII.GetString(response20.GetPayload()).TrimEnd('\0');
            var GatewayData = new GatewayDataArgs(BitConverter.ToString(GatewayAddress).ToLower().Replace("-", string.Empty), GatewayTag);
            OnNewGatewayEvent(GatewayData);
            var frame74 = new HARTFrame(GatewayAddress, 74);
            var response74 = PDU(frame74);
            var deviceCount = BitConverter.ToInt16(response74.GetPayload()[3..5].Reverse().ToArray());
            for (ushort i = 1; i < deviceCount; i++)
            {
                var frame84 = new HARTFrame(GatewayAddress, 84, BitConverter.GetBytes(i).Reverse().ToArray());
                var response84 = PDU(frame84);
                var DeviceAddress = HARTFrame.GetAddress(response84.GetPayload()[6..11]);
                char[] charsToTrim = { '\0', ' ' };
                var DeviceTag = Encoding.ASCII.GetString(response84.GetPayload()[12..44]).Trim(charsToTrim);
                Console.WriteLine(BitConverter.ToString(response84.GetPayload()[12..44]));
                Console.WriteLine(DeviceTag.Length);
                OnNewDeviceEvent(new DeviceDataArgs(BitConverter.ToString(DeviceAddress).ToLower().Replace("-", string.Empty), DeviceTag, GatewayData.GatewayID));
                Devices.Add(new Tuple<byte[], string>(DeviceAddress, DeviceTag));
            }
        }
        // Events
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
        /// <summary>
        /// keep alive-event if keep-alive is answered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        //close
        public void Dispose()
        {
            client.Close();
        }
    }
}
