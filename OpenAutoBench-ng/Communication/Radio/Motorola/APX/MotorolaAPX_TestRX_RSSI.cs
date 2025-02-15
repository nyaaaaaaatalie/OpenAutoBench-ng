using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.APX
{
    public class MotorolaAPX_TestRX_RSSI : MotorolaXCMPRadio_TestRX_RSSI
    {
        public MotorolaAPX_TestRX_RSSI(XCMPRadioTestParams testParams) : base(testParams)
        {

        }

        public async Task setup()
        {
            await base.setup();
            TXFrequencies = MotorolaAPX_Frequencies.TxFrequencies(Radio);
        }
    }
}
