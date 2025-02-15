using OpenAutoBench_ng.Communication.Instrument.Connection;

namespace OpenAutoBench_ng.Communication.Instrument.GeneralDynamics_R2670
{
    public class GeneralDynamics_R2670Instrument: IBaseInstrument
    {
        private IInstrumentConnection Connection;

        public bool Connected { get; private set; }

        public bool SupportsP25 { get { return false; } }

        public bool SupportsDMR { get { return false; } }

        public GeneralDynamics_R2670Instrument(IInstrumentConnection conn, int addr)
        {
            Connected = false;
            Connection = conn;
        }

        public async Task Connect()
        {
            Connection.Connect();
        }

        public async Task Disconnect()
        {
            Connection.Disconnect();
        }

        public async Task GenerateSignal(float power)
        {
            throw new NotImplementedException();
        }

        public async Task GenerateFMSignal(float power, float afFreq)
        {
            //GenerateSignal(power);
            throw new NotImplementedException();
        }

        public Task StopGenerating()
        { 
            throw new NotImplementedException();
        }

        public async Task SetGenPort(InstrumentOutputPort outputPort)
        {
            throw new NotImplementedException();
        }

        public async Task SetRxFrequency(int frequency)
        {
            // RM command
            throw new NotImplementedException();
        }

        public async Task SetTxFrequency(int frequency)
        {
            throw new NotImplementedException();
        }

        public async Task<float> MeasurePower()
        {
            // MR command, pg. 106
            await Transmit("MR 0");
            string[] results = await ReadMRReadings();
            return float.Parse(results[1]);
        }

        public async Task<float> MeasureFrequencyError()
        {
            // MR command, pg. 106
            await Transmit("MR 0");
            string[] results = await ReadMRReadings();
            return float.Parse(results[0]);
        }

        public async Task<float> MeasureFMDeviation()
        {
            // MR command, pg. 106
            await Transmit("MR 0");
            string[] results = await ReadMRReadings();
            return float.Parse(results[1]);
        }

        public async Task<string> GetInfo()
        {
            return await Send("*IDN?");
        }

        public async Task Reset()
        {
            await Send("*RST");
        }

        public async Task SetDisplay(InstrumentScreen screen)
        {
            throw new NotImplementedException();
        }

        public Task<float> MeasureP25RxBer()
        {
            throw new NotImplementedException();
        }

        public Task<float> MeasurDMRRxBer()
        {
            throw new NotImplementedException("R2670 does not support DMR.");
        }

        public Task<float> MeasureDMRRxBer()
        {
            throw new NotImplementedException();
        }

        public Task ResetBERErrors()
        {
            throw new NotImplementedException();
        }

        /**
         * PRIVATE METHODS
         */

        private async Task<string> Send(string command)
        {
            return await Connection.Send(command);
        }

        private async Task Transmit(string command)
        {
            await Connection.Transmit(command);
        }

        private async Task<string> ReadLine()
        {
            return await Connection.ReadLine();
        }

        /// <summary>
        /// Parses the four lines of data from the 2670
        /// </summary>
        /// <returns>
        /// Array of strings.
        /// Index 0: Frequency Error;
        /// Index 1: Power;
        /// Index 2: Deviation Positive;
        /// Index 3: Deviation Negative
        /// </returns>
        private async Task<string[]> ReadMRReadings()
        {
            List<string> valList = new List<string>();
            for (int i=0; i<4; i++)
            {
                string val = await ReadLine();
                valList.Add(val);
            }
            return valList.ToArray();
        }

        public async Task SetupRefOscillatorTest_P25()
        {
            //Not implemented, but shouldn't raise an exception
        }

        public async Task SetupRefOscillatorTest_FM()
        {
            //Not implemented, but shouldn't raise an exception
        }

        public async Task SetupTXPowerTest()
        {
            //Not implemented, but shouldn't raise an exception
        }

        public async Task SetupTXDeviationTest()
        {
            //Not implemented, but shouldn't raise an exception
        }

        public async Task SetupTXP25BERTest()
        {
            throw new NotImplementedException();
        }

        public async Task SetupRXTestFMMod()
        {
            //Not implemented, but shouldn't raise an exception
        }

        public async Task SetupRXTestP25BER()
        {
            //Not implemented, but shouldn't raise an exception

        }

        public async Task GenerateP25STDCal(float power)
        {
            //Not implemented, but shouldn't raise an exception
        }
    }
}

