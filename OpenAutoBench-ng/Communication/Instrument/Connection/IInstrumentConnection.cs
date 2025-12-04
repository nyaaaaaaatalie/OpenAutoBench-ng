namespace OpenAutoBench_ng.Communication.Instrument.Connection
{
    public interface IInstrumentConnection
    {
        public void Connect();

        public void Disconnect();

        public Task Write(string toSend);

        public Task<string> Send(string toSend);

        public Task Transmit(string toSend);

        public Task<string> ReadLine();

        public Task TransmitByte(byte[] toSend);

        public Task<byte[]> ReceiveByte();

        public Task FlushBuffer();

        public void SetDelimeter(string delimeter);
    }
}
