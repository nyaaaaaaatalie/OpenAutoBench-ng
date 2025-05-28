using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.Quantar;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase
{
    public class MotorolaRSSRepeater_TestTX_ReferenceOscillator : IBaseTest
    {
        // private vars specific to test
        private MotorolaRSSRepeaterBase Repeater;
        private int TXFrequency;
        private int tolerance = 50;    // absolute hz

        public MotorolaRSSRepeater_TestTX_ReferenceOscillator(MotorolaRSSRepeaterBaseTestParams testParams) : base("TX: Reference Oscillator", testParams.report, testParams.instrument, testParams.callback, testParams.ct)
        {
            Repeater = testParams.radio;
        }

        public override bool IsRadioEligible()
        {
            return true;
        }

        public override async Task Setup()
        {
            LogCallback(String.Format("Setting up for {0}", Name));
            await Instrument.SetDisplay(InstrumentScreen.Monitor);
            await Repeater.SetShell(MotorolaRSSRepeaterBase.Shell.RSS);
            TXFrequency = await Repeater.GetTxFrequency();
        }

        public override async Task PerformTest()
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
                await Task.Delay(5000, Ct);
                measErr = await Instrument.MeasureFrequencyError();
                measErr = (float)Math.Round(measErr, 2);
                Report.AddResult(OpenAutoBench.ResultType.REF_OSC, measErr, TXFrequency, TXFrequency - 50, TXFrequency + 50, TXFrequency);
                LogCallback(String.Format("Measured frequency error at {0}MHz: {1}hz", (TXFrequency / 1000000D), measErr));
            }
            catch (Exception ex)
            {
                Report.AddError(OpenAutoBench.ResultType.REF_OSC, ex.ToString());
                LogCallback(String.Format("Test failed: {0}", ex.ToString()));
                throw new Exception("Test failed.", ex);
            }
            finally
            {
                Repeater.Dekey();
            }

            return measErr;
        }

        public override async Task PerformAlignment()
        {
            float last = await performTestWithReturn();
            await Task.Delay(300, Ct);

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
                await Task.Delay(500, Ct);
                last = await performTestWithReturn();
                await Task.Delay(500, Ct);
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
            await Task.Delay(300, Ct);
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

        public override async Task Teardown()
        {
            //
        }
    }
}
