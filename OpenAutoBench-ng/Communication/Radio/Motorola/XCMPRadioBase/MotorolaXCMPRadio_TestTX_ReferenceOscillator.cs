using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestTX_ReferenceOscillator : IBaseTest
    {
        public string name
        {
            get
            {
                return "TX: Reference Oscillator";
            }
        }

        public bool pass { get; private set; }

        public bool testCompleted { get; private set; }

        protected IBaseInstrument Instrument;

        protected Action<string> LogCallback;

        protected MotorolaXCMPRadioBase Radio;

        // private vars specific to test

        protected int TXFrequency;

        public MotorolaXCMPRadio_TestTX_ReferenceOscillator(XCMPRadioTestParams testParams)
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
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Task.Delay(1000);
            await Instrument.SetupRefOscillatorTest_FM();
            await Task.Delay(1000);

            // let child set frequency
        }

        public async Task performTest()
        {
            try
            {
                Radio.SetTXFrequency(TXFrequency, false);
                await Instrument.SetRxFrequency(TXFrequency, testMode.ANALOG);
                Radio.Keyup();
                await Task.Delay(5000);
                float measErr = await Instrument.MeasureFrequencyError();
                measErr = (float)Math.Round(measErr, 2);
                LogCallback(String.Format("Measured frequency error at {0}MHz: {1}hz", (TXFrequency / 1000000F), measErr));
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
