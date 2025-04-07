using Microsoft.Extensions.FileSystemGlobbing.Internal;
using OpenAutoBench_ng.Communication.Instrument.Connection;
using System.Collections.Generic;
using System.Reflection;

namespace OpenAutoBench_ng.Communication.Instrument.Astronics_R8000
{
    public class Astronics_R8000Instrument : IBaseInstrument
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

        public Astronics_R8000Instrument(IInstrumentConnection conn)
        {
            Connected = false;
            Connection = conn;
            conn.SetDelimeter("\r\n");
        }

        private async Task<string> Send(string command)
        {
            string result = await Connection.Send(command);
            string[] resultTemp = result.Split(":", 2);
            if (resultTemp[0] != "0")
            {
                throw new Exception($"Instrument returned error {resultTemp[0]}");
            }
            return resultTemp[1].Replace("\r", "").Replace("\n", "");

        }

        private async Task Transmit(string command)
        {
            await Connection.Transmit(command);
        }

        public async Task Connect()
        {
            Connection.Connect();
            await Task.Delay(100);
            await SetDisplay(InstrumentScreen.Monitor);
            await Send("SET RF:Frequency Error Units=Hertz");
            await Task.Delay(200);
            await Send("SET METER: PWR Meter Range=150W");
            await Task.Delay(200);
            await Send("SET METER:Subzone=Power Meter");
            await Task.Delay(200);

        }

        public async Task Disconnect()
        {
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
            await Send($"SET RF:Output Level={power}");
            await Task.Delay(3000);
            await SetRFPower(true);
        }

        private async Task SetRFPower(bool status)
        {
            bool currStatus = await Send("GET RF:RF Power") == "On" ? true : false;

            if (status != currStatus)
            {
                await Task.Delay(500);
                await Send("DO RF:RF Power");
            }
            await Task.Delay(500);
        }

        public async Task GenerateFMSignal(float power, float afFreq)
        {
            throw new NotImplementedException();
        }

        public async Task GenerateP251011Signal(float power)
        {
            await Send("SET P25:Gen Test Pattern=1011 Hz Tone");
            await Task.Delay(150);
            await GenerateSignal(power);
        }

        public async Task StopGenerating()
        {
            await SetRFPower(false);
        }

        public async Task SetGenPort(InstrumentOutputPort outputPort)
        {
            throw new NotImplementedException();
        }

        public async Task SetRxFrequency(int frequency)
        {
            await Send($"SET RF:Monitor Frequency={frequency} Hz");
            // necessary to wait a little while or else it will return busy
            await Task.Delay(5000);
        }

        public async Task SetTxFrequency(int frequency)
        {
            await Send($"SET RF:Generate Frequency={frequency} Hz");
            // necessary to wait a little while or else it will return busy
            await Task.Delay(5000);
        }

        public async Task<float> MeasurePower()
        {
            float dbm = float.Parse(await Send("GET METER:Measured Power"));
            float watts = (float) Math.Pow(10, (double) dbm / 10D) / 1000;
            return watts;
        }

        public async Task<float> MeasureFrequencyError()
        {
            return float.Parse(await Send("GET MONITOR:Frequency Error"));
        }

        public async Task<float> MeasureFMDeviation()
        {
            return float.Parse(await Send("GET MONITOR:Deviation+"));
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
            switch (screen)
            {
                case InstrumentScreen.Monitor:
                    await Send("SET SYSTEM:Mode Request = Monitor");
                    break;
                case InstrumentScreen.Generate:
                    await Send("SET SYSTEM:Mode Request = Generate");
                    break;
                default:
                    throw new Exception("Unknown screen requested");
            }
            await Task.Delay(5000);
        }

        public async Task<float> MeasureP25RxBer()
        {
            await Send("GO SYSTEM:P25");
            await Task.Delay(5000);
            await Send("SET P25:BER Test=Stop");
            await Task.Delay(1000);     // the times in the manual are a lie
            return float.Parse(await Send("GET P25:BER Result"));
        }

        public Task<float> MeasureDMRRxBer()
        {
            throw new NotImplementedException();
        }

        public async Task ResetBERErrors()
        {
            await Task.Delay(5000);
            await Send("SET P25:BER Test=Start");
        }

        public async Task SetupRefOscillatorTest_P25()
        {
            //Not implemented, but shouldn't raise an exception
        }

        public async Task SetupRefOscillatorTest_FM()
        {
            //Not implemented, but shouldn't raise an exception
        }

        public async Task SetupTXPowerTest()
        {
            //Not implemented, but shouldn't raise an exception
        }

        public async Task SetupTXDeviationTest()
        {
            //Not implemented, but shouldn't raise an exception
        }

        public async Task SetupTXP25BERTest()
        {
            //Not implemented, but shouldn't raise an exception
        }

        public async Task SetupRXTestFMMod()
        {
            //Not implemented, but shouldn't raise an exception
        }

        public async Task SetupRXTestP25BER()
        {
            //Not implemented, but shouldn't raise an exception

        }

        public async Task GenerateP25STDCal(float power)
        {
            //Not implemented, but shouldn't raise an exception
        }

    }
}

