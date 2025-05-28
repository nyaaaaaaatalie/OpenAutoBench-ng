using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.Quantar
{
    public class MotorolaQuantar_TestRX_RSSI : IBaseTest
    {
        // private vars specific to test
        protected MotorolaQuantar Repeater;
        private int RXFrequency;
        private int GenLevel = -90;

        public MotorolaQuantar_TestRX_RSSI(MotorolaRSSRepeaterBaseTestParams testParams) : base("RX: RSSI", testParams.report, testParams.instrument, testParams.callback, testParams.ct)
        {
            Repeater = (MotorolaQuantar)testParams.radio;
        }

        public override bool IsRadioEligible()
        {
            return true;
        }

        public override async Task Setup()
        {
            await Instrument.SetDisplay(InstrumentScreen.Generate);
            await Repeater.Transmit("SET FREQ TX 0");
            RXFrequency = await Repeater.GetRxFrequency();
        }

        public override async Task PerformTest()
        {
            await Instrument.StopGenerating();
            await Instrument.SetTxFrequency(RXFrequency);
            await Instrument.GenerateSignal(GenLevel);
            float measRssi = await Repeater.ReadRSSI();
            LogCallback(String.Format("Measured RSSI at {0}MHz: {1}db (expected {2}db)", (RXFrequency / 1000000D), measRssi, GenLevel));
            await Instrument.StopGenerating();
        }

        public override async Task Teardown()
        {
            //
        }
    }
}
