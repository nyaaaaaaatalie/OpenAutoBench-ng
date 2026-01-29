using OpenAutoBench_ng.Communication.Instrument;
using System.Data;
using System.Reflection;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

public class MotorolaXCMPRadio_TestRX_RSSI : IBaseTest
{
    // private variables specific to the test
    protected int[] TXFrequencies; //Technically these are "RX Frequencies for this test, but there's no harm done using the TX frequencies for testing the receiver Maybe we should rename this field to not make it TX Specific
    protected MotorolaXCMPRadioBase Radio;

    public MotorolaXCMPRadio_TestRX_RSSI(XCMPRadioTestParams testParams) : base("RX: RSSI Test", testParams.report, testParams.instrument, testParams.callback, testParams.ct)
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
        await Instrument.SetupRXTestFMMod();
        await Task.Delay(1000, Ct);
    }

    public override async Task PerformTest()
    {
        try
        {
            for (int i = 0; i < TXFrequencies.Length; i++)
            {
                int currFreq = TXFrequencies[i];

                // Skip any 7/800 TX-Only Freqs
                if (currFreq is > 785000000 and < 850000000)
                {
                    Console.WriteLine($"Skipping RSSI test for frequency {currFreq / 1e6:0.000000} MHz, TX-only 7/800 MHz band");
                    continue;
                }

                Radio.SetReceiveConfig(XCMPRadioReceiveOption.CSQ);
                Radio.SetRXFrequency(currFreq, Bandwidth.BW_25kHz, RxModulation.C4FM);
                await Instrument.SetTxFrequency(currFreq);

                await Task.Delay(500, Ct);
                await Instrument.GenerateSignal(-50); //For future version, this should be a customizable value, Note: R2670 max gen level on RF I/O port is -50 dBm
                await Task.Delay(1500, Ct);
                byte rssi = Radio.GetStatus(StatusOperation.RSSI)[0];

                await Instrument.StopGenerating();
                Report.AddResult(OpenAutoBench.ResultType.RSSI, (float)rssi, 150, 140, 255, currFreq);
                LogCallback(String.Format("Measured RSSI at {0:0.000000} MHz: {1}", (currFreq / 1000000D), rssi));
            }
        }
        catch (OperationCanceledException)
        {
            throw;
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
