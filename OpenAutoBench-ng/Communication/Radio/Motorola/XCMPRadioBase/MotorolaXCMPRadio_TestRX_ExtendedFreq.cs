using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestRX_ExtendedFreq : IBaseTest
    {
        public string name
        {
            get
            {
                return "RX: Extended Frequency Test";
            }
        }

        public bool pass { get; private set; }

        public bool testCompleted { get; private set; }

        protected IBaseInstrument Instrument;

        protected Action<string> LogCallback;

        protected MotorolaXCMPRadioBase Radio;

        protected int StartFrequency;
        protected int EndFrequency;
        protected int StepFrequency;

        public MotorolaXCMPRadio_TestRX_ExtendedFreq(XCMPRadioTestParams testParams)
        {
            LogCallback = testParams.callback;
            Radio = testParams.radio;
            Instrument = testParams.instrument;
            StartFrequency = testParams.ExtendedTestStart;
            EndFrequency = testParams.ExtendedTestEnd;
            StepFrequency = testParams.ExtendedTestStep;
        }

        public bool isRadioEligible()
        {
            return true;
        }

        public async Task setup()
        {
            LogCallback(String.Format("Setting up for {0}", name));
            await Instrument.SetDisplay(InstrumentScreen.Generate);
            await Task.Delay(1000);
            await Instrument.SetupRXTestFMMod();
            await Task.Delay(1000);
        }

        public async Task performTest()
        {
            try
            {
                for (int i=StartFrequency; i<=EndFrequency; i+=StepFrequency)
                {
                    Radio.SetReceiveConfig(XCMPRadioReceiveOption.CSQ);
                    Radio.SetRXFrequency(i, false);
                    await Instrument.SetTxFrequency(i);
                    await Task.Delay(5000);
                    await Instrument.GenerateSignal(-50);
                    await Task.Delay(5000);
                    byte[] rssi = Radio.GetStatus(MotorolaXCMPRadioBase.StatusOperation.RSSI);
                    await Instrument.StopGenerating();
                    LogCallback(String.Format("Measured RSSI at {0}MHz: {1}", (i / 1000000D), rssi[0]));


                    if (Instrument.SupportsP25)
                    {
                        Radio.SetReceiveConfig(XCMPRadioReceiveOption.STD_1011);
                        await Instrument.SetupRXTestP25BER();
                        await Task.Delay(5000);
                        await Instrument.GenerateP25STDCal(-116);
                        await Task.Delay(5000);
                        string BER = Radio.GetP25BER(4);
                        await Instrument.StopGenerating();
                        LogCallback(String.Format("Measured BER at {0}MHz: {1}", (i / 1000000D), BER));
                        await Instrument.SetupRXTestFMMod();
                        await Task.Delay(5000);
                    }
                    else
                    {
                        LogCallback("Skipping BER due to no instrument support");
                    }
                }
                
            }
            catch (Exception ex)
            {
                LogCallback(String.Format("Test failed: {0}", ex.ToString()));
                throw new Exception("Test failed.", ex);
            }

        }

        public async Task performAlignment()
        {
            throw new NotImplementedException();
        }

        public async Task teardown()
        {
            //
        }
    }
}
