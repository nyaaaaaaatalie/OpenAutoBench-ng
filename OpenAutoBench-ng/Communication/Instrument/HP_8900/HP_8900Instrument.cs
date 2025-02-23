using OpenAutoBench_ng.Communication.Instrument.Connection;

namespace OpenAutoBench_ng.Communication.Instrument.HP_8900
{
    public class HP_8900Instrument : IBaseInstrument
    {
        private IInstrumentConnection Connection;

        public bool Connected { get; private set; }

        public bool SupportsP25 { get { return false; } }

        public bool SupportsDMR { get { return false; } }

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

        public async Task GenerateSignal(float power)
        {
            await Send("RFG:AMPL " + power.ToString());
        }

        public async Task GenerateFMSignal(float power, float afFreq)
        {
            await GenerateSignal(power);
            throw new NotImplementedException();
        }

        public async Task StopGenerating()
        {
            await Send("RFG:AMPL:STAT 0");
        }

        public async Task SetGenPort(InstrumentOutputPort outputPort)
        {
            switch (outputPort)
            {
                case InstrumentOutputPort.RF_IN_OUT:
                    await Send("RFG:OUTP 'RF Out'");
                    break;

                case InstrumentOutputPort.DUPLEX_OUT:
                    await Send("RFG:OUTP DUPL");
                    break;
            }
        }

        public async Task SetRxFrequency(int frequency)
        {
            await Transmit(string.Format("RFAN:FREQ {0}", frequency.ToString()));
        }

        public async Task SetTxFrequency(int frequency)
        {
            await Send("RFG:FREQ " + frequency.ToString());
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
            switch (screen)
            {
                case InstrumentScreen.Monitor:
                    await Send("DISP RFAN");
                    break;
                case InstrumentScreen.Generate:
                    await Send("DISP RFG");
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
        }

        public async Task SetupTXP25BERTest()
        {
            throw new NotImplementedException("HP 8900 does not support digital tests.");
        }

        public async Task SetupRXTestFMMod()
        {
            //Not implemented, but shouldn't raise an exception
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
