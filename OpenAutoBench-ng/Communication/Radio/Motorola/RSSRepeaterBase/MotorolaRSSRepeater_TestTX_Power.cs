using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.Quantar;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase
{
    public class MotorolaRSSRepeater_TestTX_Power : IBaseTest
    {
        // private vars specific to test
        protected MotorolaRSSRepeaterBase Repeater;
        private int TXFrequency;
        protected int PA_PWR = 0;

        public MotorolaRSSRepeater_TestTX_Power(MotorolaRSSRepeaterBaseTestParams testParams) : base("TX: Power", testParams.report, testParams.instrument, testParams.callback, testParams.ct)
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
            TXFrequency = await Repeater.GetTxFrequency();
            await Repeater.Send("AL STNPWR RESET");
            await Task.Delay(100, Ct);
            PA_PWR = int.Parse(await Repeater.Get("GET PA ORD_PWR"));
        }

        protected async Task<float> performTestWithReturn(bool record = false)
        {
            await Repeater.Send($"SET TX PWR {PA_PWR}");
            await Instrument.SetRxFrequency(TXFrequency);
            Repeater.Keyup();
            await Task.Delay(5000, Ct);
            float measPower = await Instrument.MeasurePower();
            Repeater.Dekey();
            measPower = (float) Math.Round(measPower, 2);
            if (record)
            {
                Report.AddResult(OpenAutoBench.ResultType.TX_POWER, measPower, PA_PWR, PA_PWR - 5, PA_PWR + 5, TXFrequency);
            }
            LogCallback(String.Format("Measured power at {0}MHz: {1}w (expected {2}W)", (TXFrequency / 1000000D), measPower, PA_PWR));
            return measPower;
        }

        public override async Task PerformTest()
        {
            await performTestWithReturn(true);
        }

        public override async Task PerformAlignment()
        {
            // run 5 times
            for (int i = 0; i < 5; i++)
            {
                float measPower = await performTestWithReturn();
                LogCallback($"Round {i}: {Math.Round(measPower, 2)}");
                measPower = (float)Math.Round(measPower * 100);
                double radioPower = Math.Round((double)(PA_PWR * 100));
                LogCallback("Writing new power level to radio");
                await Repeater.Send($"AL STNPWR WR {radioPower} {measPower}");
                await Task.Delay(3000, Ct);
            }

            await Repeater.Send("AL STNPWR SAVE");
            await Task.Delay(6000, Ct);

            float finalMeasPower = await performTestWithReturn();
            Report.AddResult(OpenAutoBench.ResultType.TX_POWER, finalMeasPower, PA_PWR, PA_PWR + 5, PA_PWR - 5, TXFrequency);
            LogCallback($"Final measured power: {Math.Round(finalMeasPower, 2)}w");
        }

        public override async Task Teardown()
        {
            //
        }
    }
}
