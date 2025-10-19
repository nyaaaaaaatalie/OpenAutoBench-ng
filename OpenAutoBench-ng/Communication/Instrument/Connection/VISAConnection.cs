using Ivi.Visa;
using NationalInstruments.Visa;
using System.Text;

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
                // Set a timeout of 500 ms
                _mb.TimeoutMilliseconds = 500;
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
        /// Send a command and don't expect a response
        /// </summary>
        /// <param name="toSend"></param>
        /// <returns></returns>
        public async Task Write(string toSend)
        {
            if (!connected) { throw new Exception("VISA disconnected"); }
            // Log
            Console.WriteLine($"VISA: >>SNT>> {toSend} ({BitConverter.ToString(Encoding.UTF8.GetBytes(toSend))})");
            // Send
            _mb.RawIO.Write(toSend);
        }

        /// <summary>
        /// Send a command and expect a response
        /// </summary>
        /// <param name="toSend"></param>
        /// <returns></returns>
        public async Task<string> Send(string toSend)
        {
            if (!connected) { throw new Exception("VISA disconnected"); }
            // Log
            Console.WriteLine($"VISA: >>SNT>> {toSend} ({BitConverter.ToString(Encoding.UTF8.GetBytes(toSend))})");
            // Send the command string
            _mb.RawIO.Write(toSend);
            // Read back a line (try 5 times, 5 * 500ms = 2.5 seconds for a timeout)
            int tries = 5;
            while (tries > 0)
            {
                try
                {
                    string resp = _mb.RawIO.ReadString();
                    // Log
                    Console.WriteLine($"VISA: <<RCV<< {resp} ({BitConverter.ToString(Encoding.UTF8.GetBytes(resp))})");
                    // Return
                    return resp;
                }
                catch (IOTimeoutException ex)
                {
                    Console.WriteLine($"VISA: Read timeout, retrying...{6 - tries}/5");
                }
                tries--;
            }
            throw new TimeoutException("VISA: Failed to read data within timeout window");
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
