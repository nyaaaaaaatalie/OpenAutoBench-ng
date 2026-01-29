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

            if (testParams.instrument.SupportsP25 && testParams.doTxBer)
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

            if (testParams.instrument.SupportsP25 && testParams.doRxBer)
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

            testParams.ct.ThrowIfCancellationRequested();

            await Task.Delay(1000, testParams.ct);

            if (testParams.doDeviationTest)
            {
                MotorolaAPX_TestTX_DeviationBalance test = new MotorolaAPX_TestTX_DeviationBalance(testParams);
                await test.Setup();
                await test.PerformAlignment();
                await test.Teardown();
            }
        }
        
        public override void SetTransmitPower(TxPowerLevel power)
        {
            // Override low power on APX8000 with low power (new)
            if (ModelNumber.StartsWith("H91T") && power == TxPowerLevel.Low)
                base.SetTransmitPower(TxPowerLevel.LowNew);
            else
                base.SetTransmitPower(power);
        }

    }
}
