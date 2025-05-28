using CSPID;
using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.OpenAutoBench;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestTX_DeviationBalance : IBaseTest
    {
        // private vars specific to test
        protected MotorolaXCMPRadioBase Radio;

        protected int[] TXFrequencies;

        protected int[] CharPoints;

        public MotorolaXCMPRadio_TestTX_DeviationBalance(XCMPRadioTestParams testParams) : base("TX: Deviation Balance", testParams.report, testParams.instrument, testParams.callback, testParams.ct)
        {
            Radio = testParams.radio;
        }

        public override bool IsRadioEligible()
        {
            return true;
        }

        public override async Task Setup()
        {
            LogCallback(String.Format("Setting up for {0}", Name));
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Task.Delay(1000, Ct);
            await Instrument.SetupTXDeviationTest();
            await Task.Delay(1000, Ct);
        }

        public override async Task PerformTest()
        {
            try
            {
                for (int i = 0; i < TXFrequencies.Length; i++)
                {   
                    int currFreq = TXFrequencies[i];
                    Radio.SetTXFrequency(currFreq, false);
                    await Instrument.SetRxFrequency(currFreq, testMode.ANALOG);
                    // low tone
                    Radio.SetTransmitConfig(XCMPRadioTransmitOption.DEVIATION_LOW);
                    Radio.Keyup();
                    await Task.Delay(10000, Ct);
                    float measDevLow = await Instrument.MeasureFMDeviation();
                    measDevLow = (float)Math.Round(measDevLow);
                    LogCallback(String.Format("TX Deviation Point at {0}MHz (low tone): {1}hz", (currFreq / 1000000F), measDevLow));
                    Radio.Dekey();
                    await Task.Delay(1000, Ct);

                    // high tone
                    Radio.SetTransmitConfig(XCMPRadioTransmitOption.DEVIATION_HIGH);
                    Radio.Keyup();
                    await Task.Delay(10000, Ct);
                    float measDevHigh = await Instrument.MeasureFMDeviation();
                    measDevHigh = (float)Math.Round(measDevHigh);
                    LogCallback(String.Format("TX Deviation Point at {0}MHz (high tone): {1}hz", (currFreq / 1000000F), measDevHigh));
                    Radio.Dekey();

                    // percentage difference
                    float percentDifference = (measDevHigh - measDevLow) / measDevLow * 100;
                    Report.AddResult(OpenAutoBench.ResultType.TX_DEVIATION_BAL, percentDifference, 0.0, -1.5, 1.5, currFreq);
                    LogCallback(String.Format("Variance between high tone and low tone at {0}MHz: {1}%", (currFreq / 1000000F), Math.Round(percentDifference, 2)));
                    await Task.Delay(1000, Ct);
                }
            }
            catch (Exception ex)
            {
                Report.AddError(OpenAutoBench.ResultType.TX_DEVIATION_BAL, ex.ToString());
                LogCallback(String.Format("Test failed: {0}", ex.ToString()));
                throw new Exception("Test failed.", ex);
            }
            finally
            {
                Radio.Dekey();
            }

        }

        /// <summary>
        /// Measure the deviation balance variance between low & high
        /// </summary>
        /// <returns>the percentage difference between low & high</returns>
        private async Task<float> measureDeviationBalance(float lowDeviation)
        {
            // Measure high tone deviation
            float measDevHigh = await Instrument.MeasureFMDeviation();
            measDevHigh = (float)Math.Round(measDevHigh);

            // percentage difference
            float percentDifference = (float)Math.Round((measDevHigh - lowDeviation) / lowDeviation * 100, 2);
            LogCallback($"Low deviation {lowDeviation} Hz, High deviation {measDevHigh} Hz, Percent difference {percentDifference}%");

            return percentDifference;
        }

        public override async Task PerformAlignment()
        {
            LogCallback("Starting Deviation Balance alignment routine");

            try
            {
                foreach (int Frequency in TXFrequencies)
                {
                    LogCallback($"Starting deviation balance alignment for frequency {Frequency / 1E6:F5} MHz");

                    // Setup Instrument
                    await Instrument.SetRxFrequency(Frequency);

                    // Wait for things to settle
                    await Task.Delay(500, Ct);

                    // Measure fixed low deviation value
                    Radio.SetTXFrequency(Frequency, false);
                    Radio.SetTransmitConfig(XCMPRadioTransmitOption.DEVIATION_LOW);
                    Radio.Keyup();
                    await Task.Delay(5000, Ct);
                    float measDevLow = await Instrument.MeasureFMDeviation();
                    measDevLow = (float)Math.Round(measDevLow);
                    LogCallback($"Low tone deviation at {Frequency} MHz: {measDevLow} Hz");
                    Radio.Dekey();
                    await Task.Delay(500, Ct);

                    // Create and setup softpot tuning loop
                    TuningLoops.SoftpotTuningLoop loop = new TuningLoops.SoftpotTuningLoop(
                        Radio,
                        MotorolaXCMPRadioBase.SoftpotType.ModBalance,
                        () => measureDeviationBalance(measDevLow),
                        0.0,                                // target is 0% difference
                        1.0,                                // 1.5% is the Moto spec so 1.0 is our limit for success
                        new Range<double>(-25.0, 25.0),     // +/- 25% seems reasonable
                        new PIDGains(-0.2, 0.0, 0.0),       // SWAG value
                        2000,                               // Wait 2 seconds for each measurement
                        30,                                 // 30 second tuning loop timeout
                        LogCallback,
                        Ct
                    );

                    loop.Setup();

                    // Setup for high deviation
                    Radio.SetTransmitConfig(XCMPRadioTransmitOption.DEVIATION_HIGH);

                    // Key radio
                    Radio.Keyup();

                    // Wait
                    await Task.Delay(3000, Ct);

                    // Perform tuning
                    bool result = await loop.Tune();

                    // Take final measurement if tuning succeeded
                    if (result)
                    {
                        await Task.Delay(3000, Ct);
                        float measBalance = await measureDeviationBalance(measDevLow);
                        Report.AddResult(ResultType.TX_DEVIATION_BAL, measBalance, 0.0, -1.5, 1.5, Frequency);
                        LogCallback(String.Format("Measured deviation balance at {0} MHz: {1}%", (Frequency / 1000000F), measBalance));
                    }
                    else
                    {
                        Report.AddError(ResultType.REF_OSC, "Alignment for Reference Oscillator Failed");
                        LogCallback("Alignment for Reference Oscillator Failed");
                    }

                    // Dekey
                    Radio.Dekey();

                    // Let the radio rest for 5 seconds
                    LogCallback("Letting radio cooldown for 5sec...");
                    await Task.Delay(5000, Ct);
                }
            }
            catch (Exception ex)
            {
                Report.AddError(ResultType.TX_DEVIATION_BAL, ex.ToString());
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
