using System.Data;
using System.Net.Sockets;
using System.Text;
using OpenAutoBench_ng.Communication.Instrument.Connection;

namespace OpenAutoBench_ng.Communication.Instrument.IFR_2975
{
    public class IFR_2975Connection : IInstrumentConnection
    {
        private IInstrumentConnection _connection;

        public IFR_2975Connection(IInstrumentConnection lower)
        {
            _connection = lower;
        }

        public void Connect()
        {
            _connection.Connect();
        }

        public void Disconnect()
        {
            _connection.Disconnect();
        }

        public async Task FlushBuffer()
        {
            await _connection.FlushBuffer();
        }

        public Task<byte[]> ReceiveByte()
        {
            throw new NotImplementedException();
        }

        public async Task Write(string toSend)
        {
            Console.WriteLine($"Writing: {toSend}");
            await _connection.FlushBuffer();
            toSend += "\n";
            byte[] dataOut = System.Text.Encoding.ASCII.GetBytes(toSend);
            await _connection.TransmitByte(dataOut);
        }

        public async Task<string> Send(string toSend)
        {
            Console.WriteLine($"Sending: {toSend}");
            await _connection.FlushBuffer();
            toSend += "\n";
            byte[] dataOut = System.Text.Encoding.ASCII.GetBytes(toSend);
            await _connection.TransmitByte(dataOut);
            byte[] dataIn = new byte[1024];
            var result = new List<string>();
            var foundResponse = false;
            while (!foundResponse)
            {
                await Task.Delay(200);
                string currentString = Encoding.ASCII.GetString(await _connection.ReceiveByte());
                Console.WriteLine($"Received from monitor: {currentString}");
                if(currentString.Contains('%'))
                {
                    foundResponse = true;
                }
                currentString = currentString.Replace("\0", "");
                currentString = currentString.Replace("\r", "");
                var lines = currentString.Split('\n');
                foreach(var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if(trimmedLine!="")
                    {
                        result.Add(trimmedLine);
                    }
                }
            }
            var endResult = "";
            foreach (var line in result)
            {
                var lineToAdd = line.Replace("??\\u0001", "").Replace("%","").Trim();
                if (lineToAdd != toSend.Trim() && lineToAdd != "")
                {
                    endResult += line.Trim() + "\n";
                }
            }
            Console.WriteLine($"Received from monitor (end): {endResult}");
            endResult.Trim();
            endResult = endResult.Trim('\n');
            return endResult;
        }

        public async Task Transmit(string toSend)
        {
            await _connection.TransmitByte(Encoding.ASCII.GetBytes(toSend));
            await Task.Delay(100);
            await _connection.ReceiveByte();
        }

        public Task TransmitByte(byte[] toSend)
        {
            throw new NotImplementedException();
        }

        public async Task<string> ReadLine()
        {
            throw new NotImplementedException();
        }

        public void SetDelimeter(string delimeter)
        {
            throw new NotImplementedException();
        }
    }
}
