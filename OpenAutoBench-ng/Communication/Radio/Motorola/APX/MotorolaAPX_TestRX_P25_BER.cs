using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.APX
{
    public class MotorolaAPX_TestRX_P25_BER : MotorolaXCMPRadio_TestRX_P25BER
    {
        public MotorolaAPX_TestRX_P25_BER(XCMPRadioTestParams testParams): base(testParams)
        {
        
        }

        public override async Task Setup()
        {
            await base.Setup();
            TXFrequencies = MotorolaAPX_RefData.TxFrequencies(Radio);
        }
    }
}
