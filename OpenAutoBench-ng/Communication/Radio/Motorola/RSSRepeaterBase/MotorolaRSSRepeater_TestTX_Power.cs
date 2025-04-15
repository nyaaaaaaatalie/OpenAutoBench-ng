using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.Quantar;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase
{
    public class MotorolaRSSRepeater_TestTX_Power : IBaseTest
    {
        public string name { get
            {
                return "TX: Power";
            } }

        public bool pass { get; private set; }

        public bool testCompleted { get; private set; }

        protected IBaseInstrument Instrument;

        protected Action<string> LogCallback;

        protected MotorolaRSSRepeaterBase Repeater;

        // private vars specific to test

        private int TXFrequency;

        protected int PA_PWR = 0;

        public MotorolaRSSRepeater_TestTX_Power(MotorolaRSSRepeaterBaseTestParams testParams)
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
            TXFrequency = await Repeater.GetTxFrequency();
            await Repeater.Send("AL STNPWR RESET");
            await Task.Delay(100);
            PA_PWR = int.Parse(await Repeater.Get("GET PA ORD_PWR"));
        }

        protected async Task<float> performTestWithReturn()
        {
            await Repeater.Send($"SET TX PWR {PA_PWR}");
            await Instrument.SetRxFrequency(TXFrequency, testMode.ANALOG);
            Repeater.Keyup();
            await Task.Delay(5000);
            float measPower = await Instrument.MeasurePower();
            Repeater.Dekey();
            measPower = (float) Math.Round(measPower, 2);
            LogCallback(String.Format("Measured power at {0}MHz: {1}w (expected {2}W)", (TXFrequency / 1000000D), measPower, PA_PWR));
            return measPower;
        }

        public async Task performTest()
        {
            await performTestWithReturn();
        }

        public async Task performAlignment()
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
                await Task.Delay(3000);
            }

            await Repeater.Send("AL STNPWR SAVE");
            await Task.Delay(6000);

            float finalMeasPower = await performTestWithReturn();
            LogCallback($"Final measured power: {Math.Round(finalMeasPower, 2)}w");
        }

        public async Task teardown()
        {
            //
        }
    }
}
