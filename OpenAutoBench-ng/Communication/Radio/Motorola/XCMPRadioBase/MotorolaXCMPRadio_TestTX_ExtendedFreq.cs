using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadio_TestTX_ExtendedFreq : IBaseTest
    {
        public string name
        {
            get
            {
                return "TX: Extended Frequency Test";
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

        public MotorolaXCMPRadio_TestTX_ExtendedFreq(XCMPRadioTestParams testParams)
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
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Task.Delay(1000);
        }

        public async Task performTest()
        {
            try
            {
                for (int i=StartFrequency; i<=EndFrequency; i+=StepFrequency)
                {
                    Radio.SetTransmitConfig(XCMPRadioTransmitOption.REFOSC);
                    Radio.SetTXFrequency(i, false);
                    await Instrument.SetRxFrequency(i, testMode.ANALOG);
                    Radio.Keyup();
                    await Task.Delay(5000);
                    float measErr = await Instrument.MeasureFrequencyError();
                    float measPwr = await Instrument.MeasurePower();
                    Radio.Dekey();
                    await Task.Delay(1000);
                    
                    measErr = (float)Math.Round(measErr, 2);
                    LogCallback(String.Format("Measured frequency error at {0}MHz: {1}hz", (i / 1000000D), measErr));
                    LogCallback(String.Format("Measured power at {0}MHz: {1}w", (i / 1000000D), measPwr));

                    if (Instrument.SupportsP25)
                    {
                        Radio.SetTransmitConfig(XCMPRadioTransmitOption.STD_1011);
                        Radio.Keyup();
                        await Task.Delay(1500);
                        await Instrument.ResetBERErrors();
                        await Task.Delay(5000);
                        float measBer = await Instrument.MeasureP25RxBer();
                        Radio.Dekey();
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
                LogCallback(String.Format("Test failed: {0}", ex.ToString()));
                throw new Exception("Test failed.", ex);
            }
            finally
            {
                Radio.Dekey();
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
