using OpenAutoBench_ng.Communication.Instrument;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestTX_PowerCharacterization :IBaseTest
    {
        // private vars specific to test
        protected MotorolaXCMPRadioBase Radio;

        protected int[] TXFrequencies;

        protected int[] CharPoints;

        protected MotorolaXCMPRadioBase.SoftpotParams txPwrCharPoints;

        public MotorolaXCMPRadio_TestTX_PowerCharacterization(XCMPRadioTestParams testParams) : base("TX: Power Characterization", testParams.report, testParams.instrument, testParams.callback, testParams.ct)
        {
            Radio = testParams.radio;
        }

        public override bool IsRadioEligible()
        {
            return Radio.ModelNumber.StartsWith("M20S") ||
                   Radio.ModelNumber.StartsWith("H92U") ||
                   Radio.ModelNumber.StartsWith("H91T");
        }

        public override async Task Setup()
        {
            LogCallback(String.Format("Setting up for {0}", Name));
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Task.Delay(1000, Ct);
            await Instrument.SetupTXPowerTest();
            await Task.Delay(1000, Ct);
            //CharPoints = Radio.GetTXPowerPoints();
            //Console.WriteLine($"Got TX power characterization points: {CharPoints.ToString()}");
        }

        public override async Task PerformTest()
        {
            try
            {
                for (int i = 0; i < txPwrCharPoints.Frequencies.Length; i++)
                {
                    Radio.SetTXFrequency(txPwrCharPoints.Frequencies[i], Bandwidth.BW_25kHz, TxDeviation.NoModulation);
                    await Instrument.SetRxFrequency(txPwrCharPoints.Frequencies[i], testMode.ANALOG);

                    // Set Low Power
                    Radio.SetTransmitPower(TxPowerLevel.Low);
                    Radio.Keyup();

                    // Measure
                    await Task.Delay(5000, Ct);
                    float measPow = await Instrument.MeasurePower();
                    measPow = (float)Math.Round(measPow, 2);
                    Radio.Dekey();

                    // TODO: Determine what the actual target output powers are based on the softpot settings
                    Report.AddResult(OpenAutoBench.ResultType.TX_POWER, measPow, 0.0f, 0.75f, 7.0f, txPwrCharPoints.Frequencies[i]);
                    LogCallback(String.Format("TX Low Power Point at {0}MHz: {1}w", (txPwrCharPoints.Frequencies[i] / 1000000D), measPow));
                    await Task.Delay(500, Ct);

                    // high power
                    Radio.SetTransmitPower(TxPowerLevel.High);
                    Radio.Keyup();

                    // Measure
                    await Task.Delay(5000, Ct);
                    measPow = await Instrument.MeasurePower();
                    measPow = (float)Math.Round(measPow, 2);
                    Radio.Dekey();

                    // TODO: Determine what the actual target output powers are based on the softpot settings
                    Report.AddResult(OpenAutoBench.ResultType.TX_POWER, measPow, 0.0f, 0.75f, 7.0f, txPwrCharPoints.Frequencies[i]);
                    LogCallback(String.Format("TX High Power Point at {0}MHz: {1}w", (txPwrCharPoints.Frequencies[i] / 1000000D), measPow));
                    
                    await Task.Delay(500, Ct);
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
