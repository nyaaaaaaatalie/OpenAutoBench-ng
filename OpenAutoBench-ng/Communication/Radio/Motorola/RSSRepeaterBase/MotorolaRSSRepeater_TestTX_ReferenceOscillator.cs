using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.Quantar;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase
{
    public class MotorolaRSSRepeater_TestTX_ReferenceOscillator : IBaseTest
    {
        public string name
        {
            get
            {
                return "TX: Reference Oscillator";
            }
        }

        public bool pass { get; private set; }

        public bool testCompleted { get; private set; }

        private IBaseInstrument Instrument;

        private Action<string> LogCallback;

        private MotorolaRSSRepeaterBase Repeater;

        // private vars specific to test

        private int TXFrequency;

        private int tolerance = 50;    // absolute hz

        public MotorolaRSSRepeater_TestTX_ReferenceOscillator(MotorolaRSSRepeaterBaseTestParams testParams)
        {
            LogCallback = testParams.callback;
            Repeater = testParams.radio;
            Instrument = testParams.instrument;
        }

        public bool isRadioEligible()
        {
            return true;
        }

        public async Task setup()
        {
            LogCallback(String.Format("Setting up for {0}", name));
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Repeater.SetShell(MotorolaRSSRepeaterBase.Shell.RSS);
            TXFrequency = await Repeater.GetTxFrequency();
        }

        public async Task performTest()
        {
            await performTestWithReturn();
        }

        public async Task<float> performTestWithReturn()
        {
            float measErr = 0.0f;
            try
            {
                await Instrument.SetRxFrequency(TXFrequency, testMode.ANALOG);
                Repeater.Keyup();
                await Task.Delay(5000);
                measErr = await Instrument.MeasureFrequencyError();
                measErr = (float)Math.Round(measErr, 2);
                LogCallback(String.Format("Measured frequency error at {0}MHz: {1}hz", (TXFrequency / 1000000D), measErr));
            }
            catch (Exception ex)
            {
                LogCallback(String.Format("Test failed: {0}", ex.ToString()));
                throw new Exception("Test failed.", ex);
            }
            finally
            {
                Repeater.Dekey();
            }

            return measErr;
        }

        public async Task performAlignment()
        {
            float last = await performTestWithReturn();
            await Task.Delay(300);

            int maxStepSize = 32;  // As you set
            int stepSize = maxStepSize;
            bool lastSign = last > 0; // true for positive, false for negative

            while (Math.Abs(last) > tolerance)
            {
                // Check for sign change
                bool currentSign = last > 0;
                if (currentSign != lastSign)
                {
                    stepSize = Math.Max(1, stepSize / 2);  // Halve the step size, but ensure it's at least 1
                    lastSign = currentSign;
                }

                // Set direction based on the sign of the last error
                int step = last > 0 ? stepSize : -stepSize;

                await StepPend(step);
                await Task.Delay(500);
                last = await performTestWithReturn();
                await Task.Delay(500);
            }
        }


        public async Task StepPend(int step)
        {
            int pend = await GetPend();

            // Adjust step to ensure pendulum value remains within [0, 255]
            if (pend + step > 255)
            {
                step = 255 - pend;
            }
            else if (pend + step < 0)
            {
                step = -pend;
            }

            if (step == 0)  // If no change is needed, just return
            {
                return;
            }

            // direction is reversed for whatever godforsaken reason
            string dir = step < 0 ? "UP" : "DN";

            await Repeater.Send($"AL PEND {dir} {Math.Abs(step)}");
            await Task.Delay(300);
        }



        public async Task WritePend(int pendValue)
        {
            int pend = await GetPend();
            WritePend(pendValue - pend);

        }

        public async Task<int> GetPend()
        {
            string pend = await Repeater.Get("AL PEND RD");
            return int.Parse(pend);
        }

        public async Task teardown()
        {
            //
        }
    }
}
