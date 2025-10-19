using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using OpenAutoBench_ng.Communication.Radio.Motorola.Quantar;
using System.IO.Ports;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

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

        public class XcmpMessage
        {
            /// <summary>
            /// XCMP message type
            /// </summary>
            public MsgType MsgType { get; private set; }
            /// <summary>
            /// Opcode for the request
            /// </summary>
            public Opcode Opcode { get; private set; }
            /// <summary>
            /// Result used for response messages
            /// </summary>
            public Result Result { get; set; }
            /// <summary>
            /// The byte data of the message
            /// </summary>
            public byte[] Data { get; set; }
            /// <summary>
            /// The length of all data excluding the starting two length bytes
            /// </summary>
            public int Length
            {
                get
                {
                    // Responses have an extra byte for status
                    if (MsgType == MsgType.RESPONSE)
                        return Data.Length + 3;
                    else
                        return Data.Length + 2;
                }
            }
            /// <summary>
            /// The length of all bytes in the message, including length bytes
            /// </summary>
            public int ByteLength
            {
                get
                {
                    return Length + 2;
                }
            }
            /// <summary>
            /// Get the XCMP message as bytes to send over a connection, including the starting length bytes
            /// </summary>
            public byte[] Bytes { 
                get
                {
                    // Create the new 
                    byte[] msg = new byte[ByteLength];
                    // Add length bytes
                    msg[0] = (byte)((Length >> 8) & 0xFF);
                    msg[1] = (byte)(Length & 0xFF);
                    // Generate Type/Opcode Header Bytes
                    byte[] header = GetTypeOpcodeHeader(MsgType, Opcode);
                    msg[2] = header[0];
                    msg[3] = header[1];
                    // Add optional result code and data
                    if (MsgType == MsgType.RESPONSE)
                    {
                        msg[4] = (byte)Result;
                        Array.Copy(Data, 0, msg, 5, Data.Length);
                    }
                    else
                    {
                        Array.Copy(Data, 0, msg, 4, Data.Length);
                    }
                    // return the array
                    return msg;
                } 
            }

            /// <summary>
            /// Create a new XCMP message of the specified type
            /// </summary>
            /// <param name="type"></param>
            public XcmpMessage(MsgType type, Opcode opcode)
            {
                MsgType = type;
                Opcode = opcode;
                Data = new byte[] { };
            }

            /// <summary>
            /// Parse an XCMP message from a byte aray including the starting length bytes
            /// </summary>
            /// <param name="data"></param>
            public XcmpMessage(byte[] data)
            {
                // Get length first
                int len = (data[0] << 8) + (data[1] & 0xFF);
                Console.WriteLine($"XCMP: Decoding message of length {len}");
                // Get type & opcode next
                UInt16 header = (UInt16)((data[2] << 8) + (data[3] & 0xFF));
                MsgType = GetMsgType(header);
                Opcode = GetOpcode(header);
                
                if (MsgType == MsgType.RESPONSE)
                {
                    Result = (Result)data[4];
                    Data = data.Skip(5).Take(len - 3).ToArray();
                    Console.WriteLine($"XCMP: Got MsgType {(byte)MsgType:X} ({Enum.GetName(MsgType)}), Opcode {(byte)Opcode:X} ({Enum.GetName(Opcode)}), Result {(byte)Result:X} ({Enum.GetName(Result)}), Data: [{Convert.ToHexString(Data)}]");
                }
                else
                {
                    Data = data.Skip(4).Take(len - 2).ToArray();
                    Console.WriteLine($"XCMP: Got MsgType {(byte)MsgType:X} ({Enum.GetName(MsgType)}), Opcode {(byte)Opcode:X} ({Enum.GetName(Opcode)}), Data: [{Convert.ToHexString(Data)}]");
                }
                // Validate
                if (Length != len)
                {
                    throw new Exception($"Decoded message lengths don't match (got {Length} but expected {len})");
                }
            }
        }

        public class SoftpotMessage : XcmpMessage
        {
            /// <summary>
            /// The softpot operation
            /// </summary>
            public SoftpotOperation Operation { 
                get
                {
                    return (SoftpotOperation)Data[0];
                }
                set
                {
                    Data[0] = (byte)value;
                }
            }
            /// <summary>
            /// The softpot type
            /// </summary>
            public SoftpotType Type
            {
                get
                {
                    return (SoftpotType)Data[1];
                }
                set
                {
                    Data[1] = (byte)value;
                }
            }
            /// <summary>
            /// The softpot value or values as a variable-length byte array
            /// </summary>
            public byte[] Value {  
                get
                {
                    // Return everything after the softpot oepration/type
                    return Data.Skip(2).ToArray();
                }
                set
                {
                    // Save the old values for recreating the array
                    SoftpotOperation oper = Operation;
                    SoftpotType type = Type;
                    // Create a new data array
                    Data = new byte[value.Length + 2];
                    Operation = oper;
                    Type = type;
                    // Copy the value into the data array
                    Array.Copy(value, 0, Data, 2, value.Length);
                }
            }
            /// <summary>
            /// Create a new Softpot-specific XCMP message
            /// </summary>
            /// <param name="msgType"></param>
            /// <param name="operation"></param>
            /// <param name="type"></param>
            public SoftpotMessage(MsgType msgType, SoftpotOperation operation, SoftpotType type) : base(msgType, Opcode.SOFTPOT)
            {
                // Start the data array with size 2
                Data = new byte[2];
                // Parse our values
                Operation = operation;
                Type = type;
            }
            /// <summary>
            /// Parse a softpot-specific XCMP message from a byte array
            /// </summary>
            /// <param name="data"></param>
            public SoftpotMessage(byte[] data) : base(data)
            {
            }
        }

        /// <summary>
        /// Softpot Parameters Struct
        /// </summary>
        public struct SoftpotParams
        {
            public int Min { get; set; }
            public int Max { get; set; }
            public int[] Frequencies { get; set; }
            public int ByteLength { get; set; }
            public int[] Values { get; set; }
        }

        public enum SoftpotType : byte
        {
            RefOsc = 0x00,
            TxPower = 0x01,
            ModBalance = 0x02,
            FrontendFilt1 = 0x03,
            CurrentLimit = 0x04,
            ModLimit = 0x05,
            TempComp = 0x06,
            TxPowerChar = 0x07,
            BattCal = 0x08,
            RFPABias1 = 0x09,
            RFPABias2 = 0x0A,
            RFPABias3 = 0x0B,
            RFPABias4 = 0x0C,
            FrontendFilt2 = 0x0D,
            FrontendFilt3 = 0x0E,
            RFPAGainCal = 0x0F,
            RFPAGainCalPoint = 0x10,
            TxPowerCharPoint = 0x11,
            IntMicGain = 0x12,
            ExtMicGain = 0x13,
            TxIQBal = 0x14,
            MaxTunedPwr = 0x15,
            HPDRSSIComp = 0x16,
            HPDRFPABias1 = 0x17,
            HPDRFPABias2 = 0x18,
            HPDRFPABias3 = 0x19,
            HPDRFPABias4 = 0x1A,
            HPDCurentLimit = 0x1B,
            HPDTxPower = 0x1C,
            HPDPhaseComp = 0x1D,
            HPDAmpComp = 0x1E,
            RxAttComp = 0x1F,
            FrontEndGain = 0x20,
            StepAtten = 0x21,
            Volume = 0x22,
            PwrCtrlAttOff = 0x23,
            DACn = 0x24,
            IntTempADC = 0x25,
            BattVoltADC = 0x26,
            PAVoltLimit = 0x27,
            PAMaxIset = 0x28,
            PwrCtrlBattParam = 0x29,
            BattVoltCutSlope = 0x2A,
            LowPortMod = 0x2B,
            PASatRef = 0x2C,
            SpurSetting = 0x2D,
            IntRDAC = 0x2E,
            RDACPwrChar = 0x2F,
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

        /// <summary>
        /// Convert a 4-byte motorola frequency to a frequency in Hz
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static UInt32 BytesToFrequency(byte[] data)
        {
            // Validate length
            if (data.Length != 4) { throw new ArgumentException("Frequency data must be 4 bytes!"); }
            // Convert bytes to int32 and multiply by 5 Hz (after reversing the endianess)
            return (UInt32)(BitConverter.ToInt32(data.Reverse().ToArray()) * 5);
        }

        /// <summary>
        /// Convert a frequency in Hz to a 4-byte motorola-specific array
        /// </summary>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public static byte[] FrequencyToBytes(int frequency)
        {
            // We reverse the byte order to get the endianness correct
            return BitConverter.GetBytes((UInt32)frequency / 5).Reverse().ToArray();
        }

        /// <summary>
        /// Convert an array of bytes to an integer value
        /// </summary>
        /// <param name="bytes">bytes to convert</param>
        /// <returns>converted integer value</returns>
        /// <exception cref="NotImplementedException">if byte size is not 8, 4, 2, or 1</exception>
        public static int SoftpotBytesToValue(byte[] bytes)
        {
            // flip byte array since softpot bytes are little-endian
            bytes = bytes.Reverse().ToArray();
            // Convert
            switch (bytes.Length)
            {
                case 4:
                    return BitConverter.ToInt32(bytes);
                case 2:
                    return BitConverter.ToInt16(bytes);
                case 1:
                    return bytes[0];
                default:
                    throw new NotImplementedException($"Value byte length of {bytes.Length} not supported!");
            }
        }

        /// <summary>
        /// Convert an integer value to an array of bytes
        /// </summary>
        /// <param name="val">value to convert</param>
        /// <returns>byte array</returns>
        /// <exception cref="NotImplementedException"></exception>
        public static byte[] SoftpotValueToBytes(int val, int byteLen)
        {
            // Bytes holder
            byte[] bytes;
            // Convert
            switch (byteLen)
            {
                case 4:
                    bytes = BitConverter.GetBytes(val);
                    break;
                case 2:
                    bytes = BitConverter.GetBytes((short)val);
                    break;
                case 1:
                    bytes = new byte[] { (byte)val };
                    break;
                default:
                    throw new NotImplementedException($"Value byte length of {byteLen} not supported!");
            }
            // Swap for little-endian
            bytes = bytes.Reverse().ToArray();
            // Return
            return bytes;
        }

        /// <summary>
        /// Get the XCMP opcode from the 2-byte type/opcode
        /// </summary>
        /// <param name="typeOpcode"></param>
        /// <returns></returns>
        public static Opcode GetOpcode(UInt16 header)
        {
            // Opcode is the lower 12 bits of the header
            return (Opcode)(header & 0xFFF);
        }
        /// <summary>
        /// Get the XCMP message type from the 2-byte type/opcode
        /// </summary>
        /// <param name="typeOpcode"></param>
        /// <returns></returns>
        public static MsgType GetMsgType(UInt16 header)
        {
            // MsgType is the top 4 bits of the header
            return (MsgType)((header & 0xF000) >> 12);
        }
        /// <summary>
        /// Get the XCMP message header (type + opcode) as a UInt16
        /// </summary>
        /// <param name="type"></param>
        /// <param name="opcode"></param>
        /// <returns></returns>
        public static byte[] GetTypeOpcodeHeader(MsgType type, Opcode opcode)
        {
            return new byte[2]
            {
                (byte)( ((byte)type << 4) + ((byte)opcode >> 8) ),
                (byte)( (UInt16)opcode & 0xFF)
            };
        }

        /// <summary>
        /// Connect to the attached radio
        /// </summary>
        /// <param name="underTest"></param>
        public void Connect(bool underTest = false)
        {
            _connection.Connect();
            if (!underTest)
            {
                SerialNumber = GetSerial();
                ModelNumber = GetModel();
                FirmwareVersion = $"HOST {GetVersion(VersionOperation.HostSoftware)}, DSP {GetVersion(VersionOperation.DSPSoftware)}";
                Console.WriteLine($"XCMP: connected to radio model {ModelNumber} (S/N {SerialNumber}, {FirmwareVersion})");
            }
        }

        /// <summary>
        /// Disconnect from the radio
        /// </summary>
        public void Disconnect()
        {
            _connection.Disconnect();

            Console.WriteLine($"XCMP: Disconnected from radio, seeya!");
        }

        /// <summary>
        /// Send an XCMP message and retrieve the response
        /// </summary>
        /// <param name="message">message to send</param>
        /// <param name="timeout">timeout in seconds to wait for a resposne</param>
        /// <returns></returns>
        public XcmpMessage Send(XcmpMessage message, int timeout = 1)
        {
            // Send the message
            //Console.WriteLine("XCMP: >>SNT>> " + Convert.ToHexString(message.Bytes));
            _connection.Send(message.Bytes);
            
            // Get the response
            var start = DateTime.Now;
            while (DateTime.Now < start + TimeSpan.FromSeconds(timeout))
            {
                // Get the response
                byte[] rx = _connection.Receive();
                XcmpMessage response = new XcmpMessage(rx);
                //Console.WriteLine("XCMP: <<RCV<< " + Convert.ToHexString(response.Bytes));

                // Validate it's a response
                if (response.MsgType != MsgType.RESPONSE)
                    throw new Exception($"Got non-response message! ({response.MsgType})");
                // Validate it was successful
                if (response.Result != Result.SUCCESS)
                    throw new Exception($"Response indicates {Enum.GetName(response.Result)}!");
                // Validate it matches
                if (response.Opcode != message.Opcode)
                    throw new Exception($"Received different opcode from what was sent! (Sent {message.Opcode} but got {response.Opcode})");
                // Return if everything is good
                
                return response;
            }

            // Throw a timeout if we timed out
            throw new TimeoutException("Radio did not reply in a timely manner.");
        }

        /// <summary>
        /// Byte-Level XCMP send/receive
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public byte[] SendBytes(byte[] data)
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

            //Console.WriteLine("XCMP: >>SNT>> " + Convert.ToHexString(toSend));

            _connection.Send(toSend);

            // start a timer so we don't hold infinitely
            var startTime = DateTime.UtcNow;

            while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(5))
            {
                byte[] fromRadio = _connection.Receive();

                int len = 0;

                len |= (fromRadio[0] << 8) & 0xFF;
                len |= fromRadio[1];

                //Console.WriteLine("XCMP: <<RCV<< " + Convert.ToHexString(fromRadio.Take(len + 2).ToArray()));

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

        /// <summary>
        /// Get the connected radio's serial number
        /// </summary>
        /// <returns></returns>
        public string GetSerial()
        {
            Console.WriteLine($"XCMP: getting radio serial number");

            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.SERIAL_NUMBER);

            return Encoding.UTF8.GetString(Send(msg).Data).TrimEnd('\0');
        }

        /// <summary>
        /// Get the connected radio's model number
        /// </summary>
        /// <returns></returns>
        public string GetModel()
        {
            Console.WriteLine($"XCMP: getting radio model number");

            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.MODEL_NUMBER);

            return Encoding.UTF8.GetString(Send(msg).Data).TrimEnd('\0');
        }

        /// <summary>
        /// Get Radio SW Version
        /// </summary>
        /// <param name="oper"></param>
        /// <returns></returns>
        public string GetVersion(VersionOperation oper)
        {
            Console.WriteLine($"XCMP: getting radio version for {Enum.GetName<VersionOperation>(oper)}");

            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.VERSION_INFO);
            msg.Data = new byte[] { (byte)oper };

            XcmpMessage resp = Send(msg);

            return Encoding.UTF8.GetString(resp.Data).TrimEnd('\0');
        }

        public byte[] GetStatus(StatusOperation oper)
        {
            Console.WriteLine($"XCMP: getting radio status {Enum.GetName<StatusOperation>(oper)}");

            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.RADIO_STATUS);
            msg.Data = new byte[] { (byte)oper };

            XcmpMessage resp = Send(msg);

            // Verify we got the same status back
            if ((StatusOperation)resp.Data[0] != oper)
                throw new Exception($"Did not receive expected status operation (got {resp.Data[0]:X} ({Enum.GetName((StatusOperation)resp.Data[0])}) but expected {(byte)oper:X} ({Enum.GetName(oper)}))");

            // Skip the first byte (the operation)
            return resp.Data.Skip(1).ToArray();
        }

        public MotorolaBand[] GetBands()
        {
            Console.WriteLine($"XCMP: getting radio bands");

            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.VERSION_INFO);
            msg.Data = new byte[] { (byte)VersionOperation.RFBand };

            XcmpMessage resp = Send(msg);

            List<MotorolaBand> bands = new List<MotorolaBand>();
            foreach(byte b in resp.Data) { bands.Add((MotorolaBand)b); }
            return bands.ToArray();
        }

        public void EnterServiceMode()
        {
            Console.WriteLine($"XCMP: entering service mode");

            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.ENTER_TEST_MODE);
            
            Send(msg);
        }

        public void ResetRadio()
        {
            Console.WriteLine($"XCMP: resetting radio");

            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.RADIO_RESET);

            Send(msg);
        }

        public void SetTXFrequency(int frequency, Bandwidth bandwidth, TxDeviation deviation)
        {
            Console.WriteLine($"XCMP: setting TX frequency to {frequency} (BW: {Enum.GetName(bandwidth)}, DEV: {Enum.GetName(deviation)})");

            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.TX_FREQUENCY);
            msg.Data = new byte[6];

            // First 4 bytes are frequency
            Array.Copy(FrequencyToBytes(frequency), 0, msg.Data, 0, 4);

            // Fifth byte is bandwidth
            msg.Data[4] = (byte)bandwidth;

            // Sixth byte is modulation
            msg.Data[5] = (byte)deviation;

            Send(msg);
        }

        public void SetRXFrequency(int frequency, Bandwidth bandwidth, RxModulation modulation)
        {
            Console.WriteLine($"XCMP: setting RX frequency to {frequency} (BW: {Enum.GetName(bandwidth)}, MOD: {Enum.GetName(modulation)})");

            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.RX_FREQUENCY);
            msg.Data = new byte[6];

            // First 4 bytes are frequency
            Array.Copy(FrequencyToBytes(frequency), 0, msg.Data, 0, 4);

            // Fifth byte is bandwidth
            msg.Data[4] = (byte)bandwidth;

            // Sixth byte is modulation
            msg.Data[5] = (byte)modulation;

            Send(msg);
        }

        public void Keyup(TxMicrophone microphone = TxMicrophone.ExternalMuted)
        {
            Console.WriteLine($"XCMP: keying radio");

            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.TRANSMIT);
            msg.Data = new byte[1] { (byte)microphone };

            Send(msg);
        }

        public void Dekey()
        {
            Console.WriteLine($"XCMP: dekeying radio");

            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.RECEIVE);
            msg.Data = new byte[1] { (byte)RxSpeaker.InternalMuted };

            Send(msg);
        }

        /// <summary>
        /// Send a softpot message and retrieve a softpot response
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public SoftpotMessage SendSoftpot(SoftpotMessage message)
        {
            // Send the softpot message and receive and standard XCMP message
            XcmpMessage resp = Send(message);
            // Convert the response to a softpot message by parsing the bytes
            SoftpotMessage sp_resp = new SoftpotMessage(resp.Bytes);
            // Verify that we got the correct type back
            if (sp_resp.Type != message.Type)
                throw new Exception($"Received different softpot type from what was sent! (Sent {message.Type} but got {sp_resp.Type})");
            // Return
            return sp_resp;
        }

        /// <summary>
        /// Get the current value of a softpot
        /// </summary>
        /// <param name="type">softpot type</param>
        /// <returns>The bytes representing the softpot value (variable length)</returns>
        public byte[] SoftpotGetValue(SoftpotType type)
        {
            Console.WriteLine($"XCMP: Getting softpot value for {Enum.GetName(type)}");

            SoftpotMessage msg = new SoftpotMessage(MsgType.REQUEST, SoftpotOperation.READ, type);

            return SendSoftpot(msg).Value;
        }

        /// <summary>
        /// Get the minimum value for a softpot
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public byte[] SoftpotGetMinimum(SoftpotType type)
        {
            Console.WriteLine($"XCMP: getting softpot minimum for {Enum.GetName(type)}");

            SoftpotMessage msg = new SoftpotMessage(MsgType.REQUEST, SoftpotOperation.READ_MIN, type);

            return SendSoftpot(msg).Value;
        }

        /// <summary>
        /// Get the minimum value for a softpot
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public byte[] SoftpotGetMaximum(SoftpotType type)
        {
            Console.WriteLine($"XCMP: getting softpot maximum for {Enum.GetName(type)}");

            SoftpotMessage msg = new SoftpotMessage(MsgType.REQUEST, SoftpotOperation.READ_MAX, type);

            return SendSoftpot(msg).Value;
        }

        /// <summary>
        /// Write a softpot value to radio
        /// </summary>
        /// <param name="type"></param>
        /// <param name="val"></param>
        /// <exception cref="InvalidDataException"></exception>
        public void SoftpotWrite(SoftpotType type, byte[] val)
        {
            Console.WriteLine($"XCMP: writing softpot {Enum.GetName(type)} -> {Convert.ToHexString(val)}");

            SoftpotMessage msg = new SoftpotMessage(MsgType.REQUEST, SoftpotOperation.WRITE, type);
            msg.Value = val;

            SendSoftpot(msg);
        }

        /// <summary>
        /// Temporarily update a softpot value (will not persist, make sure to write)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="val"></param>
        public void SoftpotUpdate(SoftpotType type, byte[] val)
        {
            Console.WriteLine($"XCMP: updating softpot {Enum.GetName(type)} -> {Convert.ToHexString(val)}");

            SoftpotMessage msg = new SoftpotMessage(MsgType.REQUEST, SoftpotOperation.UPDATE, type);
            msg.Value = val;

            SendSoftpot(msg);
        }

        /// <summary>
        /// Return all values for a softpot type as a list of byte arrays
        /// </summary>
        /// <param name="type"></param>
        /// <param name="byteLen"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<byte[]> SoftpotReadAll(SoftpotType type, int byteLen)
        {
            Console.Write($"XCMP: reading all softpot values for softpot {Enum.GetName(type)} ({byteLen} bytes each");

            SoftpotMessage msg = new SoftpotMessage(MsgType.REQUEST, SoftpotOperation.READ_ALL, type);

            SoftpotMessage resp = SendSoftpot(msg);

            // Validate
            if (resp.Value.Length % byteLen != 0)
                throw new Exception($"Softpot value array not an even multiple of byte length!");

            // Determine number of values in response
            int n_vals = (int)(resp.Value.Length / byteLen);

            // List
            List<byte[]> values = new List<byte[]>();
            
            // Iterate
            for (int i = 0; i < n_vals; i++)
            {
                byte[] value = resp.Value.Skip(i * byteLen).Take(byteLen).ToArray();
                values.Add(value);
            }

            return values;
        }

        /// <summary>
        /// Read all frequencies associated with a softpot type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int[] SoftpotReadAllFrequencies(SoftpotType type)
        {
            Console.WriteLine($"XCMP: reading all softpot frequencies for softpot {Enum.GetName(type)}");

            SoftpotMessage msg = new SoftpotMessage(MsgType.REQUEST, SoftpotOperation.READ_ALL_FREQ, type);

            SoftpotMessage resp = SendSoftpot(msg);

            // Parse the frequencies in the response (freqs are 4 byes each)
            int n_freqs = (resp.Length - 4) / 4;
            int[] freqs = new int[n_freqs];
            for (int i = 0; i < n_freqs; i++)
            {
                byte[] freq_bytes = resp.Value.Skip(i * 4).Take(4).ToArray();
                freqs[i] = (int)BytesToFrequency(freq_bytes);
                Console.WriteLine($"Parsing frequency {i + 1}/{n_freqs}: {Convert.ToHexString(freq_bytes)} -> {freqs[i]} Hz");
            }

            return freqs;
        }

        public virtual int[] GetTXPowerPoints()
        {
            // Implemented by derived classes
            throw new NotImplementedException();
        }

        public virtual void SetTransmitPower(TxPowerLevel power)
        {
            Console.WriteLine($"XCMP: setting TX power to {Enum.GetName(power)}");

            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.TX_POWER_LEVEL_INDEX);
            msg.Data = new byte[1] { (byte)power };

            Send(msg);
        }

        public void SetTransmitConfig(XCMPRadioTransmitOption option)
        {
            Console.WriteLine($"XCMP: setting TX config to {Enum.GetName(option)}");

            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.TRANSMIT_CONFIG);
            msg.Data = new byte[1] { (byte)option };

            Send(msg);
        }

        public void SetReceiveConfig(XCMPRadioReceiveOption option)
        {
            Console.WriteLine($"XCMP: setting RX config to {Enum.GetName(option)}");

            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.RECEIVE_CONFIG);
            msg.Data = new byte[1] { (byte)option };

            Send(msg);
        }

        /// <summary>
        /// Get the P25 RX BER
        /// </summary>
        /// <param name="nFrames">number of frames to average over for measurement</param>
        /// <returns></returns>
        public double GetP25BER(int nIntFrames)
        {
            Console.WriteLine($"XCMP: measuring P25 BER using {nIntFrames} frames of integration");

            // Configure the RX chain
            XcmpMessage msg = new XcmpMessage(MsgType.REQUEST, Opcode.RECEIVE_CONFIG);
            msg.Data = new byte[2]
            {
                (byte)RxBerTestPattern.P25_1011,
                (byte)RxModulation.C4FM
            };
            Send(msg);

            Thread.Sleep(500);

            // Setup for the test
            msg = new XcmpMessage(MsgType.REQUEST, Opcode.RX_BER_CONTROL);
            msg.Data = new byte[2]
            {
                (byte)RxBerTestMode.CONTINUOUS,
                (byte)nIntFrames
            };
            Send(msg);

            // Wait for the requested number of frames
            Thread.Sleep(800 * nIntFrames);

            // Request an RX BER report
            msg = new XcmpMessage(MsgType.REQUEST, Opcode.RX_BER_SYNC_REPORT);
            XcmpMessage resp = Send(msg);

            //System.Threading.Thread.Sleep(500);

            // Parse the response
            return CalculateP25BER(resp.Data, nIntFrames);
        }

        /// <summary>
        /// Parse an RX BER response byte array
        /// </summary>
        /// <param name="berBytes">the array of BER responses, must be a multiple of 5</param>
        /// <param name="nFrames">the number of total frames integrated per measurement</param>
        /// <returns></returns>
        private static double CalculateP25BER(byte[] berBytes, int nFrames)
        {
            // Ensure length is correct
            if (berBytes.Length % 5 != 0)
                throw new ArgumentException($"BER byte array must be a multiple of 5 (got length {berBytes.Length})");

            // Calculate number of BER frames
            int frames = berBytes.Length / 5;

            // Number of bits in a single P25 frame
            const int P25_FRAME_BITS = 3456;

            // Running total bit errors count
            int totalBitErrors = 0;
            // Total number of bits to count against
            int totalBits = 0;

            // Iterate over each report
            for (int i = 0; i < frames; i++)
            {
                // Get the frame bytes
                byte[] frame = berBytes.Skip(i * 5).Take(5).ToArray();
                // Extract frame number
                byte frame_n = frame[0];
                // Extract sync/nosync
                RxBerSyncStatus status = (RxBerSyncStatus)frame[1];
                // If no sync or lost sync, ignore
                if (status == RxBerSyncStatus.NO_SYNC || status == RxBerSyncStatus.LOST)
                    continue;
                // Add bit errors to running total
                totalBitErrors += (int)BitConverter.ToUInt32(frame.Skip(2).Take(3).ToArray());
                // The total number of bits for this report is the number of frames integrated plus the frame bit count
                totalBits += (P25_FRAME_BITS * nFrames);
            }
            // Return the percentage of bit errors
            return (totalBitErrors / totalBits);
        }

        /// <summary>
        /// Retrieve the softpot parameters for a softpot from the radio
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public SoftpotParams SoftpotGetParams(SoftpotType type)
        {
            // New struct
            SoftpotParams p = new SoftpotParams();

            // Read frequencies
            p.Frequencies = SoftpotReadAllFrequencies(type);

            // Get min/max/byte length
            byte[] min = SoftpotGetMinimum(type);
            p.Min = SoftpotBytesToValue(min);
            p.Max = SoftpotBytesToValue(SoftpotGetMaximum(type));
            p.ByteLength = min.Length;

            // Get initial values
            List<byte[]> vals = SoftpotReadAll(type, p.ByteLength);

            // Validate
            if (vals.Count != p.Frequencies.Length)
                throw new Exception($"Did not get expected number of softpot values for frequencies (Got {vals.Count}, Expected {p.Frequencies.Length}");

            // Add to list
            p.Values = new int[vals.Count];
            for (int i = 0; i < vals.Count; i++)
            {
                p.Values[i] = SoftpotBytesToValue(vals[i]);
            }

            // Return
            return p;
        }

    }
}
