using OpenAutoBench_ng.Communication.Instrument;
using System.Reflection;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

public class MotorolaXCMPRadio_TestRX_P25BER : IBaseTest
{
    public string name
    {
        get
        {
            return " RX: P25 BER Test";
        }

    }

    public bool pass { get; private set; }

    public bool testCompleted { get; private set; }

    protected IBaseInstrument Instrument;

    protected Action<string> LogCallback;

    protected MotorolaXCMPRadioBase Radio;

    //private variables specific to the test

    protected int[] TXFrequencies; //Technically these are "RX Frequencies for this test, but there's no harm done using the TX frequencies for testing the receiver Maybe we should rename this field to not make it TX Specific

    public MotorolaXCMPRadio_TestRX_P25BER(XCMPRadioTestParams testParams)
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
        await Instrument.SetupRXTestP25BER();
        await Task.Delay(1000);
    }

    public async Task performTest()
    {
        try
        {
            for (int i= 0; i < TXFrequencies.Length; i++)
            {
                int currFreq = TXFrequencies[i];
                Radio.SetReceiveConfig(XCMPRadioReceiveOption.STD_1011);
                Radio.SetRXFrequency(currFreq, false);
                await Task.Delay(5000);
                await Instrument.GenerateP25STDCal(-116, currFreq); //For future version, power should be a customizable value
                await Task.Delay(5000);
                string BER = Radio.GetP25BER(4);
                await Instrument.StopGenerating();
                LogCallback(String.Format("Measured BER at {0}MHz: {1}", (currFreq / 1000000D), BER));
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
