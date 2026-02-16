using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.Astro25
{
    public class MotorolaAstro25 : MotorolaXCMPRadioBase
    {
        public MotorolaAstro25(IXCMPRadioConnection conn) : base(conn)
        {
            //
        }

        public async Task PerformTests(XCMPRadioTestParams testParams)
        {
            /*
            if (testParams.doRefoscTest)
            {
                MotorolaAPX_TestTX_ReferenceOscillator test = new MotorolaAPX_TestTX_ReferenceOscillator(testParams);
                await test.setup();
                await test.performTest();
                await test.teardown();
            }
            await Task.Delay(1000);

            if (testParams.doPowerTest)
            {
                MotorolaAPX_TestTX_PowerCharacterization test = new MotorolaAPX_TestTX_PowerCharacterization(testParams);
                await test.setup();
                await test.performTest();
                await test.teardown();
            }

            await Task.Delay(1000);

            if (testParams.doDeviationTest)
            {
                MotorolaAPX_TestTX_DeviationBalance test = new MotorolaAPX_TestTX_DeviationBalance(testParams);
                await test.setup();
                await test.performTest();
                await test.teardown();
            }

            await Task.Delay(1000);

            if (testParams.doTxBer)
            {
                MotorolaAPX_TestTX_P25_BER test = new MotorolaAPX_TestTX_P25_BER(testParams);
                await test.setup();
                await test.performTest();
                await test.teardown();
            }
            */
        }

       /* public override int[] GetTXPowerPoints()
        {
            SoftpotMessage msg = new SoftpotMessage(MsgType.REQUEST, SoftpotOperation.READ, SoftpotType.TxPowerCharPoint);

            SoftpotMessage resp = SendSoftpot(msg);

            byte[] cmd = new byte[4];

            int[] returnVal = new int[resp.Value.Length / 2];

            returnVal[0] |= (resp.Value[0] << 8);
            returnVal[0] |= resp.Value[1];

            returnVal[1] |= (resp.Value[2] << 8);
            returnVal[1] |= resp.Value[3];

            return returnVal;
        }*/
        
    }
}
