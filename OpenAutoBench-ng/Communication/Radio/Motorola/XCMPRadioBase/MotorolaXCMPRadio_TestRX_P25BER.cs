using OpenAutoBench_ng.Communication.Instrument;
using System.Reflection;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

public class MotorolaXCMPRadio_TestRX_P25BER : IBaseTest
{
    //private variables specific to the test
    protected MotorolaXCMPRadioBase Radio;
    protected int[] TXFrequencies; //Technically these are "RX Frequencies for this test, but there's no harm done using the TX frequencies for testing the receiver Maybe we should rename this field to not make it TX Specific

    public MotorolaXCMPRadio_TestRX_P25BER(XCMPRadioTestParams testParams) : base("RX: P25 BER Test", testParams.report, testParams.instrument, testParams.callback, testParams.ct)
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
        await Instrument.SetDisplay(InstrumentScreen.Generate);
        await Task.Delay(1000, Ct);
        await Instrument.SetupRXTestP25BER();
        await Task.Delay(1000, Ct);
    }

    public override async Task PerformTest()
    {
        try
        {
            Radio.SetReceiveConfig(XCMPRadioReceiveOption.STD_1011);
            for (int i= 0; i < TXFrequencies.Length; i++)
            {
                int currFreq = TXFrequencies[i];

                // Skip any 7/800 TX-Only Freqs
                if (currFreq is > 785000000 and < 850000000)
                {
                    Console.WriteLine($"Skipping BER test for frequency {currFreq / 1e6:0.000000} MHz, TX-only 7/800 MHz band");
                    continue;
                }

                Radio.SetRXFrequency(currFreq, Bandwidth.BW_25kHz, RxModulation.C4FM);
                await Instrument.SetTxFrequency(currFreq);
                await Task.Delay(5000, Ct);
                await Instrument.GenerateP25STDCal(-116); //For future version, this should be a customizable value
                await Task.Delay(5000, Ct);
                double BER = Radio.GetP25BER(4);
                await Instrument.StopGenerating();
                Report.AddResult(OpenAutoBench.ResultType.BIT_ERROR_RATE, (float)BER, 0.0f, 0.0f, 1.0f, currFreq);
                LogCallback(String.Format("Measured BER at {0}MHz: {1}%", (currFreq / 1000000D), BER));
            }
        }
        catch (Exception ex)
        {
            Report.AddError(OpenAutoBench.ResultType.BIT_ERROR_RATE, ex.ToString());
            LogCallback(String.Format("Test failed: {0}", ex.ToString()));
            throw new Exception("Test failed.", ex);
        }
    }

    public override async Task Teardown()
    {
        //
    }

}
