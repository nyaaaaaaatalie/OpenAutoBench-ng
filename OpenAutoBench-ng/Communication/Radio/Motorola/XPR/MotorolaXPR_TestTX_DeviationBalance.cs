using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XPR
{
    public class MotorolaXPR_TestTX_DeviationBalance : MotorolaXCMPRadio_TestTX_DeviationBalance
    {
        public MotorolaXPR_TestTX_DeviationBalance(XCMPRadioTestParams testParams): base(testParams)
        {
            //
        }

        public override async Task Setup()
        {
            await base.Setup();
            TXFrequencies = MotorolaXPR_Frequencies.TxDeviationFrequencies(Radio);
        }
    }
}
