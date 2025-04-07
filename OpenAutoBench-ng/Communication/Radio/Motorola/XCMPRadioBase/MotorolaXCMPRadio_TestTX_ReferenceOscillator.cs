using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;
using OpenAutoBench_ng.OpenAutoBench;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestTX_ReferenceOscillator : IBaseTest
    {
        // Test-specific variables
        protected int TXFrequency;
        protected MotorolaXCMPRadioBase radio;

        public MotorolaXCMPRadio_TestTX_ReferenceOscillator(XCMPRadioTestParams testParams) : base("TX: Reference Oscillator", testParams.report, testParams.instrument, testParams.callback, testParams.ct)
        {
            radio = testParams.radio;
        }

        public override bool IsRadioEligible()
        {
            return true;
        }

        public override async Task Setup()
        {
            // Setup measurement
            LogCallback(String.Format("Setting up for {0}", Name));
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Task.Delay(Instrument.ConfigureDelay, Ct);
            await Instrument.SetupRefOscillatorTest_P25();
            await Task.Delay(Instrument.ConfigureDelay, Ct);
        }

        public override async Task PerformTest()
        {
            try
            {
                radio.SetTXFrequency(TXFrequency, false);
                await Instrument.SetRxFrequency(TXFrequency);
                radio.Keyup();
                await Task.Delay(5000, Ct);
                float measErr = await Instrument.MeasureFrequencyError();
                measErr = (float)Math.Round(measErr, 2);
                Report.AddResult(ResultType.REF_OSC, measErr, 0.0f, -50.0f, 50.0f, TXFrequency);
                LogCallback(String.Format("Measured frequency error at {0}MHz: {1}hz", (TXFrequency / 1000000F), measErr));
            }
            catch (Exception ex)
            {
                Report.AddError(ResultType.REF_OSC, ex.ToString());
                LogCallback(String.Format("Test error: {0}", ex.ToString()));
                throw new Exception("Test error:", ex);
            }
            finally
            {
                radio.Dekey();
            }
        }

        public override async Task Teardown()
        {
            //
        }
    }
}
