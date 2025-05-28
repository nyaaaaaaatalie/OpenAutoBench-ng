using OpenAutoBench_ng.Communication.Instrument;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestTX_PowerCharacterization :IBaseTest
    {
        // private vars specific to test
        protected MotorolaXCMPRadioBase Radio;

        protected int[] TXFrequencies;

        protected int[] CharPoints;

        private int SOFTPOT_TX_CHAR_POINTS = 0x11;

        public MotorolaXCMPRadio_TestTX_PowerCharacterization(XCMPRadioTestParams testParams) : base("TX: Power Characterization", testParams.report, testParams.instrument, testParams.callback, testParams.ct)
        {
            Radio = testParams.radio;
        }

        public override bool IsRadioEligible()
        {
            return Radio.ModelNumber.StartsWith("M20S") ||
                   Radio.ModelNumber.StartsWith("H92U") ;
        }

        public override async Task Setup()
        {
            LogCallback(String.Format("Setting up for {0}", Name));
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Task.Delay(1000, Ct);
            await Instrument.SetupTXPowerTest();
            await Task.Delay(1000, Ct);
            CharPoints = Radio.GetTXPowerPoints();
        }

        public override async Task PerformTest()
        {
            try
            {
                for (int i = 0; i < TXFrequencies.Length; i++)
                {
                    Radio.SetTXFrequency(TXFrequencies[i], false);
                    await Instrument.SetRxFrequency(TXFrequencies[i], testMode.ANALOG);

                    // low power
                    Radio.Keyup();
                    await Task.Delay(500, Ct);
                    Radio.SoftpotUpdate(MotorolaXCMPRadioBase.SoftpotType.TxPower, BitConverter.GetBytes((UInt16)CharPoints[i * 2]));
                    await Task.Delay(5000, Ct);
                    float measPow = await Instrument.MeasurePower();
                    measPow = (float)Math.Round(measPow, 2);
                    // TODO: Determine what the actual target output powers are based on the softpot settings
                    Report.AddResult(OpenAutoBench.ResultType.TX_POWER, measPow, 0.0f, 0.75f, 7.0f, TXFrequencies[i]);
                    LogCallback(String.Format("TX Low Power Point at {0}MHz: {1}w", (TXFrequencies[i] / 1000000D), measPow));
                    Radio.Dekey();
                    await Task.Delay(1000, Ct);

                    // high power
                    Radio.Keyup();
                    await Task.Delay(500, Ct);
                    Radio.SoftpotUpdate(MotorolaXCMPRadioBase.SoftpotType.TxPower, BitConverter.GetBytes((UInt16)CharPoints[i * 2 + 1]));
                    await Task.Delay(5000, Ct);
                    measPow = await Instrument.MeasurePower();
                    measPow = (float)Math.Round(measPow, 2);
                    // TODO: Determine what the actual target output powers are based on the softpot settings
                    Report.AddResult(OpenAutoBench.ResultType.TX_POWER, measPow, 0.0f, 0.75f, 7.0f, TXFrequencies[i]);
                    LogCallback(String.Format("TX High Power Point at {0}MHz: {1}w", (TXFrequencies[i] / 1000000D), measPow));
                    Radio.Dekey();
                    await Task.Delay(1000, Ct);

                }
            }
            catch (Exception ex)
            {
                Report.AddError(OpenAutoBench.ResultType.TX_POWER, ex.ToString());
                LogCallback(String.Format("Test failed: {0}", ex.ToString()));
                throw new Exception("Test failed.", ex);
            }
            finally
            {
                Radio.Dekey();
            }

        }

        public override async Task Teardown()
        {
            //
        }
    }
}
