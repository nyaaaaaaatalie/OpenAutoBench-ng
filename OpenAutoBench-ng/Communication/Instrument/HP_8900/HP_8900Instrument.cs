using OpenAutoBench_ng.Communication.Instrument.Connection;

namespace OpenAutoBench_ng.Communication.Instrument.HP_8900
{
    public class HP_8900Instrument : IBaseInstrument
    {
        private IInstrumentConnection Connection;

        public bool Connected { get; private set; }

        public bool SupportsP25 { get { return false; } }

        public bool SupportsDMR { get { return false; } }

        public string Manufacturer { get; private set; }
        public string Model { get; private set; }
        public string Serial { get; private set; }
        public string Version { get; private set; }

        public int ConfigureDelay { get { return 250; } }

        private int GPIBAddr;
        public HP_8900Instrument(IInstrumentConnection conn, int addr)
        {
            Connected = false;
            Connection = conn;
            GPIBAddr = addr;
        }

        private async Task<string> Send(string command)
        {
            return await Connection.Send(command);
        }

        private async Task Transmit(string command)
        {
            await Connection.Transmit(command);
        }

        public async Task Connect()
        {
            Connection.Connect();
            await Transmit("++mode 1");
            await Transmit("++addr " + GPIBAddr.ToString());
            await Transmit("++auto 2");
            await Transmit("++llo");
        }

        public async Task Disconnect()
        {
            await Transmit("++loc");
            await Transmit("++loc");
            Connection.Disconnect();
        }

        public async Task<bool> TestConnection()
        {
            // Connect if not already connected
            if (!Connected)
            {
                await Connect();
            }
            // Test result
            bool success = true;
            // Get info
            await GetInfo();
            // Validate manufacturer
            if (!Manufacturer.Contains("Hewlett-Packard"))
            {
                Console.WriteLine("Connected instrument is not an HP device!");
                success = false;
            }
            // Validate model
            string[] models = new string[] { "8920", "8921", "8935" };
            if (!models.Any(Model.Contains))
            {
                Console.WriteLine("Connected instrument is not an 8920, 8921, or 8935!");
                success = false;
            }
            // Close
            await Disconnect();
            // Return result
            return success;
        }

        public async Task GenerateSignal(float power)
        {
            await Transmit($"RFG:AMPL {power.ToString()} DBM");
            await Transmit("RFG:AMPL:STAT 1");
        }

        public async Task GenerateFMSignal(float power, float afFreq)
        {
            await GenerateSignal(power);
            throw new NotImplementedException();
        }

        public async Task StopGenerating()
        {
            await Transmit("RFG:AMPL:STAT 0");
        }

        public async Task SetGenPort(InstrumentOutputPort outputPort)
        {
            switch (outputPort)
            {
                case InstrumentOutputPort.RF_IN_OUT:
                    await Transmit("RFG:OUTP 'RF Out'");
                    break;

                case InstrumentOutputPort.DUPLEX_OUT:
                    await Transmit("RFG:OUTP DUPL");
                    break;
            }
        }

        public async Task SetRxFrequency(int frequency)
        {
            await Transmit(string.Format("RFAN:FREQ {0}", frequency.ToString()));
        }

        public async Task SetTxFrequency(int frequency)
        {
            // Does not return anything
            await Transmit("RFG:FREQ " + frequency.ToString());
        }

        public async Task<float> MeasurePower()
        {
            return float.Parse(await Send("MEAS:RFR:POW?"));
        }

        public async Task<float> MeasureFrequencyError()
        {
            return float.Parse(await Send("MEAS:RFR:FREQ:ERR?"));
        }

        public async Task<float> MeasureFMDeviation()
        {
            return float.Parse(await Send("MEAS:AFR:FM?"));
        }

        public async Task<bool> GetInfo()
        {
            // Get response from IDN which should be <company name>, <model number>, <serial number>, <firmware revision>
            string idenResp = await Send("*IDN?");
            try
            {
                string[] idenParams = idenResp.Split(',');
                Manufacturer = idenParams[0];
                Model = idenParams[1];
                Serial = idenParams[2];
                Version = idenParams[3];
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("IDN response invalid!");
                return false;
            }
            return true;
        }

        public async Task Reset()
        {
            await Transmit("*RST");
        }

        public async Task SetDisplay(InstrumentScreen screen)
        {
            switch (screen)
            {
                // These don't return anything, so we Transmit instead of Send
                case InstrumentScreen.Monitor:
                    await Transmit("DISP RFAN");
                    break;
                case InstrumentScreen.Generate:
                    await Transmit("DISP RFG");
                    break;
                default:
                    throw new Exception("Unknown screen requested");
            }
        }

        public Task<float> MeasureP25RxBer()
        {
            throw new NotImplementedException("HP 8900 does not support digital tests.");
        }

        public Task<float> MeasurDMR5RxBer()
        {
            throw new NotImplementedException("HP 8900 does not support digital tests.");
        }

        public Task<float> MeasureDMRRxBer()
        {
            throw new NotImplementedException();
        }

        public Task ResetBERErrors()
        {
            throw new NotImplementedException();
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
            //Partially implemented, the filters are being set-up, but other settings may need to be added.
            await Transmit("AFAN:FILT1 '<20Hz HPF'");
            await Transmit("AFAN:FILT2 '15kHz LPF'");
            // Set up FM deviation averaging
            await Transmit("MEAS:AFR:FM:AVER:STAT ON");
            await Transmit("MEAS:AFR:FM:AVER:VAL 5");
        }

        public async Task SetupTXP25BERTest()
        {
            throw new NotImplementedException("HP 8900 does not support digital tests.");
        }

        public async Task SetupRXTestFMMod()
        {
            // Ensure we're using the RF Out port
            await SetGenPort(InstrumentOutputPort.RF_IN_OUT);
        }

        public async Task SetupRXTestP25BER()
        {
            throw new NotImplementedException("HP 8900 does not support digital tests.");

        }

        public async Task GenerateP25STDCal(float power)
        {
            throw new NotImplementedException("HP 8900 does not support digital tests.");
        }
    }
}
