using Ivi.Visa;
using NationalInstruments.Visa;

namespace OpenAutoBench_ng.Communication.Instrument.Connection
{
    public class VISAConnection : IInstrumentConnection
    {
        /// <summary>
        /// VISA resource name for connection
        /// </summary>
        private string _resource { get; set; }

        /// <summary>
        /// Whether the connection is open
        /// </summary>
        private bool connected { get; set; }
        public bool IsConnected { get { return connected; } }

        /// <summary>
        /// Message-based session for VISA communication
        /// </summary>
        private IMessageBasedSession _mb { get; set; }

        public VISAConnection(string resourceName)
        {
            _resource = resourceName;
            connected = false;
        }

        public void Connect()
        {
            using (ResourceManager resourceManager = new ResourceManager())
            {
                _mb = (MessageBasedSession)resourceManager.Open(_resource);
            }
            connected = true;
        }

        public void Disconnect()
        {
            if (_mb != null)
                _mb.Dispose();
            if (connected)
                connected = false;
        }

        /// <summary>
        /// Send a command and expect a response
        /// </summary>
        /// <param name="toSend"></param>
        /// <returns></returns>
        public async Task<string> Send(string toSend)
        {
            if (!connected) { throw new Exception("VISA disconnected"); }
            // Send the command string
            _mb.RawIO.Write(toSend);
            // Read back a line
            return _mb.RawIO.ReadString();
        }

        public async Task Transmit(string toSend)
        {
            if (!connected) { throw new Exception("VISA disconnected"); }
            _mb.RawIO.Write(toSend);
        }

        public async Task<string> ReadLine()
        {
            if (!connected) { throw new Exception("VISA disconnected"); }
            return _mb.RawIO.ReadString();
        }

        public async Task TransmitByte(byte[] toSend)
        {
            if (!connected) { throw new Exception("VISA disconnected"); }
            _mb.RawIO.Write(toSend, 0, toSend.Length);
        }

        public async Task<byte[]> ReceiveByte()
        {
            throw new NotImplementedException();
        }

        public async Task FlushBuffer()
        {
            _mb.Clear();
        }

        public void SetDelimeter(string delimeter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a list of available VISA instrument resources
        /// </summary>
        /// <returns>a list of VISA instrument resource names</returns>
        public static IEnumerable<string> GetInstrumentResources()
        {
            using (ResourceManager resourceManager = new ResourceManager())
                return resourceManager.Find("?*INSTR");
        }
    }
}
