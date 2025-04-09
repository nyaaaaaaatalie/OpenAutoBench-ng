using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestTX_P25_BER : IBaseTest
    {
        public string name
        {
            get
            {
                return "TX: P25 Bit Error Rate (BER)";
            }
        }

        public bool pass { get; private set; }

        public bool testCompleted { get; private set; }

        protected IBaseInstrument Instrument;

        protected Action<string> LogCallback;

        protected MotorolaXCMPRadioBase Radio;

        // private vars specific to test

        protected int[] TXFrequencies;

        public MotorolaXCMPRadio_TestTX_P25_BER(XCMPRadioTestParams testParams)
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
            await Instrument.SetupTXP25BERTest();
            await Task.Delay(1000);

            // let child set frequency
        }

        public async Task performTest()
        {
            if (Instrument.SupportsP25)
            {
                try
                {
                    Radio.SetTransmitConfig(XCMPRadioTransmitOption.STD_1011);
                    await Task.Delay(500);
                    foreach (int TXFrequency in TXFrequencies)
                    {
                        Radio.SetTXFrequency(TXFrequency, true);
                        await Instrument.SetRxFrequency(TXFrequency, testMode.P25);
                        Radio.Keyup();
                        await Task.Delay(1500);
                        await Instrument.ResetBERErrors();
                        await Task.Delay(5000);
                        float measErr = await Instrument.MeasureP25RxBer();
                        Radio.Dekey();
                        measErr = (float)Math.Round(measErr, 4);
                        LogCallback(String.Format("Measured BER at {0}MHz: {1}%", (TXFrequency / 1000000F), measErr));
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
