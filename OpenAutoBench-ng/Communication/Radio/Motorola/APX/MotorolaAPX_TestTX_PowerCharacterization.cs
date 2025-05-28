using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.APX
{
    public class MotorolaAPX_TestTX_PowerCharacterization : MotorolaXCMPRadio_TestTX_PowerCharacterization
    {
        public MotorolaAPX_TestTX_PowerCharacterization(XCMPRadioTestParams testParams): base(testParams)
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
