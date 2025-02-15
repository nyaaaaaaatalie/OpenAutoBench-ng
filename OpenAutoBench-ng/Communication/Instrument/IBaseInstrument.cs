namespace OpenAutoBench_ng.Communication.Instrument
{
    public interface IBaseInstrument
    {
        public bool Connected { get; }

        public bool SupportsP25 { get; }

        public bool SupportsDMR { get; }
        public Task Connect();
        public Task Disconnect();
        public Task GenerateSignal(float power);
        public Task GenerateFMSignal(float power, float afFreq);

        public Task StopGenerating();

        public Task SetGenPort(InstrumentOutputPort outputPort);

        public Task SetRxFrequency(int frequency);

        public Task SetTxFrequency(int frequency);

        public Task<float> MeasurePower();

        public Task<float> MeasureFrequencyError();

        public Task<float> MeasureFMDeviation();

        public Task<string> GetInfo();

        public Task Reset();

        public Task SetDisplay(InstrumentScreen screen);

        public Task<float> MeasureP25RxBer();

        public Task<float> MeasureDMRRxBer();

        public Task ResetBERErrors();

        public Task SetupRefOscillatorTest_P25();

        public Task SetupRefOscillatorTest_FM();

        public Task SetupTXPowerTest();

        public Task SetupTXDeviationTest();

        public Task SetupTXP25BERTest();

        public Task SetupRXTestFMMod();

        public Task SetupRXTestP25BER();

        public Task GenerateP25STDCal(float power);
        
    }
}
