using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XPR
{
    public class MotorolaXPR_TestTX_PowerCharacterization : MotorolaXCMPRadio_TestTX_PowerCharacterization
    {
        public MotorolaXPR_TestTX_PowerCharacterization(XCMPRadioTestParams testParams): base(testParams)
        {
            //
        }

        public override async Task Setup()
        {
            await base.Setup();
            TXFrequencies = MotorolaXPR_Frequencies.TxPowerFrequencies(Radio);
        }
    }
}
