using OpenAutoBench_ng.Communication.Radio.Motorola.Quantar;
using System.IO.Ports;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadioBase : IBaseRadio
    {
        public string Name { get; private set; }

        public virtual string ModelName { get; private set; }

        public string SerialNumber { get; private set; }

        public string FirmwareVersion { get; private set; }

        public string InfoHeader { get; private set; }

        public string ModelNumber { get; private set; }

        protected IXCMPRadioConnection _connection;

        public enum VersionOperation : byte
        {
            HostSoftware = 0x00,
            DSPSoftware = 0x10,
            UCMSoftware = 0x20,
            MACESoftware = 0x23,
            BootloaderVersion = 0x30,
            TuningVersion = 0x40,
            CPVersion = 0x42,
            RFBand = 0x63,
            RFPowerLevel = 0x65
        }

        public enum StatusOperation : byte
        {
            RSSI = 0x02,
            BatteryLevel = 0x03,
            LowBattery = 0x04,
            ModelNumber = 0x07,
            SerialNumber = 0x08,
            ESN = 0x09,
            RadioID = 0x0E,
            RFPATemp = 0x1D,
            
        }

        public enum SoftpotOperation : byte
        {
            Read = 0x00,
            Write = 0x01,
            Update = 0x02,
            ReadAll = 0x03,
            WriteAll = 0x04,
            Autotune = 0x05,
            ReadMin = 0x06,
            ReadMax = 0x07,
            ReadFrequency = 0x08,
        }

        public MotorolaXCMPRadioBase(IXCMPRadioConnection conn)
        {
            Name = "";
            SerialNumber = "";
            ModelNumber = "";
            FirmwareVersion = "";
            InfoHeader = "";
            _connection = conn;

        }
        public void Connect(bool underTest = false)
        {
            _connection.Connect();
            if (!underTest)
            {
                SerialNumber = System.Text.Encoding.UTF8.GetString(GetStatus(StatusOperation.SerialNumber)).TrimEnd('\0');
                ModelNumber = System.Text.Encoding.UTF8.GetString(GetStatus(StatusOperation.ModelNumber)).TrimEnd('\0');
            }
        }

        public void Disconnect()
        {
            _connection.Disconnect();
        }

        public byte[] Send(byte[] data)
        {
            int opcodeOut = 0;
            opcodeOut |= (data[0] << 8);
            opcodeOut |= (data[1] & 0xFF);

            // expects to get an XCMP opcode and some data in, length is auto calculated
            byte[] toSend = new byte[data.Length + 2];

            int dataLen = data.Length;

            // length high and low bytes
            toSend[0] = (byte)((dataLen >> 8) & 0xFF);
            toSend[1] = (byte)(dataLen & 0xFF);

            Array.Copy(data, 0, toSend, 2, dataLen);

            //Console.WriteLine("Sending " + Convert.ToHexString(toSend));

            _connection.Send(toSend);

            // start a timer so we don't hold infinitely
            var startTime = DateTime.UtcNow;

            while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(5))
            {
                byte[] fromRadio = _connection.Receive();
                int len = 0;

                len |= (fromRadio[0] << 8) & 0xFF;
                len |= fromRadio[1];

                byte[] retval = new byte[len];

                Array.Copy(fromRadio, 2, retval, 0, len);

                int opcodeIn = 0;
                opcodeIn |= (retval[0] << 8);
                opcodeIn |= (retval[1] & 0xFF);

                if (opcodeIn - 0x8000 == opcodeOut)
                {
                    return retval;
                }
            }
            throw new TimeoutException("Radio did not reply in a timely manner.");
        }

        public byte[] GetVersion(VersionOperation oper)
        {
            byte[] cmd = new byte[3];

            // XCMP opcode
            cmd[0] = 0x00;
            cmd[1] = 0x0f;

            // the power index
            cmd[2] = (byte)oper;

            return Send(cmd);
        }

        public byte[] GetStatus(StatusOperation oper)
        {
            byte[] cmd = new byte[3];

            // XCMP opcode
            cmd[0] = 0x00;
            cmd[1] = 0x0e;

            // the status byte
            cmd[2] = (byte)oper;

            byte[] result = Send(cmd);

            byte[] returnVal = new byte[result.Length - 4];

            //Console.WriteLine("Length is " + returnVal.Length);

            Array.Copy(result, 4, returnVal, 0, result.Length - 4);

            return returnVal;
        }

        public void SetPowerLevel(int powerIndex)
        {
            byte[] cmd = new byte[3];
            
            // XCMP opcode
            cmd[0] = 0x00;
            cmd[1] = 0x06;
            
            // the power index
            cmd[2] = (byte)powerIndex;

            Send(cmd);
        }

        public void EnterServiceMode()
        {
            byte[] cmd = new byte[2];

            // XCMP opcode
            cmd[0] = 0x00;
            cmd[1] = 0x0c;

            Send(cmd);
        }

        public void ResetRadio()
        {
            byte[] cmd = new byte[2];

            // XCMP opcode
            cmd[0] = 0x00;
            cmd[1] = 0x0d;

            Send(cmd);
        }

        public void SetTXFrequency(int frequency, bool modulated)
        {
            // divide by 5 to fit in XCMP opcode
            frequency = frequency / 5;
            byte[] cmd = new byte[8];

            // XCMP opcode
            cmd[0] = 0x00;
            cmd[1] = 0x0b;

            // frequency
            cmd[2] = (byte) ((frequency >> 24) & 0xFF);
            cmd[3] = (byte) ((frequency >> 16) & 0xFF);
            cmd[4] = (byte) ((frequency >> 8) & 0xFF);
            cmd[5] = (byte) (frequency & 0xFF);

            // bw
            cmd[6] = 0x64;

            // modulated yes/no
            cmd[7] = Convert.ToByte(modulated);

            Send(cmd);
        }

        public void SetRXFrequency(int frequency, bool modulated)
        {
            // divide by 5 to fit in XCMP opcode
            frequency = frequency / 5;
            byte[] cmd = new byte[8];

            // XCMP opcode
            cmd[0] = 0x00;
            cmd[1] = 0x0a;

            // frequency
            cmd[2] = (byte)((frequency >> 24) & 0xFF);
            cmd[3] = (byte)((frequency >> 16) & 0xFF);
            cmd[4] = (byte)((frequency >> 8) & 0xFF);
            cmd[5] = (byte)(frequency & 0xFF);

            // bw
            cmd[6] = 0x64;

            // modulated yes/no
            cmd[7] = Convert.ToByte(modulated);

            Send(cmd);
        }

        public void Keyup()
        {
            byte[] cmd = new byte[3];

            // transmit opcode
            cmd[0] = 0x00;
            cmd[1] = 0x04;

            cmd[2] = 0x03;

            Send(cmd);
        }

        public void Dekey()
        {
            byte[] cmd = new byte[3];

            // receive opcode
            cmd[0] = 0x00;
            cmd[1] = 0x05;

            cmd[2] = 0x11;

            Send(cmd);
        }

        public void SoftpotRead(int id)
        {
            byte[] cmd = new byte[4];

            // receive opcode
            cmd[0] = 0x00;
            cmd[1] = 0x01;

            cmd[2] = 0x00;

            cmd[3] = (byte) id;

            Send(cmd);
        }
        public void SoftpotWrite(int id, int val)
        {
            throw new NotImplementedException();
        }

        public void SoftpotUpdate(int id, int val)
        {
            byte[] cmd = new byte[6];

            // receive opcode
            cmd[0] = 0x00;
            cmd[1] = 0x01;

            cmd[2] = 0x02;

            cmd[3] = (byte)id;

            cmd[4] = (byte)((byte) (val >> 8) & 0xFF);
            cmd[5] = (byte) (val & 0xFF);

            Send(cmd);
        }

        public virtual int[] GetTXPowerPoints()
        {
            throw new NotImplementedException();
        }

        public void SetTransmitConfig(XCMPRadioTransmitOption option)
        {
            byte[] cmd = new byte[3];

            // transmit config opcode
            cmd[0] = 0x00;
            cmd[1] = 0x02;

            cmd[2] = (byte)option;

            Send(cmd);
        }

        public void SetReceiveConfig(XCMPRadioReceiveOption option)
        {
            byte[] cmd = new byte[3];

            // receive config opcode
            cmd[0] = 0x00;
            cmd[1] = 0x03;

            cmd[2] = (byte)option;

            Send(cmd);

            cmd = new byte[3];

            // receive opcode
            cmd[0] = 0x00;
            cmd[1] = 0x05;
            cmd[2] = 0x01;

            Send(cmd);
        }

        public virtual MotorolaBand[] GetBands()
        {
            throw new NotImplementedException();
        }

        public double GetP25BER(int nbrFrames)
        {
            byte[] cmd = new byte[4];

            // receive config opcode
            cmd[0] = 0x00;
            cmd[1] = 0x03;

            cmd[2] = 0x21; //test Pattern - P25 1011 Standard
            cmd[3] = 0x00; // Mod Type - C4FM

            byte[] reply = Send(cmd);

            System.Threading.Thread.Sleep(500);

            //BER RX Test initialization opcode
            byte[]cmd1 = new byte[4];
            cmd1[0] = 0x00;
            cmd1[1] = 0x16;

            cmd1[2] = 0x02; //Operation
            cmd1[3] = (byte)nbrFrames; //Number of frames to be integrated for the BER measurement

            Send(cmd1);

            System.Threading.Thread.Sleep(800 * nbrFrames); // Giving the radio enough time before pulling BER measurement

            byte[] cmd2 = new byte[2];
            
            //RX BER SYNC Report opcode
            cmd2[0] = 0x00;
            cmd2[1] = 0x17;

            byte[] result = Send(cmd2);

            System.Threading.Thread.Sleep(500);

            //Discarding the 1st 3 bytes
            byte[] berReply = new byte[25];
            Array.Copy(result, 3, berReply, 0, 25);

            return CalculateP25BER(nbrFrames, berReply);
        }


        private static double CalculateP25BER(int nbrFrames, byte[] berReply)
        {
            string noOfBitError = ""; // Stores the number of bit errors as a string.
            double errorPercentage = -1; // Default is negative 1 if calculation fails

            int chunks = berReply.Length / 5; // Each chunk in the byte array is 5 bytes long.
            int lastFrameNumber = 0; // Tracks the last valid frame number.
            int currentIndex = 0; // Tracks the current index in the byte array.
            int totalBitsPerFrame = 3456; // Number of bits per frame for calculation.

            // Process each 5-byte chunk.
            while (chunks != 0)
            {
                int frameNumber = berReply[currentIndex]; // Extract the frame number from the first byte.

                if (frameNumber != 0) // Only process if the frame number is non-zero.
                {
                    // Handle wrap-around of frame numbers (assuming frame numbers cycle at 255).
                    if (lastFrameNumber == 255)
                    {
                        lastFrameNumber = 0;
                    }

                    // If the frame number is smaller than the last one, skip this chunk.
                    if (frameNumber < lastFrameNumber)
                    {
                        currentIndex += 5; // Move to the next chunk.
                        chunks--; // Decrease the chunk count.
                        continue;
                    }

                    // Analyze the second byte for sync status.
                    if (berReply[currentIndex + 1] == 1)
                    {
                        // "No Sync Detected"
                    }
                    else if (berReply[currentIndex + 1] == 0)
                    {
                        // "Sync Detected"
                    }
                    else if (berReply[currentIndex + 1] == 2)
                    {
                        // "Sync Indeterminate"
                    }

                    // If the current frame number is not greater than the last one, skip this chunk.
                    if (lastFrameNumber >= frameNumber)
                    {
                        currentIndex += 5; // Move to the next chunk.
                        chunks--; // Decrease the chunk count.
                        continue;
                    }

                    // Update the last processed frame number.
                    lastFrameNumber = frameNumber;

                    // Extract the 4-byte bit error count from the chunk (starting at the 3rd byte).
                    long bitErrorCount = Convert4ByteArraytoLong(0, berReply[currentIndex + 2], berReply[currentIndex + 3], berReply[currentIndex + 4]);
                    noOfBitError = bitErrorCount.ToString(); // Update the bit error count string.

                    // Calculate the bit error percentage.
                    double numerator = (double)(bitErrorCount * 100L); // Scale bit error count to percentage.
                    double denominator = (double)(nbrFrames * totalBitsPerFrame); // Total bits in all frames.
                    
                    // Store the result
                    errorPercentage = numerator / denominator;
                }

                currentIndex += 5; // Move to the next chunk.
                chunks--; // Decrease the chunk count.
            }

            return errorPercentage;
        }

        private static long Convert4ByteArraytoLong(byte byte1, byte byte2, byte byte3, byte byte4)
        {
            // Convert each byte to a long (to ensure no data loss during bitwise operations).
            long byte1AsLong = (long)((ulong)byte1); // Most significant byte (MSB)
            long byte2AsLong = (long)((ulong)byte2);
            long byte3AsLong = (long)((ulong)byte3);
            long byte4AsLong = (long)((ulong)byte4); // Least significant byte (LSB)

            // Shift the bytes into their proper positions in a 32-bit number.
            long byte1Shifted = byte1AsLong << 24; // Shift MSB to the most significant position.
            long byte2Shifted = byte2AsLong << 16; // Shift to the second-most significant position.
            long byte3Shifted = byte3AsLong << 8;  // Shift to the third-most significant position.

            // Combine all the shifted values to reconstruct the original 32-bit value.
            return byte1Shifted + byte2Shifted + byte3Shifted + byte4AsLong;
        }

    }
}
