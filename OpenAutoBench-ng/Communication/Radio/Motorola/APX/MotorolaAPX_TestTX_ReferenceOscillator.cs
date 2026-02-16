using CSPID;
using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;
using OpenAutoBench_ng.Communication.Radio.Motorola.XPR;
using OpenAutoBench_ng.OpenAutoBench;
using System.Collections;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.APX
{
    public class MotorolaAPX_TestTX_ReferenceOscillator : MotorolaXCMPRadio_TestTX_ReferenceOscillator
    {
        public MotorolaAPX_TestTX_ReferenceOscillator(XCMPRadioTestParams testParams) : base(testParams)
        {
            //
        }

        public async Task Setup()
        {

            await base.Setup();

            //On FREON Radios, the frequencies and softpots can be obtained trough the READ_ALL_FREQ opcode
            //For Mackiaw radios, READ_ALL_FREQ is not available, frequencies are obtained trough hardcoded lookup, and the softpots
            //settings are recovered individually trough serveral XMCP request
            if (MotorolaAPX_RefData.isFreon(Radio))
            {
                RefOscParams = Radio.SoftpotGetParams(SoftpotType.RefOsc);
                if (RefOscParams.Frequencies.Length > 1)
                    throw new Exception($"Ref Osc softpot returned more than 1 frequency, this is not currently supported!");
            }
            else
            {
                int[] frequencies = MotorolaAPX_RefData.TxFrequencies(Radio);
                
                RefOscParams.Frequencies = new int[1];
                RefOscParams.Frequencies[0] = frequencies[frequencies.Length - 1];
                
                RefOscParams.Min = MotorolaXCMPRadioBase.SoftpotBytesToValue(Radio.SoftpotGetMinimum(SoftpotType.RefOsc));
                RefOscParams.Max = MotorolaXCMPRadioBase.SoftpotBytesToValue(Radio.SoftpotGetMaximum(SoftpotType.RefOsc));
                
                RefOscParams.Values = new int[1];
                RefOscParams.Values[0] = MotorolaXCMPRadioBase.SoftpotBytesToValue(Radio.SoftpotGetValue(SoftpotType.RefOsc));

                RefOscParams.ByteLength = 2; 

                List<byte[]> softpotList = Radio.SoftpotReadAll(SoftpotType.RefOsc, RefOscParams.ByteLength);
                RefOscParams.Values[0] = MotorolaXCMPRadioBase.SoftpotBytesToValue(softpotList[0]);
            }
        }
    }
}
