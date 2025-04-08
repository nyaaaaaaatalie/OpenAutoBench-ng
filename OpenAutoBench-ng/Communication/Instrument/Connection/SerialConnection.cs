using System.IO.Ports;

namespace OpenAutoBench_ng.Communication.Instrument.Connection
{
    public class SerialConnection : IInstrumentConnection
    {
        public string PortName { get; private set; }

        private SerialPort _serialPort { get; set; }

        // opens an 115200 8n1 port without flow control
        public SerialConnection(string portName, int baudrate, string delimeter = "\n")
        {
            _serialPort = new SerialPort(portName);
            _serialPort.BaudRate = baudrate;
            // set 5sec timeout
            _serialPort.ReadTimeout = 5000;
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            _serialPort.DtrEnable = true;
            _serialPort.RtsEnable = true;

        }

        public void Connect()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _serialPort.Open();
        }

        public void Disconnect()
        {
            _serialPort.Close();
        }

        /// <summary>
        /// Transmit and receive data
        /// </summary>
        /// <param name="toSend"></param>
        /// <returns></returns>
        public async Task<string> Send(string toSend)
        {
            await Transmit(toSend);
            return await ReadLine();
        }

        /// <summary>
        /// Transmit data, but not receive
        /// </summary>
        /// <param name="toSend"></param>
        /// <returns></returns>
        public async Task Transmit(string toSend)
        {
            _serialPort.WriteLine(toSend);
        }

        public async Task<string> ReadLine()
        {
            return _serialPort.ReadLine();
        }

        public async Task TransmitByte(byte[] toSend)
        {
            _serialPort.Write(toSend, 0, toSend.Length);
        }

        public async Task<byte[]> ReceiveByte()
        {
            throw new NotImplementedException();
        }

        public async Task FlushBuffer()
        {
            await _serialPort.BaseStream.FlushAsync();
        }

        public void SetDelimeter(string delimeter)
        {
            _serialPort.NewLine = delimeter;
        }
    }
}
