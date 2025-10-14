using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;
using OpenAutoBench_ng.OpenAutoBench;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XPR
{
    public class MotorolaXPR : MotorolaXCMPRadioBase
    {
        private MotorolaXNLConnection _xnl;
        public MotorolaXPR(IXCMPRadioConnection conn): base(conn)
        {
            // load XNL keys
            Preferences prefs = new Preferences();
            Settings settings = prefs.Load();
            _xnl = new MotorolaXNLConnection(conn, settings.MotoTrboKeys, settings.MotoTrboDelta, 0);
            base._connection = _xnl;
        }

        public async Task PerformTests(XCMPRadioTestParams testParams)
        {
            if (testParams.doRefoscTest)
            {
                MotorolaXPR_TestTX_ReferenceOscillator test = new MotorolaXPR_TestTX_ReferenceOscillator(testParams);
                await test.Setup();
                await test.PerformTest();
                await test.Teardown();
            }
            await Task.Delay(1000);

            if (testParams.doDeviationTest)
            {
                MotorolaXPR_TestTX_DeviationBalance test = new MotorolaXPR_TestTX_DeviationBalance(testParams);
                await test.Setup();
                await test.PerformTest();
                await test.Teardown();
            }
            await Task.Delay(1000);

            if (testParams.doPowerTest)
            {
                MotorolaXPR_TestTX_PowerCharacterization test = new MotorolaXPR_TestTX_PowerCharacterization(testParams);
                await test.Setup();
                await test.PerformTest();
                await test.Teardown();
            }
            await Task.Delay(1000);

            testParams.radio.ResetRadio();
        }

        public override int[] GetTXPowerPoints()
        {
            byte[] cmd = new byte[4];

            // softpot opcode
            cmd[0] = 0x00;
            cmd[1] = 0x01;

            cmd[2] = 0x03;  // readall

            cmd[3] = 0x11;  // get TX power characterization points

            byte[] temp = Send(cmd);

            byte[] result = new byte[temp.Length - 5];
            Array.Copy(temp, 5, result, 0, temp.Length - 5);

            int[] returnVal = new int[result.Length / 2];

            for (int i = 0; i < returnVal.Length; i++)
            {
                returnVal[i] |= (result[i * 2] << 8);
                returnVal[i] |= result[(i * 2) + 1];
            }

            return returnVal;
        }
    }
}