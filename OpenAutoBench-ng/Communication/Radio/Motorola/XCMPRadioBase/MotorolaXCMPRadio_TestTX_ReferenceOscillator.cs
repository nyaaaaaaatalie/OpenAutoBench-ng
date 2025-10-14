using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;
using OpenAutoBench_ng.OpenAutoBench;
using CSPID;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestTX_ReferenceOscillator : IBaseTest
    {
        // Test-specific variables
        protected int TXFrequency;
        protected MotorolaXCMPRadioBase radio;
        protected double freqErrMax = 50.0;

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
                await Instrument.SetRxFrequency(TXFrequency, testMode.ANALOG);
                radio.Keyup();
                await Task.Delay(5000, Ct);

                float measErr = await Instrument.MeasureFrequencyError();
                measErr = (float)Math.Round(measErr, 2);
                Report.AddResult(ResultType.REF_OSC, measErr, 0.0, -freqErrMax, freqErrMax, TXFrequency);
                LogCallback(String.Format("Measured frequency error at {0}MHz: {1}hz", (TXFrequency / 1000000F), measErr));
            }
            catch (Exception ex)
            {
                Report.AddError(ResultType.REF_OSC, ex.ToString());
                LogCallback(String.Format("Test error: {0}", ex.ToString()));
                throw;
            }
            finally
            {
                radio.Dekey();
            }
        }

        public override async Task PerformAlignment()
        {
            LogCallback("Starting Reference Oscillator alignment routine");

            try
            {
                // Create and setup softpot tuning loop
                TuningLoops.SoftpotTuningLoop loop = new TuningLoops.SoftpotTuningLoop(
                    radio,
                    MotorolaXCMPRadioBase.SoftpotType.RefOsc,
                    Instrument.MeasureFrequencyError,
                    0.0,                                // target is 0 Hz frequency error
                    10.0,                               // most instruments should be able to maintain accuracy to within +/- 10 Hz
                    new Range<double>(-10000, 10000),   // +/- 10kHz seems reasonable
                    new PIDGains(-1.0, 0.0, 0.0),       // note negative feedback required
                    1000,                                // Wait 1s for each measurement
                    30,                                 // 30 second test timeout
                    LogCallback,
                    Ct
                );

                loop.Setup();

                // Setup Instrument
                await Instrument.SetRxFrequency(TXFrequency, testMode.ANALOG);

                // Setup radio
                radio.SetTXFrequency(TXFrequency, false);

                // Key radio
                radio.Keyup();

                // Wait a few seconds for things to settle
                await Task.Delay(3000, Ct);

                // Perform tuning
                bool result = await loop.Tune();

                // Take final measurement if tuning succeeded
                if (result)
                {
                    await Task.Delay(1000, Ct);
                    float measErr = await Instrument.MeasureFrequencyError();
                    measErr = (float)Math.Round(measErr, 2);
                    Report.AddResult(ResultType.REF_OSC, measErr, 0.0, -freqErrMax, freqErrMax, TXFrequency);
                    LogCallback(String.Format("Measured frequency error at {0}MHz: {1}hz", (TXFrequency / 1000000F), measErr));
                }
                else
                {
                    Report.AddError(ResultType.REF_OSC, "Alignment for Reference Oscillator Failed");
                    LogCallback("Alignment for Reference Oscillator Failed");
                }
            }
            catch (Exception ex)
            {
                Report.AddError(ResultType.REF_OSC, ex.ToString());
                LogCallback(String.Format("Got error during alignment routine: {0}", ex.ToString()));
                throw;
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
