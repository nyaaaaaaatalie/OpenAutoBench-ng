using OpenAutoBench_ng.Communication.Instrument.Connection;
using System.Globalization;

namespace OpenAutoBench_ng.Communication.Instrument.GeneralDynamics_R2670
{
    public class GeneralDynamics_R2670Instrument: IBaseInstrument
    {
        private IInstrumentConnection Connection;

        public bool Connected { get; private set; }

        public bool SupportsP25 { get { return true; } }

        public bool SupportsDMR { get { return false; } }

        private int txFreq;

        public GeneralDynamics_R2670Instrument(IInstrumentConnection conn, int addr)
        {
            Connected = false;
            Connection = conn;

        }

        public async Task Connect()
        {
            Connection.Connect();
        }

        public async Task Disconnect()
        {
            Connection.Disconnect();
        }

        public async Task GenerateSignal(float power)
        {
            string freq = (txFreq / 1_000_000.0).ToString("F5");
            string pwr = power.ToString("F1");
            string str = "RG " + freq + ", 1, " + pwr + ", 1, 1\r";
            await Transmit(str);
        }

       /* public async Task GenerateFMSignal(float power, int frequency)
        {
            string freq = (frequency / 1_000_000.0).ToString("F5");
            string pwr = power.ToString("F1");
            string str = "RG "+ freq+ ", 1, " + pwr + ", 1, 1\r";
            await Transmit(str);
        } */

        public async Task StopGenerating()
        {
            // The R2670 continuously transmit in generate mode. To stop generating, we go bact to monitor mode.
            await Transmit($"ARM 140.00000 1, 1, 1\r");
            await Transmit("RM 140.00000, 1, 1, 1, 1\r"); 
        }

        public async Task SetGenPort(InstrumentOutputPort outputPort)
        {
            throw new NotImplementedException();
        }

        public async Task SetRxFrequency(int frequency)
        {
            
        }

        public async Task SetRxFrequency(int frequency, testMode mode)
        {

            if (mode == testMode.ANALOG)
            {
                string freq = (frequency / 1_000_000.0).ToString("F5");
                await Transmit($"RM {freq} 1, 1, 1, 1\r");
            }
            else if (mode == testMode.P25)
            {
                string freq = (frequency / 1_000_000.0).ToString("F5");
                await Transmit($"ARM {freq} 1, 1, 1\r");
            }
            else if (mode == testMode.DMR)
            {
                throw new NotImplementedException("R2670 does not support DMR.");
            }
        }

        public async Task SetTxFrequency(int frequency)
        {
            txFreq = frequency;
        }

        public async Task<float> MeasurePower()
        {

            await Transmit("MR 0;?2\r");
            string[] results = await GetReadings(1);

            float powerWatts = float.Parse(results[0].Split(',')[1].Replace("W", "").Trim(),
                                           NumberStyles.Float,
                                           CultureInfo.InvariantCulture);
            return powerWatts;
        }

        public async Task<float> MeasureFrequencyError()
        {
            // MR command, pg. 106
            await Transmit("MR 0;?1\r");
            string[] results = await GetReadings(1);

            float frequencyErrorHz = float.Parse(results[0].Split(',')[1].Replace("kHz", "").Trim(), NumberStyles.Float, CultureInfo.InvariantCulture) * 1000;
            return frequencyErrorHz;
        }

        public async Task<float> MeasureFMDeviation()
        {
            // MR command, pg. 106
            await Transmit("MR 0;?\r");
            string[] results = await GetReadings(4);
            float fmDeviationPosHz = float.Parse(results[2].Replace("MM+,", "").Replace("kHz", "").Trim(),
                                          NumberStyles.Float,
                                          CultureInfo.InvariantCulture) * 1000;

            float fmDeviationNegHz = float.Parse(results[3].Replace("MM-,", "").Replace("kHz", "").Trim(),
                                          NumberStyles.Float,
                                          CultureInfo.InvariantCulture) * 1000;

            float deviation = (Math.Abs(fmDeviationPosHz) + Math.Abs(fmDeviationNegHz)) / 2;
            return deviation;
        }

        public async Task<string> GetInfo()
        {
            return await Send("*IDN?");

        }

        public async Task Reset()
        {
            await Send("*RST");
        }

        public async Task SetDisplay(InstrumentScreen screen)
        {
            //throw new NotImplementedException();
        }

        public async Task<float> MeasureP25RxBer()
        {
            throw new NotImplementedException("The R2670 does not have a command to get the BER remotely. This test is not available with this instrument");
        }

        public Task<float> MeasurDMRRxBer()
        {
            throw new NotImplementedException("The R2670 does not support DMR.");
        }

        public Task<float> MeasureDMRRxBer()
        {
            throw new NotImplementedException();
        }

        public async Task ResetBERErrors()
        {
           // throw new NotImplementedException();
        }

        /**
         * PRIVATE METHODS
         */

        private async Task<string> Send(string command)
        {
            return await Connection.Send(command);
        }

        private async Task Transmit(string command)
        {
            await Connection.Transmit(command);
        }

        private async Task<string> ReadLine()
        {
            return await Connection.ReadLine();
        }

        /// <summary>
        /// Parses the four lines of data from the 2670
        /// </summary>
        /// <returns>
        /// Array of strings.
        /// Index 0: Frequency Error;
        /// Index 1: Power;
        /// Index 2: Deviation Positive;
        /// Index 3: Deviation Negative
        /// </returns>
        private async Task<string[]> ReadMRReadings()
        {
            List<string> valList = new List<string>();
            for (int i=0; i<4; i++)
            {
                string val = await ReadLine();
                valList.Add(val);
            }
            return valList.ToArray();
        }
        /// <summary>
        /// Parse the arguments from a querry command that returns multiple values. The number of expected values is provided as an argument
        /// </summary>
        /// <param name="nbrValues"></param>
        /// <returns></returns>
        private async Task<string[]> GetReadings(int nbrValues)
        {
            List<string> values = new List<string>();
            for (int i=0; i<nbrValues; i++)
            {
                string val = await ReadLine();
                values.Add(val);
            }
            return  values.ToArray();
        }

        public async Task SetupRefOscillatorTest_P25()
        {
            //Not implemented, but shouldn't raise an exception
        }

        public async Task SetupRefOscillatorTest_FM()
        {
            await Transmit("MODE 0\r"); // Standard mode
            await Transmit("RM 136.00000, 1, 1, 1, 0\r"); // Monitor, 20db attenuation, RF Port, FM, Wideband // The frequency is not relevant it will be adjusted during test sequence

        }

        public async Task SetupTXPowerTest()
        {
            await Transmit("MODE 0\r"); // Standard mode
            await Transmit("RM 137.00000, 1, 1, 1, 0\r"); // Monitor, 20db attenuation, RF Port, FM, Wideband // The frequency is not relevant it will be adjusted during test sequence
        }

        public async Task SetupTXDeviationTest()
        {
            await Transmit("MODE 0\r"); // Standard mode
            await Transmit("RM 138.00000, 1, 1, 1, 1\r"); // Monitor, 20db attenuation, RF Port, FM, Wideband // The frequency is not relevant it will be adjusted during test sequence
            await Transmit("FF 0, 2\r");
        }

        public async Task SetupTXP25BERTest()
        {
            throw new NotImplementedException("The R2670 does not have a command to get the BER remotely. This test is not available");
        }

        public async Task SetupRXTestFMMod()
        {
            await Transmit("MODE 0\r"); // Standard mode
            await Transmit("RM 139.00000, 1, 1, 1, 1\r"); // Monitor, 20db attenuation, RF Port, FM, Narrowband // The frequency is not relevant it will be adjusted during test sequence
        }

        public async Task SetupRXTestP25BER()
        {
            await Transmit("MODE 5\r");
            await Task.Delay(3000);
            await Transmit("ARG 141.00000, 1, -120.0, 1\r");
            await Task.Delay(3000);

            //There is no way to change the P25 Audio Generation features programatically, so keypress must be manially registered to configure this test
            //For this to work, the cursor in the P25 Audio Zone must be in the default spot

            await Transmit("GK 3, 2\r"); //AUD key press
            await Task.Delay(200);
            await Transmit("GK 2, 2\r"); // SK3 Key Press - This sets the code to 1011HZ Pattern
            await Task.Delay(200);
            await Transmit("GK 4, 0\r"); // Up Key Press
            await Task.Delay(200);
            await Transmit("GK 0, 2\r"); // 2 Key press
            await Task.Delay(200);
            await Transmit("GK 1, 1\r"); // 8 Key press
            await Task.Delay(200);
            await Transmit("GK 0, 3\r"); // 3 Key press
            await Task.Delay(200);
            await Transmit("GK 4, 4\r"); // TAB Key Press
            await Task.Delay(200);
            await Transmit("GK 2, 0"); // SK1 Key Press - Set P25 Transmit to continuous
            await Task.Delay(200);
            await Transmit("GK 4, 1"); // Down Key Press
            await Task.Delay(200);

        }

        public async Task GenerateP25STDCal(float power)
        {
            await Transmit("mode 5\r");
            string freq = (txFreq / 1_000_000.0).ToString("F5");
            string pwr = power.ToString("F1");
            string str = "ARG " + freq + ", 1, " + pwr + ", 1\r";
            await Transmit(str);
            await Task.Delay(1000);
        }
    }
    
}

