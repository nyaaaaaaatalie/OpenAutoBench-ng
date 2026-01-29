using CSPID;
using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.OpenAutoBench;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestTX_DeviationBalance : IBaseTest
    {
        // private vars specific to test
        protected MotorolaXCMPRadioBase Radio;

        // Test Parameters
        protected MotorolaXCMPRadioBase.SoftpotParams ModBalParams;

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
            // Configure Instrument
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Task.Delay(1000, Ct);
            await Instrument.SetupTXDeviationTest();
            await Task.Delay(1000, Ct);
        }

        private async Task PerformDeviationBalanceTest(int frequency, int softpotValue)
        {
            Radio.SetTXFrequency(frequency, Bandwidth.BW_25kHz, TxDeviation.NoModulation);
            await Instrument.SetRxFrequency(frequency, testMode.ANALOG);
            // low tone
            Radio.SetTransmitConfig(XCMPRadioTransmitOption.DEVIATION_LOW);
            Radio.SetTransmitPower(TxPowerLevel.Low);
            Radio.Keyup();
            Radio.SoftpotUpdate(SoftpotType.ModBalance, MotorolaXCMPRadioBase.SoftpotValueToBytes(softpotValue, ModBalParams.ByteLength));
            await Task.Delay(6000, Ct);
            float measDevLow = await Instrument.MeasureFMDeviation();
            measDevLow = (float)Math.Round(measDevLow);
            LogCallback(String.Format("TX Deviation Point at {0}MHz (low tone): {1}hz", (frequency / 1000000F), measDevLow));
            Radio.Dekey();
            await Task.Delay(500, Ct);

            // high tone
            Radio.SetTransmitConfig(XCMPRadioTransmitOption.DEVIATION_HIGH);
            Radio.Keyup();
            Radio.SoftpotUpdate(SoftpotType.ModBalance, MotorolaXCMPRadioBase.SoftpotValueToBytes(softpotValue, ModBalParams.ByteLength));
            await Task.Delay(6000, Ct);
            float measDevHigh = await Instrument.MeasureFMDeviation();
            measDevHigh = (float)Math.Round(measDevHigh);
            LogCallback(String.Format("TX Deviation Point at {0}MHz (high tone): {1}hz", (frequency / 1000000F), measDevHigh));
            Radio.Dekey();

            // percentage difference
            float percentDifference = (measDevHigh - measDevLow) / measDevLow * 100;
            Report.AddResult(OpenAutoBench.ResultType.TX_DEVIATION_BAL, percentDifference, 0.0, -1.5, 1.5, frequency);
            LogCallback(String.Format("Variance between high tone and low tone at {0}MHz: {1}%", (frequency / 1000000F), Math.Round(percentDifference, 2)));
        }

        public override async Task PerformTest()
        {
            try
            {
                for (int i = 0; i < ModBalParams.Frequencies.Length; i++)
                {   
                    await PerformDeviationBalanceTest(ModBalParams.Frequencies[i], ModBalParams.Values[i]);
                    await Task.Delay(500, Ct);
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
                for (int i = 0; i < ModBalParams.Frequencies.Length; i++)
                {
                    LogCallback($"Starting deviation balance alignment for frequency {ModBalParams.Frequencies[i] / 1E6:F5} MHz");

                    // Setup Instrument
                    await Instrument.SetRxFrequency(ModBalParams.Frequencies[i], testMode.ANALOG);

                    // Wait for things to settle
                    await Task.Delay(500, Ct);

                    // Measure fixed low deviation value
                    Radio.SetTXFrequency(ModBalParams.Frequencies[i], Bandwidth.BW_25kHz, TxDeviation.Default);
                    Radio.SetTransmitConfig(XCMPRadioTransmitOption.DEVIATION_LOW);
                    Radio.SetTransmitPower(TxPowerLevel.Low);
                    Radio.Keyup(TxMicrophone.InternalMuted);
                    Radio.SoftpotUpdate(SoftpotType.ModBalance, MotorolaXCMPRadioBase.SoftpotValueToBytes(ModBalParams.Values[i], ModBalParams.ByteLength));
                    await Task.Delay(6000, Ct);
                    float measDevLow = await Instrument.MeasureFMDeviation();
                    measDevLow = (float)Math.Round(measDevLow);
                    LogCallback($"Low tone deviation at {ModBalParams.Frequencies[i]} MHz: {measDevLow} Hz");
                    Radio.Dekey();
                    await Task.Delay(500, Ct);

                    // Create and setup softpot tuning loop
                    TuningLoops.SoftpotTuningLoop loop = new TuningLoops.SoftpotTuningLoop(
                        Radio,
                        SoftpotType.ModBalance,
                        new Range<int>(ModBalParams.Min, ModBalParams.Max),
                        ModBalParams.ByteLength,
                        () => measureDeviationBalance(measDevLow),
                        0.0,                                // target is 0% difference
                        0.75,                               // 1.5% is the Moto spec so 1.0 is our limit for success
                        10.0,                               // Softpot variance target
                        new Range<double>(-25.0, 25.0),     // +/- 25% seems reasonable
                        new PIDGains(-0.2, 0.0, 0.0),       // SWAG value
                        3000,                               // Wait 2 seconds for each measurement
                        60,                                 // 30 second tuning loop timeout
                        LogCallback,
                        Ct
                    );
                    loop.Setup();

                    // Setup for high deviation
                    Radio.SetTransmitConfig(XCMPRadioTransmitOption.DEVIATION_HIGH);

                    // Key radio
                    Radio.Keyup();

                    // Update initial softpot value
                    Radio.SoftpotUpdate(SoftpotType.ModBalance, MotorolaXCMPRadioBase.SoftpotValueToBytes(ModBalParams.Values[i], ModBalParams.ByteLength));

                    // Wait
                    await Task.Delay(2500, Ct);

                    // Perform tuning
                    bool result = await loop.Tune();

                    // Dekey
                    Radio.Dekey();

                    // Take final measurement if tuning succeeded
                    if (result)
                    {
                        // Wait
                        await Task.Delay(1000, Ct);
                        // Take Measurement
                        LogCallback($"Taking final deviation balance measurement for frequency {ModBalParams.Frequencies[i] / 1e6F:0.00000} MHz");
                        await PerformDeviationBalanceTest(ModBalParams.Frequencies[i], (int)loop.FinalSoftpotValue);
                    }
                    else
                    {
                        Report.AddError(ResultType.TX_DEVIATION_BAL, $"Alignment for Reference Oscillator Failed ({ModBalParams.Frequencies[i] / 1e6F:0.00000} MHz)");
                        LogCallback($"Alignment for Reference Oscillator Failed ({ModBalParams.Frequencies[i] / 1e6F:0.00000} MHz)");
                    }

                    

                    // Let the radio rest for 5 seconds
                    LogCallback("Letting radio cooldown...");
                    await Task.Delay(2500, Ct);
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
