using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.APX
{
    public class MotorolaAPX_TestTX_DeviationBalance : MotorolaXCMPRadio_TestTX_DeviationBalance
    {
        public MotorolaAPX_TestTX_DeviationBalance(XCMPRadioTestParams testParams): base(testParams)
        {
            //
        }

        public async Task Setup()
        {
            await base.Setup();

            LogCallback(String.Format("Setting up for {0}", Name));

            if (MotorolaAPX_RefData.isFreon(Radio))
            {
                ModBalParams = Radio.SoftpotGetParams(SoftpotType.ModBalance);
            }
            else
            {
                int[] frequencies = MotorolaAPX_RefData.TxFrequencies(Radio);

                ModBalParams.Frequencies = new int[frequencies.Length];
                ModBalParams.Frequencies = frequencies;

                ModBalParams.Min = MotorolaXCMPRadioBase.SoftpotBytesToValue(Radio.SoftpotGetMinimum(SoftpotType.ModBalance));
                ModBalParams.Max = MotorolaXCMPRadioBase.SoftpotBytesToValue(Radio.SoftpotGetMaximum(SoftpotType.ModBalance));

                ModBalParams.ByteLength = 2;

                ModBalParams.Values = new int[frequencies.Length];

                List<byte[]> softpotList = Radio.SoftpotReadAll(SoftpotType.ModBalance, ModBalParams.ByteLength);
                for (int i = 0; i < softpotList.Count; i++)
                {
                    ModBalParams.Values[i] = MotorolaXCMPRadioBase.SoftpotBytesToValue(softpotList[i]);
                }


            }
        }
    }
}
