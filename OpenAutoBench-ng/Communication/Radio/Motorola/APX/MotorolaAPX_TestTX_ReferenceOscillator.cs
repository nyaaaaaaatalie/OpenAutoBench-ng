using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.APX
{
    public class MotorolaAPX_TestTX_ReferenceOscillator : MotorolaXCMPRadio_TestTX_ReferenceOscillator
    {
        public MotorolaAPX_TestTX_ReferenceOscillator(XCMPRadioTestParams testParams): base(testParams)
        {
            //
        }

        public override async Task Setup()
        {
            await base.Setup();
            int[] TXFrequencies = MotorolaAPX_RefData.TxFrequencies(radio);
            TXFrequency = TXFrequencies[TXFrequencies.Length - 1];
        }
    }
}
