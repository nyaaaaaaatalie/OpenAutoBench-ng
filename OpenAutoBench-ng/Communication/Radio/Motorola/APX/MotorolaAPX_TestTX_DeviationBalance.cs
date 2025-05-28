using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.APX
{
    public class MotorolaAPX_TestTX_DeviationBalance : MotorolaXCMPRadio_TestTX_DeviationBalance
    {
        public MotorolaAPX_TestTX_DeviationBalance(XCMPRadioTestParams testParams): base(testParams)
        {
            //
        }

        public override async Task Setup()
        {
            await base.Setup();
            TXFrequencies = MotorolaAPX_RefData.TxFrequencies(Radio);
        }
    }
}
