using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.Astro25
{
    public class MotorolaAstro25_TestTX_DeviationBalance : MotorolaXCMPRadio_TestTX_DeviationBalance
    {
        public MotorolaAstro25_TestTX_DeviationBalance(XCMPRadioTestParams testParams): base(testParams)
        {
            //
        }

        public override async Task Setup()
        {
            await base.Setup();
            TXFrequencies = MotorolaAstro25_Frequencies.TxFrequencies(Radio);
        }
    }
}
