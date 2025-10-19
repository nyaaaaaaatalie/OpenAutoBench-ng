using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestTX_P25_BER : IBaseTest
    {
        // private vars specific to test
        protected MotorolaXCMPRadioBase Radio;
        protected int[] TXFrequencies;

        public MotorolaXCMPRadio_TestTX_P25_BER(XCMPRadioTestParams testParams) : base("TX: P25 Bit Error Rate (BER)", testParams.report, testParams.instrument, testParams.callback, testParams.ct)
        {
            Radio = testParams.radio;
        }

        public override bool IsRadioEligible()
        {
            return true;
        }

        public override async Task Setup()
        {
            LogCallback(String.Format("Setting up for {0}", Name));
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Task.Delay(1000, Ct);
            await Instrument.SetupTXP25BERTest();
            await Task.Delay(1000, Ct);

            // let child set frequency
        }

        public override async Task PerformTest()
        {
            if (Instrument.SupportsP25)
            {
                try
                {
                    Radio.SetTransmitConfig(XCMPRadioTransmitOption.STD_1011);
                    await Task.Delay(500, Ct);
                    foreach (int TXFrequency in TXFrequencies)
                    {
                        Radio.SetTXFrequency(TXFrequency, Bandwidth.BW_25kHz, TxDeviation.Default);
                        await Instrument.SetRxFrequency(TXFrequency, testMode.P25);
                        Radio.Keyup();
                        await Task.Delay(1500, Ct);
                        await Instrument.ResetBERErrors();
                        await Task.Delay(5000, Ct);
                        float measErr = await Instrument.MeasureP25RxBer();
                        Radio.Dekey();
                        measErr = (float)Math.Round(measErr, 4);
                        Report.AddResult(OpenAutoBench.ResultType.BIT_ERROR_RATE, measErr, 0.0f, 0.0f, 1.0f, TXFrequency);
                        LogCallback(String.Format("Measured BER at {0}MHz: {1}%", (TXFrequency / 1000000F), measErr));
                        await Task.Delay(1000, Ct);
                    }
                }
                catch (Exception ex)
                {
                    Report.AddError(OpenAutoBench.ResultType.BIT_ERROR_RATE, ex.ToString());
                    LogCallback(String.Format("Test failed: {0}", ex.ToString()));
                    throw new Exception("Test failed.", ex);
                }
                finally
                {
                    Radio.Dekey();
                }
            }

        }

        public override async Task Teardown()
        {
            //
        }
    }
}
