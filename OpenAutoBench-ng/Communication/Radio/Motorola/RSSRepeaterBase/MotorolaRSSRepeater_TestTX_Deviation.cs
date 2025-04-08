using OpenAutoBench_ng.Communication.Instrument;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase
{
    public class MotorolaRSSRepeater_TestTX_Deviation : IBaseTest
    {
        public string name
        {
            get
            {
                return "TX: Deviation";
            }
        }

        public bool pass { get; private set; }

        public bool testCompleted { get; private set; }

        private IBaseInstrument Instrument;

        private Action<string> LogCallback;

        protected MotorolaRSSRepeaterBase Repeater;

        // private vars specific to test

        public MotorolaRSSRepeater_TestTX_Deviation(MotorolaRSSRepeaterBaseTestParams testParams)
        {
            LogCallback = testParams.callback;
            Repeater = testParams.radio;
            Instrument = testParams.instrument;
        }

        public bool isRadioEligible()
        {
            return true;
        }

        public async Task setup()
        {
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Task.Delay(1000);
            await Instrument.SetupTXDeviationTest();
            await Task.Delay(1000);
        }

        public async Task performTest()
        {

            for (int i = 1; i < 5; i++)
            {
                int TXFrequency = 0;
                string result = await Repeater.Send($"AL TXDEV GO F{i}");
                TXFrequency = Convert.ToInt32(result.Split(" = ")[1]);
                await Instrument.SetRxFrequency(TXFrequency, "FM");
                
                //Repeater.Keyup();     // sending GO will key the repeater up
                await Task.Delay(5000);
                float measDev = await Instrument.MeasureFMDeviation();
                Repeater.Dekey();
                measDev = (float)Math.Round(measDev, 2);
                LogCallback(String.Format("Measured deviation at {0}MHz: {1}hz", (TXFrequency / 1000000F), measDev));
                await Task.Delay(1000); 
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
