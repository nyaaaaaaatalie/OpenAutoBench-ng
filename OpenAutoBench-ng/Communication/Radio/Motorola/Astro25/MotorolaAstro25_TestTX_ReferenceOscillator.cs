using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.Astro25
{
    public class MotorolaAstro25_TestTX_ReferenceOscillator : MotorolaXCMPRadio_TestTX_ReferenceOscillator
    {
        public MotorolaAstro25_TestTX_ReferenceOscillator(XCMPRadioTestParams testParams): base(testParams)
        {
            //
        }

        public override async Task Setup()
        {
            await base.Setup();
            int[] TXFrequencies = MotorolaAstro25_Frequencies.TxFrequencies(radio);
            TXFrequency = TXFrequencies[TXFrequencies.Length - 1];
        }
    }
}
