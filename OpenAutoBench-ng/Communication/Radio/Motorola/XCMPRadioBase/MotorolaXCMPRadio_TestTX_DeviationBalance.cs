using OpenAutoBench_ng.Communication.Instrument;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestTX_DeviationBalance : IBaseTest
    {
        public string name
        {
            get
            {
                return "TX: Deviation Balance";
            }
        }

        public bool pass { get; private set; }

        public bool testCompleted { get; private set; }

        protected IBaseInstrument Instrument;

        protected Action<string> LogCallback;

        protected MotorolaXCMPRadioBase Radio;

        // private vars specific to test

        protected int[] TXFrequencies;

        protected int[] CharPoints;

        public MotorolaXCMPRadio_TestTX_DeviationBalance(XCMPRadioTestParams testParams)
        {
            LogCallback = testParams.callback;
            Radio = testParams.radio;
            Instrument = testParams.instrument;
        }

        public bool isRadioEligible()
        {
            return true;    // ?????
        }

        public async Task setup()
        {
            LogCallback(String.Format("Setting up for {0}", name));
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Task.Delay(1000);
            await Instrument.SetupTXDeviationTest();
            await Task.Delay(1000);
        }

        public async Task performTest()
        {
            try
            {
                for (int i = 0; i < TXFrequencies.Length; i++)
                {   
                    int currFreq = TXFrequencies[i];
                    Radio.SetTXFrequency(currFreq, false);
                    await Instrument.SetRxFrequency(currFreq, "ANALOG");
                    // low tone
                    Radio.SetTransmitConfig(XCMPRadioTransmitOption.DEVIATION_LOW);
                    Radio.Keyup();
                    await Task.Delay(10000);
                    float measDevLow = await Instrument.MeasureFMDeviation();
                    measDevLow = (float)Math.Round(measDevLow);
                    LogCallback(String.Format("TX Deviation Point at {0}MHz (low tone): {1}hz", (currFreq / 1000000F), measDevLow));
                    Radio.Dekey();
                    await Task.Delay(1000);

                    // high tone
                    Radio.SetTransmitConfig(XCMPRadioTransmitOption.DEVIATION_HIGH);
                    Radio.Keyup();
                    await Task.Delay(10000);
                    float measDevHigh = await Instrument.MeasureFMDeviation();
                    measDevHigh = (float)Math.Round(measDevHigh);
                    LogCallback(String.Format("TX Deviation Point at {0}MHz (high tone): {1}hz", (currFreq / 1000000F), measDevHigh));
                    Radio.Dekey();

                    // percentage difference
                    float percentDifference = (measDevHigh - measDevLow) / measDevLow * 100;
                    LogCallback(String.Format("Variance between high tone and low tone at {0}MHz: {1}%", (currFreq / 1000000F), Math.Round(percentDifference, 2)));
                    await Task.Delay(1000);
                }
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
