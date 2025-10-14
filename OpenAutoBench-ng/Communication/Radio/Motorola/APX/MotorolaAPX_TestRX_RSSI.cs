using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.APX
{
    public class MotorolaAPX_TestRX_RSSI : MotorolaXCMPRadio_TestRX_RSSI
    {
        public MotorolaAPX_TestRX_RSSI(XCMPRadioTestParams testParams) : base(testParams)
        {

        }

        public override async Task Setup()
        {
            await base.Setup();
            TXFrequencies = MotorolaAPX_RefData.TxFrequencies(Radio);
        }
    }
}
