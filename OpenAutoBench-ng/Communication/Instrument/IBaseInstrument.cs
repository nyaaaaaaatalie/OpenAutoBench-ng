namespace OpenAutoBench_ng.Communication.Instrument
{
    public enum testMode
    {
        ANALOG = 0x00,
        P25 = 0x01,
        DMR = 0x02,
    }
    
    public interface IBaseInstrument
    {
        public bool Connected { get; }

        public bool SupportsP25 { get; }

        public bool SupportsDMR { get; }
        
        /// <summary>
        /// Instrument Manufacturer
        /// </summary>
        public string Manufacturer { get; }
        /// <summary>
        /// Instrument Model
        /// </summary>
        public string Model { get; }
        /// <summary>
        /// Instrument Serial number
        /// </summary>
        public string Serial { get; }
        /// <summary>
        /// Instrument software/firmware version
        /// </summary>
        public string Version { get; }
        /// <summary>
        /// Time to wait after sending an instrument command to let the device configure itself
        /// </summary>
        public int ConfigureDelay { get; }

        public Task Connect();
        public Task Disconnect();
        public Task<bool> TestConnection();
        public Task GenerateSignal(float power);

        public Task StopGenerating();

        public Task SetGenPort(InstrumentOutputPort outputPort);

        public Task SetRxFrequency(int frequency, testMode mode);

        public Task SetTxFrequency(int frequency);

        public Task<float> MeasurePower();

        public Task<float> MeasureFrequencyError();

        public Task<float> MeasureFMDeviation();

        public Task<bool> GetInfo();

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
