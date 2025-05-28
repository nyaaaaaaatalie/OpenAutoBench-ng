using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;
using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.Quantar
{
    public class MotorolaQuantar_TestTX_Power : MotorolaRSSRepeater_TestTX_Power
    {
        public MotorolaQuantar_TestTX_Power(MotorolaRSSRepeaterBaseTestParams testParams) :
            base(testParams)
        {
        }

        public override async Task Setup()
        {
            await base.Setup();
            await Repeater.Send("ALN STNPWR RESET");
            await Repeater.Send("SET TX PWR 100");
            
        }
    }
}
