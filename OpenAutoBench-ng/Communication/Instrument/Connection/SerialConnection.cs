using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using OpenAutoBench_ng.OpenAutoBench;

namespace OpenAutoBench_ng.Communication.Instrument.Connection
{
    public class SerialConnection : IInstrumentConnection
    {
        public string PortName { get; private set; }

        private SerialPort _serialPort { get; set; }

        private int retries = 3;

        // opens an 115200 8n1 port without flow control
        public SerialConnection(string portName, int baudrate, Settings.SerialNewlineType delimeter = Settings.SerialNewlineType.LF, bool useDTR = false)
        {
            _serialPort = new SerialPort(portName);
            _serialPort.BaudRate = baudrate;
            // set 3sec timeout
            _serialPort.ReadTimeout = 3000;
            switch (delimeter)
            {
                case Settings.SerialNewlineType.LF:
                    _serialPort.NewLine = "\n";
                    break;
                case Settings.SerialNewlineType.CR:
                    _serialPort.NewLine = "\r";
                    break;
                case Settings.SerialNewlineType.CRLF:
                    _serialPort.NewLine = "\r\n";
                    break;
            }
            _serialPort.DtrEnable = useDTR;
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
            for (int i = 0; i < retries; i++)
            {
                // Send command
                await Transmit(toSend);
                // Try to read line
                try
                {
                    string data = await ReadLine();
                    Console.WriteLine($"Serial Receive: {data} ({BitConverter.ToString(Encoding.Default.GetBytes(data)).Replace("-", " ")}");
                    return data;
                }
                catch (TimeoutException)
                {
                    Console.WriteLine($"Serial received attempt {i}/{retries} failed, retrying");
                }
            }
            throw new TimeoutException($"Failed to get response to command {toSend} after {retries} retries!");
        }

        /// <summary>
        /// Transmit data, but not receive
        /// </summary>
        /// <param name="toSend"></param>
        /// <returns></returns>
        public async Task Transmit(string toSend)
        {
            Console.WriteLine($"Serial Transmit: {toSend} ({BitConverter.ToString(Encoding.Default.GetBytes(toSend)).Replace("-"," ")})");
            _serialPort.WriteLine(toSend);
        }

        public async Task<string> ReadLine()
        {
            try
            {
                return _serialPort.ReadLine();
            }
            catch (TimeoutException)
            {
                byte[] buffer = new byte[_serialPort.BytesToRead];
                _serialPort.Read(buffer, 0, _serialPort.BytesToRead);
                Console.WriteLine($"Did not receive full serial line before timeout!");
                Console.WriteLine($"    Bytes received: {Encoding.UTF8.GetString(buffer)} ({BitConverter.ToString(buffer).Replace("-"," ")})");
                throw new TimeoutException();
            }
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
