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
            if (MotorolaAPX_RefData.isFreon(Radio))
            {
                txPwrCharPoints = Radio.SoftpotGetParams(SoftpotType.TxPowerCharPoint);
            }
            else
            {
                int[] frequencies = MotorolaAPX_RefData.TxFrequencies(Radio);

                txPwrCharPoints.Frequencies = new int[frequencies.Length*2];
                txPwrCharPoints.Frequencies = frequencies;

                //Has to be hardcoded, getMin and getMax OP_CODES not supported for this softpot type
                txPwrCharPoints.Min = 0;
                txPwrCharPoints.Max = 4095;

                txPwrCharPoints.ByteLength = 2;

                txPwrCharPoints.Values = new int[frequencies.Length*2];

                List<byte[]> softpotList = Radio.SoftpotReadAll(SoftpotType.TxPowerCharPoint, txPwrCharPoints.ByteLength);
                for (int i = 0; i < softpotList.Count; i++)
                {
                    txPwrCharPoints.Values[i] = MotorolaXCMPRadioBase.SoftpotBytesToValue(softpotList[i]);
                }
            }
        }
    }
}
