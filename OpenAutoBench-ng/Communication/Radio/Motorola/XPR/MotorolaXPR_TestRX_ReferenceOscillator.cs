using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XPR
{
    public class MotorolaXPR_TestRX_ReferenceOscillator
    {
        public string name
        {
            get
            {
                return "RX: Reference Oscillator";
            }
        }

        public bool pass { get; private set; }

        public bool testCompleted { get; private set; }

        protected IBaseInstrument Instrument;

        protected Action<string> LogCallback;

        protected MotorolaXCMPRadioBase Radio;

        // private vars specific to test

        protected int RXFrequency;

        public MotorolaXPR_TestRX_ReferenceOscillator(XCMPRadioTestParams testParams)
        {
            LogCallback = testParams.callback;
            Radio = testParams.radio;
            Instrument = testParams.instrument;
        }

        public bool isRadioEligible()
        {
            return true;
        }

        public async Task setup()
        {
            LogCallback(String.Format("Setting up for {0}", name));
            await Instrument.SetDisplay(InstrumentScreen.Generate);
            await Task.Delay(1000);

            RXFrequency = MotorolaXPR_Frequencies.RxRefOscFrequencies(Radio);
        }

        public async Task performTest()
        {
            try
            {
                Radio.SetRXFrequency(RXFrequency, Bandwidth.BW_25kHz, RxModulation.C4FM);
                await Instrument.SetTxFrequency(RXFrequency);
                Radio.Keyup();
                await Task.Delay(5000);
                float measErr = await Instrument.MeasureFrequencyError();
                measErr = (float)Math.Round(measErr, 2);
                LogCallback(String.Format("Measured frequency error at {0}MHz: {1}hz", (RXFrequency / 1000000D), measErr));
            }
            catch (Exception ex)
            {
                LogCallback(String.Format("Test failed: {0}", ex.ToString()));
                throw new Exception("Test failed.", ex);
            }
            finally
            {
                Radio.Dekey();
            }

        }

        public async Task performAlignment()
        {
            throw new NotImplementedException();
        }

        public async Task teardown()
        {
            //
        }
    }
}
