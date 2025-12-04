using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestRX_ExtendedFreq : IBaseTest
    {
        // Test-specific variables
        protected MotorolaXCMPRadioBase Radio;
        protected int StartFrequency;
        protected int EndFrequency;
        protected int StepFrequency;

        public MotorolaXCMPRadio_TestRX_ExtendedFreq(XCMPRadioTestParams testParams) : base("RX: Extended Frequency Test", testParams.report, testParams.instrument, testParams.callback, testParams.ct)
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
            await Instrument.SetDisplay(InstrumentScreen.Generate);
            await Task.Delay(1000, Ct);
            await Instrument.SetupRXTestFMMod();
            await Task.Delay(1000, Ct);
        }

        public override async Task PerformTest()
        {
            try
            {
                for (int i=StartFrequency; i<=EndFrequency; i+=StepFrequency)
                {
                    Radio.SetReceiveConfig(XCMPRadioReceiveOption.CSQ);
                    Radio.SetRXFrequency(i, Bandwidth.BW_25kHz, RxModulation.C4FM);
                    await Instrument.SetTxFrequency(i);
                    await Task.Delay(5000, Ct);
                    await Instrument.GenerateSignal(-47); // was -50
                    await Task.Delay(5000, Ct);
                    byte rssi = Radio.GetStatus(StatusOperation.RSSI)[0];

                    await Instrument.StopGenerating();
                    Report.AddResult(OpenAutoBench.ResultType.RSSI, rssi, 150, 150, 255, i);
                    LogCallback(String.Format("Measured RSSI at {0}MHz: {1}", (i / 1000000D), rssi));


                    if (Instrument.SupportsP25)
                    {
                        Radio.SetReceiveConfig(XCMPRadioReceiveOption.STD_1011);
                        await Instrument.SetupRXTestP25BER();
                        await Task.Delay(1000, Ct);

                        await Instrument.GenerateP25STDCal(-116);
                        await Task.Delay(5000, Ct);
                        double BER = Radio.GetP25BER(4);
                        await Instrument.StopGenerating();
                        Report.AddResult(OpenAutoBench.ResultType.BIT_ERROR_RATE, (float)BER, 0.0f, 0.0f, 1.0f, i);
                        LogCallback(String.Format("Measured BER at {0}MHz: {1}%", (i / 1000000D), BER));
                        await Instrument.SetupRXTestFMMod();

                        await Task.Delay(1000, Ct);

                    }
                    else
                    {
                        LogCallback("Skipping BER due to no instrument support");
                    }
                }
                
            }
            catch (Exception ex)
            {
                Report.AddError(OpenAutoBench.ResultType.RSSI, ex.ToString());
                LogCallback(String.Format("Test failed: {0}", ex.ToString()));
                throw new Exception("Test failed.", ex);
            }

        }

        public override async Task Teardown()
        {
            //
        }
    }
}
