using OpenAutoBench_ng.Communication.Instrument;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase
{
    public class MotorolaRSSRepeater_TestTX_Deviation : IBaseTest
    {
        // private vars specific to test
        protected MotorolaRSSRepeaterBase Repeater;

        public MotorolaRSSRepeater_TestTX_Deviation(MotorolaRSSRepeaterBaseTestParams testParams) : base("TX: Deviation", testParams.report, testParams.instrument, testParams.callback, testParams.ct)
        {
            Repeater = testParams.radio;
        }

        public override bool IsRadioEligible()
        {
            return true;
        }

        public override async Task Setup()
        {
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Task.Delay(1000, Ct);
            await Instrument.SetupTXDeviationTest();
            await Task.Delay(1000, Ct);
        }

        public override async Task PerformTest()
        {

            for (int i = 1; i < 5; i++)
            {
                int TXFrequency = 0;
                string result = await Repeater.Send($"AL TXDEV GO F{i}");
                TXFrequency = Convert.ToInt32(result.Split(" = ")[1]);
                await Instrument.SetRxFrequency(TXFrequency, testMode.ANALOG);
                
                //Repeater.Keyup();     // sending GO will key the repeater up
                await Task.Delay(5000, Ct);
                float measDev = await Instrument.MeasureFMDeviation();
                Repeater.Dekey();
                measDev = (float)Math.Round(measDev, 2);
                // TODO: figure out what the actual target deviation is for this test
                Report.AddResult(OpenAutoBench.ResultType.TX_DEVIATION, measDev, 2830, 2500, 3000, TXFrequency);
                LogCallback(String.Format("Measured deviation at {0}MHz: {1}hz", (TXFrequency / 1000000F), measDev));
                await Task.Delay(1000, Ct); 
            }
        }

        public override async Task Teardown()
        {
            //
        }
    }
}
