using System.Data;
using System.Net.Sockets;

namespace OpenAutoBench_ng.Communication.Instrument.Connection
{
    public class IPConnection : IInstrumentConnection
    {
        private TcpClient _client;

        private NetworkStream _stream;

        private string _hostname;

        private int _port;

        private string _delimeter;

        public IPConnection(string hostname, int port, string delimeter = "\n")
        {
            _hostname = hostname;
            _delimeter = delimeter;

            if (port > 65535)
            {
                throw new ArgumentOutOfRangeException("port should be <65535");
            }

            _port = port;

            _client = new TcpClient(_hostname, _port);
            _stream = _client.GetStream();
        }

        public void Connect()
        {
            _stream = _client.GetStream();
        }

        public void Disconnect()
        {
            _stream.Close();
            _client.Close();
        }

        public async Task FlushBuffer()
        {
            await _stream.FlushAsync();
        }

        public async Task<byte[]> ReceiveByte()
        {
            byte[] data = new byte[1024];
            _stream.Read(data, 0, data.Length);
            return data;
        }

        public async Task Write(string toSend)
        {
            await Transmit(toSend);
        }

        public async Task<string> Send(string toSend)
        {
            await Transmit(toSend);

            return await ReadLine();
        }

        public async Task Transmit(string toSend)
        {
            toSend += _delimeter;
            byte[] dataOut = System.Text.Encoding.ASCII.GetBytes(toSend);
            _stream.Write(dataOut, 0, dataOut.Length);
        }

        public async Task<string> ReadLine()
        {
            var reader = new StreamReader(_stream);
            return await reader.ReadLineAsync();
        }

        public async Task TransmitByte(byte[] toSend)
        {
            _stream.Write(toSend, 0, toSend.Length);
        }

        public void SetDelimeter(string delimeter)
        {
            _delimeter = delimeter;
        }
    }
}
