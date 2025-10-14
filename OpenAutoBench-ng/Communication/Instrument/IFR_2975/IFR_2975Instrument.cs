using OpenAutoBench_ng.Communication.Instrument.Connection;

namespace OpenAutoBench_ng.Communication.Instrument.IFR_2975
{
    public class IFR_2975Instrument : IBaseInstrument
    {
        private IInstrumentConnection Connection;

        public bool Connected { get; private set; }

        public bool SupportsP25 { get { return true; } }

        public bool SupportsDMR { get { return false; } }

        public string Manufacturer { get; private set; }
        public string Model { get; private set; }
        public string Serial { get; private set; }
        public string Version { get; private set; }

        public int ConfigureDelay { get { return 250; } }

        public IFR_2975Instrument(IInstrumentConnection conn)
        {
            Connected = false;
            Connection = new IFR_2975Connection(conn);
        }

        private async Task<string> Send(string command)
        {
            return await Connection.Send(command);
        }

        private async Task Transmit(string command)
        {
            await Connection.Transmit(command);
        }

        public async Task Connect()
        {
            Connection.Connect();
            await Send("LOCKOUT ON");

        }

        public async Task Disconnect()
        {
            await Send("LOCKOUT OFF");
            Connection.Disconnect();
        }

        public async Task<bool> TestConnection()
        {
            // TODO: Implement this
            Console.WriteLine("Connection test not yet implemented for instrument!");
            return false;
        }

        public async Task GenerateSignal(float power)
        { 
            
            await Send($"Generator RFLEVel {power.ToString()}");
            await Task.Delay(1000);
            await Send("Generator PTT 1");
        }

        public async Task GenerateFMSignal(float power)
        {
            await Send($"Generator RFLEVel {power.ToString()}");
            await Task.Delay(1000);
            await Send("Generator PTT 1");
        }
            

        public async Task StopGenerating()
        {
            await Send("Generator PTT 0");
        }

        public async Task SetGenPort(InstrumentOutputPort outputPort)
        {
            await Send("Generator RFOUTput 0"); // Set Generator port to T/R
        }

        public async Task SetRxFrequency(int frequency, testMode mode)
        {
            if (mode == testMode.ANALOG || mode == testMode.P25)
            {
                await Send($"Receiver FREQuency {frequency.ToString()} Hz");
            }
            else if (mode == testMode.DMR)
            {
                throw new NotImplementedException("The IFR2975 does not support DMR.");
            }
        }

        public async Task SetTxFrequency(int frequency)
        {
            await Send($"Generator FREQuency {frequency.ToString()} Hz");
        }

        public async Task<float> MeasurePower()
        {
            return float.Parse(await Send("Power VALue"));
        }

        public async Task<float> MeasureFrequencyError()
        {
            return float.Parse(await Send("RFError VALue"));
        }

        public async Task<float> MeasureFMDeviation()
        {
            return float.Parse(await Send("FMDev VALue")) * 1000F;
        }

        public async Task<bool> GetInfo()
        {
            // Get response from IDN which should be <company name>, <model number>, <serial number>, <firmware revision>
            string idenResp = await Send("*IDN?");
            try
            {
                string[] idenParams = idenResp.Split(',');
                Manufacturer = idenParams[0];
                Model = idenParams[1];
                Serial = idenParams[2];
                Version = idenParams[3];
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("IDN response invalid!");
                return false;
            }
            return true;
        }

        public async Task Reset()
        {
            await Send("*RST");
        }

        public async Task SetDisplay(InstrumentScreen screen)
        {
            //await Transmit("DISP " + displayName);
        }

        public async Task<float> MeasureP25RxBer()
        {
            string resp = await Send("Ber READING");
            // reading is percentage as decimal
            return float.Parse(resp.Split(" ")[0]) * 100;
        }

        public Task<float> MeasureDMRRxBer()
        {
            throw new NotImplementedException();
        }

        public async Task ResetBERErrors()
        {
            await Send("Ber RESETERRors");
        }

        public async Task SetupRefOscillatorTest_P25()
        {
            await Send("Receiver DEMODulation 14"); // P25 Wide Band
            await Send("Receiver FILter 0"); // No DEMOD Filter
            await Send("Receiver RFINput 0"); // Set Receiver to T/R Port
            await Send("Receiver IFGAIN 0"); // No gain applied on signal
            await Send("Receiver ATTENuation 20"); // 20db Attenuation on the receiver
            await Send("RFError AVG 10"); // Set up the RF Error to a 10 sample average
            await Send("RFError ENable 3"); // Enable RF Error meter
        }

        public async Task SetupRefOscillatorTest_FM()
        {
            await Send("Receiver ATTENuation 20"); // 20db Attenuation on the receiver
            await Send("Receiver DEMODulation 10"); // FM Wide Band
            await Send("Receiver FILter 0"); // No DEMOD Filter
            await Send("Receiver RFINput 0"); // Set Receiver to T/R Port
            await Send("Receiver IFGAIN 0"); // No gain applied on signal
            await Send("Receiver ATTENuation 20"); // 20db Attenuation on the receiver
            await Send("RFError AVG 10"); // Set up the RF Error to a 10 sample average
            await Send("RFError ENable 3"); // Enable RF Error meter
        }

        public async Task SetupTXPowerTest()
        {
            await Send("Receiver DEMODulation 11"); // No Demodulation
            await Send("Receiver FILter 0"); // No DEMOD Filter
            await Send("Receiver RFINput 0"); // Set Receiver to T/R Port
            await Send("Receiver IFGAIN 0"); // No gain applied on signal
            await Send("Receiver ATTENuation 20"); // 20db Attenuation on the receiver
            await Send("Power ENable On"); // Enable power meter
            await Send("Power ZERO"); // Zero the power meter while no signal is applied
            await Send("Power prange 0"); // Set the meter to auto range
        }

        public async Task SetupTXDeviationTest()
        {
            await Send("Receiver DEMODulation 3"); //  FM 60 kHz DEMOD.
            await Send("Receiver RFINput 0"); // Set Receiver to T/R Port 
            await Send("Receiver IFGAIN 0"); // No gain applied on signal
            await Send("Receiver FILter 4"); // 15 kHz Low-Pass
            await Send("Receiver ATTENuation 20"); // 20db Attenuation on the receiver
            await Send("FMDev Enable 3"); // Enable the deviation meter
            await Send("FMDev MATH 1"); //Average
            await Send("FMDev AVG 200"); //Number of samples for the averaging
            await Send("FMDev RUN 1"); //Indicate to the meter to start running continuous collection
        }

        public async Task SetupTXP25BERTest()
        {
            await Send("Receiver DEMODulation 14"); // P25 Wide Band
            await Send("Receiver RFINput 0"); // Set Receiver to T/R Port 
            await Send("Receiver IFGAIN 0"); // No gain applied on signal
            await Send("Receiver FILter 4"); // 15 kHz Low-Pass
            await Send("Receiver ATTENuation 20"); // 20db Attenuation on the receiver
            await Send("Ber ENable 3"); // Enable BER meter
            await Send("Ber FRAMES 8"); // Sample size of 8 frames
            await Send("Ber PATTERN 0"); // Standars 1011 P25 Test Pattern
            await Send("Ber PEAKhold 0"); // Disable peak mode
            await Send("Ber RUN 1"); // Indicate to the BER Meter to start continuous collection
            await Send("Ber RESETErrors"); // Reset Error counter
        }

        public async Task SetupRXTestFMMod()
        {
            await Send("Generator RFOUTput 0"); // Set Generator to T/R Port
            await Send("Generator DCPOWer 1"); // Turn Generator Mode On
            await Send("Generator PTT 0"); // Sets the Generator to Off
            await Send("Generator MOD 1"); // Sets Generator to FM Modulation
            await Send("FGen MODe3 1");  //Set Function Generator to Tone injection mode
            await Send("FGen Freq3 1000"); // 1KHz Tone Modulation
            await Send("FGen Dev3 3"); // 3kHz Deviation
            await Send("FGen SH3 0"); // Sine audio wave shape
        }

        public async Task SetupRXTestP25BER()
        {
            await Send("Generator RFOUTput 0"); // Set Generator to T/R Port
            await Send("Generator DCPOWer 1"); // Turn Generator Mode On
            await Send("Generator PTT 0"); // Sets the Generator to Off
            await Send("FGen MODe3 0");  //Disable Funtion Generator
            await Send("Generator MOD 8"); //Sets Generator to P25 Modulation
            await Send("Generator P25Mode 4"); // Sets 1011HZ Std pattern

        }

        public async Task GenerateP25STDCal(float power)
        {
            await Send($"Generator RFLEVel {power.ToString()}"); //Set Generator Power level
            await Task.Delay(1000);
            await Send("Generator PTT 1"); //Send signal
        }
    }
}
