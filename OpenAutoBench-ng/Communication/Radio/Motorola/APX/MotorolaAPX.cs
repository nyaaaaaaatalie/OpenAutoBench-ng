using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;
using OpenAutoBench_ng.OpenAutoBench;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.APX
{
    public class MotorolaAPX : MotorolaXCMPRadioBase
    {
        public override string ModelName { get
            {
                return MotorolaAPX_RefData.ModelName(this);
            } }

        public MotorolaAPX(IXCMPRadioConnection conn) : base(conn)
        {
            //
        }

        public async Task PerformTests(XCMPRadioTestParams testParams)
        {
            // Power test before RefOsc test to let the radio warm up
            if (testParams.doPowerTest)
            {
                MotorolaAPX_TestTX_PowerCharacterization test = new MotorolaAPX_TestTX_PowerCharacterization(testParams);
                await test.Setup();
                await test.PerformTest();
                await test.Teardown();
            }

            testParams.ct.ThrowIfCancellationRequested();

            await Task.Delay(1000, testParams.ct);

            if (testParams.doRefoscTest)
            {
                MotorolaAPX_TestTX_ReferenceOscillator test = new MotorolaAPX_TestTX_ReferenceOscillator(testParams);
                await test.Setup();
                await test.PerformTest();
                await test.Teardown();
            }

            testParams.ct.ThrowIfCancellationRequested();

            await Task.Delay(1000, testParams.ct);

            if (testParams.doDeviationTest)
            {
                MotorolaAPX_TestTX_DeviationBalance test = new MotorolaAPX_TestTX_DeviationBalance(testParams);
                await test.Setup();
                await test.PerformTest();
                await test.Teardown();
            }

            testParams.ct.ThrowIfCancellationRequested();

            await Task.Delay(1000, testParams.ct);

            if (testParams.doTxBer)
            {
                MotorolaAPX_TestTX_P25_BER test = new MotorolaAPX_TestTX_P25_BER(testParams);
                await test.Setup();
                await test.PerformTest();
                await test.Teardown();
            }

            testParams.ct.ThrowIfCancellationRequested();

            await Task.Delay(1000, testParams.ct);

            if(testParams.doRssiTest)
            {
                MotorolaAPX_TestRX_RSSI test = new MotorolaAPX_TestRX_RSSI(testParams);
                await test.Setup();
                await test.PerformTest();
                await test.Teardown();
            }

            testParams.ct.ThrowIfCancellationRequested();

            await Task.Delay(1000, testParams.ct);

            if (testParams.doRxBer)
            {
                MotorolaAPX_TestRX_P25_BER test = new MotorolaAPX_TestRX_P25_BER(testParams);
                await test.Setup();
                await test.PerformTest();
                await test.Teardown();

            }

            testParams.ct.ThrowIfCancellationRequested();

            await Task.Delay(1000, testParams.ct);

            if (testParams.doTxExtendedTest)
            {
                MotorolaAPX_TestTX_ExtendedFreq test = new MotorolaAPX_TestTX_ExtendedFreq(testParams);
                await test.Setup();
                await test.PerformTest();
                await test.Teardown();
            }

            testParams.ct.ThrowIfCancellationRequested();

            await Task.Delay(1000, testParams.ct);

            if (testParams.doRxExtendedTest)
            {
                MotorolaAPX_TestRX_ExtendedFreq test = new MotorolaAPX_TestRX_ExtendedFreq(testParams);
                await test.Setup();
                await test.PerformTest();
                await test.Teardown();
            }
        }

        public async Task PerformAlignments(XCMPRadioTestParams testParams)
        {
            if (testParams.doRefoscTest)
            {
                MotorolaAPX_TestTX_ReferenceOscillator test = new MotorolaAPX_TestTX_ReferenceOscillator(testParams);
                await test.Setup();
                await test.PerformAlignment();
                await test.Teardown();
            }
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

        public override MotorolaBand[] GetBands()
        {
            byte[] fromRadio = GetVersion(VersionOperation.RFBand);
            byte[] bands = new byte[fromRadio.Length - 3];
            List<MotorolaBand> bandList = new List<MotorolaBand>();

            Array.Copy(fromRadio, 3, bands, 0, fromRadio.Length - 3);
            foreach (byte b in bands)
            {
                bandList.Add((MotorolaBand)b);
            }
            return bandList.ToArray();
        }

        
    }
}
