using OpenAutoBench_ng.Communication.Instrument;

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
                    await Instrument.SetRxFrequency(currFreq);
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
                    Report.AddResult(OpenAutoBench.ResultType.TX_DEVIATION_BAL, percentDifference, 0.0f, 0.0f, 1.5f, currFreq);
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

        public override async Task Teardown()
        {
            //
        }
    }
}
