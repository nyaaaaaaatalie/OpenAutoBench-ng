using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestTX_ExtendedFreq : IBaseTest
    {
        protected MotorolaXCMPRadioBase Radio;

        protected int StartFrequency;
        protected int EndFrequency;
        protected int StepFrequency;

        public MotorolaXCMPRadio_TestTX_ExtendedFreq(XCMPRadioTestParams testParams) : base("TX: Extended Frequency Test", testParams.report, testParams.instrument, testParams.callback, testParams.ct)
        {
            Radio = testParams.radio;
            StartFrequency = testParams.ExtendedTestStart;
            EndFrequency = testParams.ExtendedTestEnd;
            StepFrequency = testParams.ExtendedTestStep;
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
        }

        public override async Task PerformTest()
        {
            try
            {
                for (int i=StartFrequency; i<=EndFrequency; i+=StepFrequency)
                {
                    Radio.SetTransmitConfig(XCMPRadioTransmitOption.REFOSC);
                    Radio.SetTXFrequency(i, Bandwidth.BW_25kHz, TxDeviation.NoModulation);
                    await Instrument.SetRxFrequency(i, testMode.ANALOG);
                    Radio.Keyup();
                    await Task.Delay(5000, Ct);
                    float measErr = await Instrument.MeasureFrequencyError();
                    float measPwr = await Instrument.MeasurePower();
                    Radio.Dekey();
                    await Task.Delay(1000, Ct);
                    
                    measErr = (float)Math.Round(measErr, 2);

                    Report.AddResult(OpenAutoBench.ResultType.FREQ_ERROR, measErr, 0.0f, -50.0f, 50.0f, i);
                    LogCallback(String.Format("Measured frequency error at {0}MHz: {1}hz", (i / 1000000D), measErr));
                    
                    // TODO: add test result for power measurement, we need to determine what the target value is
                    LogCallback(String.Format("Measured power at {0}MHz: {1}w", (i / 1000000D), measPwr));

                    if (Instrument.SupportsP25)
                    {
                        Radio.SetTransmitConfig(XCMPRadioTransmitOption.STD_1011);
                        Radio.Keyup();
                        await Task.Delay(1500, Ct);
                        await Instrument.ResetBERErrors();
                        await Task.Delay(5000, Ct);
                        float measBer = await Instrument.MeasureP25RxBer();
                        Radio.Dekey();
                        Report.AddResult(OpenAutoBench.ResultType.BIT_ERROR_RATE, measBer, 0.0f, 0.0f, 1.0f, i);
                        LogCallback(String.Format("Measured BER at {0}MHz: {1}%", (i / 1000000D), measBer));
                    }
                    else
                    {
                        LogCallback("Skipping BER due to no instrument support");
                    }
                }
                
            }
            catch (Exception ex)
            {
                Report.AddError(OpenAutoBench.ResultType.FREQ_ERROR, ex.ToString());
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
