using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XPR
{
    public class MotorolaXPR_TestTX_ReferenceOscillator : MotorolaXCMPRadio_TestTX_ReferenceOscillator
    {
        public MotorolaXPR_TestTX_ReferenceOscillator(XCMPRadioTestParams testParams): base(testParams)
        {
            //
        }

        public override async Task Setup()
        {
            await base.Setup();
            TXFrequency = MotorolaXPR_Frequencies.TxRefOscFrequencies(radio);
        }
    }
}
