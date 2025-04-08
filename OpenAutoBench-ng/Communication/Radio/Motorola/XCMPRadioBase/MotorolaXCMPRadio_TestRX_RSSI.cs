using OpenAutoBench_ng.Communication.Instrument;
using System.Reflection;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

public class MotorolaXCMPRadio_TestRX_RSSI : IBaseTest
{
    public string name
    {
        get
        {
            return " RX: RSSI Test";
        }

    }

    public bool pass { get; private set; }

    public bool testCompleted { get; private set; }

    protected IBaseInstrument Instrument;

    protected Action<string> LogCallback;

    protected MotorolaXCMPRadioBase Radio;

    //private variables specific to the test

    protected int[] TXFrequencies; //Technically these are "RX Frequencies for this test, but there's no harm done using the TX frequencies for testing the receiver Maybe we should rename this field to not make it TX Specific

    public MotorolaXCMPRadio_TestRX_RSSI(XCMPRadioTestParams testParams)
    {
        LogCallback = testParams.callback;
        Radio = testParams.radio;
        Instrument = testParams.instrument;
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
            for (int i = 0; i < TXFrequencies.Length; i++)
            {
                int currFreq = TXFrequencies[i];
                Radio.SetReceiveConfig(XCMPRadioReceiveOption.CSQ);
                Radio.SetRXFrequency(currFreq, false);
                await Instrument.GenerateFMSignal(-50f, currFreq);
                await Task.Delay(5000);
                byte[] rssi = Radio.GetStatus(MotorolaXCMPRadioBase.StatusOperation.RSSI);
                await Instrument.StopGenerating();
                LogCallback(String.Format("Measured RSSI at {0}MHz: {1}", (currFreq / 1000000D), rssi[0]));
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
        //RX Test No Aligment Possble
    }

    public async Task teardown()
    {
        //
    }

}
