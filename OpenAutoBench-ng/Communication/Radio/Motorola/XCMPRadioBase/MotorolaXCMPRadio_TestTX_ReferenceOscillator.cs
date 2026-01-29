using CSPID;
using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.APX;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;
using OpenAutoBench_ng.OpenAutoBench;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestTX_ReferenceOscillator : IBaseTest
    {
        // Test-specific variables
        protected MotorolaXCMPRadioBase Radio;
        protected double freqErrMax = 50.0;

        // Test Parameters
        protected MotorolaXCMPRadioBase.SoftpotParams RefOscParams;

        public MotorolaXCMPRadio_TestTX_ReferenceOscillator(XCMPRadioTestParams testParams) : base("TX: Reference Oscillator", testParams.report, testParams.instrument, testParams.callback, testParams.ct)
        {
            Radio = testParams.radio;
        }

        public override bool IsRadioEligible()
        {
            return true;
        }

        public override async Task Setup()
        {
            // Setup measurement
            LogCallback(String.Format("Setting up for {0}", Name));

            // Configure Instrument
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Task.Delay(Instrument.ConfigureDelay, Ct);
            await Instrument.SetupRefOscillatorTest_P25();
            await Task.Delay(Instrument.ConfigureDelay, Ct);
        }

        public override async Task PerformTest()
        {
            try
            {
                Radio.SetTXFrequency(RefOscParams.Frequencies[0], Bandwidth.BW_25kHz, TxDeviation.NoModulation);
                await Instrument.SetRxFrequency(RefOscParams.Frequencies[0], testMode.ANALOG);
                Radio.Keyup();
                await Task.Delay(5000, Ct);

                float measErr = await Instrument.MeasureFrequencyError();
                measErr = (float)Math.Round(measErr, 2);
                Report.AddResult(ResultType.REF_OSC, measErr, 0.0, -freqErrMax, freqErrMax, RefOscParams.Frequencies[0]);
                LogCallback($"Measured frequency error at {RefOscParams.Frequencies[0] / 1e6F:0.000000} MHz: {measErr} hz");
            }
            catch (Exception ex)
            {
                Report.AddError(ResultType.REF_OSC, ex.ToString());
                LogCallback(String.Format("Test error: {0}", ex.ToString()));
                throw;
            }
            finally
            {
                Radio.Dekey();
            }
        }

        public override async Task PerformAlignment()
        {
            LogCallback("Starting Reference Oscillator alignment routine");

            try
            {
                double kp;
                if (MotorolaAPX_RefData.isFreon(Radio))
                {
                    kp = -1.0;
                }
                else
                {
                    kp = 1.0;
                }

                // Create and setup softpot tuning loop
                TuningLoops.SoftpotTuningLoop loop = new TuningLoops.SoftpotTuningLoop(
                    Radio,
                    SoftpotType.RefOsc,
                    new Range<int>(RefOscParams.Min, RefOscParams.Max),
                    RefOscParams.ByteLength,
                    Instrument.MeasureFrequencyError,
                    0.0,                                // target is 0 Hz frequency error
                    10.0,                               // most instruments should be able to maintain accuracy to within +/- 10 Hz,
                    5.0,                                // Softpot Variance, TODO: Tweak This
                    new Range<double>(-10000, 10000),   // +/- 10kHz seems reasonable
                    new PIDGains(kp, 0.0, 0.0),       // note negative feedback required
                    3000,                                // Wait 1s for each measurement
                    60,                                 // 30 second test timeout
                    LogCallback,
                    Ct
                );

                loop.Setup();

                // Setup Instrument
                await Instrument.SetRxFrequency(RefOscParams.Frequencies[0], testMode.ANALOG);

                // Setup radio
                Radio.SetTXFrequency(RefOscParams.Frequencies[0], Bandwidth.BW_25kHz, TxDeviation.NoModulation);
                Radio.SetTransmitPower(TxPowerLevel.Low);

                // Key radio
                Radio.Keyup();

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
                    Report.AddResult(ResultType.REF_OSC, measErr, 0.0, -freqErrMax, freqErrMax, RefOscParams.Frequencies[0]);
                    LogCallback($"Measured frequency error at {RefOscParams.Frequencies[0] / 1000000F:0.000000} MHz: {measErr} hz");
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
                Radio.Dekey();
            }
        }

        public override async Task Teardown()
        {
            //
        }
    }
}
