using OpenAutoBench_ng.Communication.Instrument;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestTX_PowerCharacterization :IBaseTest
    {
        public string name
        {
            get
            {
                return "TX: Power Characterization";
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

        private int SOFTPOT_TX_CHAR_POINTS = 0x11;

        public MotorolaXCMPRadio_TestTX_PowerCharacterization(XCMPRadioTestParams testParams)
        {
            LogCallback = testParams.callback;
            Radio = testParams.radio;
            Instrument = testParams.instrument;
        }

        public bool isRadioEligible()
        {
            return Radio.ModelNumber.StartsWith("M20S") ||
                   Radio.ModelNumber.StartsWith("H92U") ;
        }

        public async Task setup()
        {
            LogCallback(String.Format("Setting up for {0}", name));
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Task.Delay(1000);
            await Instrument.SetupTXPowerTest();
            await Task.Delay(1000);
            CharPoints = Radio.GetTXPowerPoints();
        }

        public async Task performTest()
        {
            try
            {
                for (int i = 0; i < TXFrequencies.Length; i++)
                {
                    Radio.SetTXFrequency(TXFrequencies[i], false);
                    await Instrument.SetRxFrequency(TXFrequencies[i], "ANALOG");

                    // low power
                    Radio.Keyup();
                    await Task.Delay(500);
                    Radio.SoftpotUpdate(0x01, CharPoints[i * 2]);
                    await Task.Delay(5000);
                    float measPow = await Instrument.MeasurePower();
                    measPow = (float)Math.Round(measPow, 2);
                    LogCallback(String.Format("TX Low Power Point at {0}MHz: {1}w", (TXFrequencies[i] / 1000000D), measPow));
                    Radio.Dekey();
                    await Task.Delay(1000);

                    // high power
                    Radio.Keyup();
                    await Task.Delay(500);
                    Radio.SoftpotUpdate(0x01, CharPoints[i * 2 + 1]);
                    await Task.Delay(5000);
                    measPow = await Instrument.MeasurePower();
                    measPow = (float)Math.Round(measPow, 2);
                    LogCallback(String.Format("TX High Power Point at {0}MHz: {1}w", (TXFrequencies[i] / 1000000D), measPow));
                    Radio.Dekey();
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
