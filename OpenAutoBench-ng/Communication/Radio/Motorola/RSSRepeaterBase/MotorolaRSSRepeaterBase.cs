using OpenAutoBench_ng.Communication.Radio.Motorola.Quantar;
using System.Collections;
using System.ComponentModel;
using System.IO.Ports;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase
{
    public class MotorolaRSSRepeaterBase : IBaseRadio
    {
        public string Name { get; private set; }

        public virtual string ModelName { get; private set; }

        public string ModelNumber { get; private set; }

        public string SerialNumber { get; private set; }

        public string FirmwareVersion { get; private set; }

        public string InfoHeader { get; private set; }

        public enum Shell
        {
            MAIN,
            RSS,
            RAP
        }

        private SerialPort _serialPort;

        private List<byte> PacketBuffer;

        private List<string> ReplyBuffer;

        private bool HasData = false;

        public MotorolaRSSRepeaterBase(string portName, int baud = 115200)
        {
            PacketBuffer = new List<byte>();
            ReplyBuffer = new List<string>();

            _serialPort = new SerialPort(portName);
            _serialPort.BaudRate = baud;
            _serialPort.NewLine = "\r\n";
            _serialPort.ReadTimeout = 1000;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);

            Name = "";
            SerialNumber = "";
            FirmwareVersion = "";
            InfoHeader = "";
        }
        public async Task Connect()
        {
            _serialPort.Open();
            await SetShell(Shell.RSS);
            await Task.Delay(100);
            // TODO: fix this
            // get name twice because buffer clear doesn't work
            FirmwareVersion = await GetFirmwareVersion();
            SerialNumber = await GetSerialNumber();
            Name = SerialNumber;
            
        }

        public void Disconnect()
        {
            _serialPort.Close();
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = sender as SerialPort;

            int toRead = sp.BytesToRead;
            byte[] inData = new byte[toRead];

            sp.Read(inData, 0, inData.Length);

            foreach (byte b in inData)
            {
                if (b == '\n')
                {
                    // we found the end of a command
                    ReplyBuffer.Add(System.Text.Encoding.UTF8.GetString(PacketBuffer.ToArray()));
                    //Console.WriteLine($"Got {System.Text.Encoding.UTF8.GetString(PacketBuffer.ToArray())}");
                    HasData = true;
                    PacketBuffer.Clear();
                }
                else
                {
                    PacketBuffer.Add(b);
                }
            }
        }

        private async Task<string> ReadLine()
        {
            while (!HasData)
            {
                await Task.Delay(5);
            }
            if (ReplyBuffer.Count <= 1)
            {
                HasData = false;
            }
            string result = ReplyBuffer[0].Replace("\n", "").Replace("\r", "");
            Console.WriteLine($"Read line {result}");
            ReplyBuffer.RemoveAt(0);
            return result; 
        }

        private void FlushBuffer()
        {
            //PacketBuffer.Clear();
            ReplyBuffer.Clear();
            HasData = false;
            //HasData = false;
        }

        public async Task PerformTests(MotorolaRSSRepeaterBaseTestParams testParams)
        {
            await AccessDisable();
            await Task.Delay(300);
            await SetShell(Shell.RSS);
            await Task.Delay(300);

            if (testParams.doRefoscTest)
            {
                MotorolaRSSRepeater_TestTX_ReferenceOscillator test = new MotorolaRSSRepeater_TestTX_ReferenceOscillator(testParams);
                await test.Setup();
                await Task.Delay(1000);
                await test.PerformTest();
                await Task.Delay(1000);
                await test.Teardown();

            }

            await Task.Delay(1000);

            if (testParams.doPowerTest)
            {
                MotorolaRSSRepeater_TestTX_Power test = new MotorolaRSSRepeater_TestTX_Power(testParams);
                await test.Setup();
                await Task.Delay(1000);
                await test.PerformTest();
                await Task.Delay(1000);
                await test.Teardown();
            }

            await Task.Delay(1000);

            if (testParams.doDeviationTest)
            {
                MotorolaRSSRepeater_TestTX_Deviation test = new MotorolaRSSRepeater_TestTX_Deviation(testParams);
                await test.Setup();
                await Task.Delay(1000);
                await test.PerformTest();
                await Task.Delay(1000);
                await test.Teardown();
            }

            await Task.Delay(1000);

            if (testParams.doRssiTest)
            {
                testParams.callback("Did RSSI test");
            }

            await Task.Delay(1000);
            await Reset();
            await Task.Delay(1000); // flush commands
        }

        public async Task PerformAlignment(MotorolaRSSRepeaterBaseTestParams testParams)
        {
            await AccessDisable();
            await Task.Delay(300);
            await SetShell(Shell.RSS);
            await Task.Delay(300);

            if (testParams.doRefoscTest)
            {
                MotorolaRSSRepeater_TestTX_ReferenceOscillator test = new MotorolaRSSRepeater_TestTX_ReferenceOscillator(testParams);
                await test.Setup();
                await Task.Delay(1000);
                await test.PerformAlignment();
                await Task.Delay(1000);
                await test.Teardown();
            }

            if (testParams.doPowerTest)
            {
                MotorolaRSSRepeater_TestTX_Power test = new MotorolaRSSRepeater_TestTX_Power(testParams);
                await test.Setup();
                await Task.Delay(1000);
                await test.PerformAlignment();
                await Task.Delay(1000);
                await test.Teardown();
            }
            await Task.Delay(1000);
            await Reset();
            await Task.Delay(1000); // flush commands
        }

        public async Task Transmit(string data)
        {
            _serialPort.WriteLine(data);
            FlushBuffer();
        }

        public async Task<string> Send(string data)
        {
            Console.WriteLine($"Sending {data}");
            await Task.Delay(75);
            // flush buffer
            FlushBuffer();
            _serialPort.WriteLine(data);
            await Task.Delay(150);
            // flush two lines to clear the echoed line (what you send comes back)
            string val = await ReadLine();
            Console.WriteLine($"Echo line: {val}");
            string line = await ReadLine();
            Console.WriteLine($"Reply line: {line}");
            // clean up the output a bit
            line = line.Replace("\n", "").Replace("\r", "").Trim();
            // see if repeater returned an error
            if (line.StartsWith("?"))
            {
                throw new Exception(string.Format("Repeater returned error: {0}", line));
            }
            return line;
        }

        public async Task<string> Get(string command)
        {
            string result = await Send(command);
            
            try
            {
                result = result.Split('=')[1].Trim();
            }
            catch (Exception e)
            {
                throw new Exception("Unable to split command. Got " + result);
            }

            return result;
        }

        public async Task SetShell(Shell shell)
        {
            try
            {
                await Transmit("EXIT");
            }
            catch
            {
                // do nothing; if we are in main it will error
            }
            
            // if we are requesting the main shell, then do nothing
            if (shell == Shell.MAIN)
            {
                return;
            }
            else if (shell == Shell.RSS)
            {
                await Transmit("DORSS");
            }
            else if (shell == Shell.RAP)
            {
                await Transmit("DORAP");
            }
            else
            {
                throw new NotImplementedException(string.Format("Requested to switch to unknown shell: {}", shell));
            }
        }

        public async Task<Shell> GetShell()
        {
            // send blank line to get the current shell
            string result = await Send("");
            Shell currShell = new Shell();
            if (result.Contains("]-O"))
            {
                currShell = Shell.MAIN;
            }
            else if (result.Contains("RSS"))
            {
                currShell = Shell.RSS;
            }
            else if (result.Contains("RAP"))
            {
                currShell = Shell.RAP;
            }
            else
            {
                throw new Exception(string.Format("Received unknown shell: {0}", result));
            }
            return currShell;
        }

        public async Task AccessEnable()
        {
            await SetShell(Shell.RAP);
            await Transmit("FP ACC_DIS OFF");
            await SetShell(Shell.RSS);
        }

        public async Task AccessDisable()
        {
            await SetShell(Shell.RAP);
            await Transmit("FP ACC_DIS ON");
            await SetShell(Shell.RSS);
        }

        public async Task Reset()
        {
            await SetShell(Shell.RSS);
            await Transmit("RESET");
        }

        public void Keyup()
        {
            Send("KEYUP");
        }

        public void Dekey()
        {
            Send("DEKEY");
        }

        public async Task<int> GetTxFrequency()
        {
            return int.Parse(await Get("GET TX FREQ"));
        }

        public async Task<int> GetRxFrequency()
        {
            return int.Parse(await Get("GET RX FREQ"));
        }

        public async Task<string> GetName()
        {
            return await Get("GET STN NAME");
        }

        public async Task<string> GetSerialNumber()
        {
            return await Get("GET STN SN");
        }

        public async Task<string> GetFirmwareVersion()
        {
            return await Get("GET FW_VER SC");
        }
    }
}
