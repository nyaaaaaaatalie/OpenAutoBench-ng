namespace OpenAutoBench_ng.OpenAutoBench
{
    public class Settings
    {
        public int Version { get; set; }

        public enum InstrumentTypeEnum
        {
            //Generic = 0,            // will just issue SCPI and hope for the best; analog only
            HP_8900 = 1,            // HP 8935, 8920, possibly others
            //Aeroflex_3920 = 2,      // Aeroflex 3920
            IFR_2975 = 3,           // Aeroflex/IFR 2975
            R2670 = 4,              // General Dynamics/Motorola R2600
            Astronics_R8000 = 5,    // Freedom/Astronics R8100
            Viavi_8800SX = 6,       // Aeroflex/Viavi 8800SX
        }

        public InstrumentTypeEnum InstrumentType { get; set; }

        public enum InstrumentConnectionTypeEnum
        {
            Serial = 0,
            //USB = 1,
            IP = 2,
            VISA = 3,
        }

        public enum SerialNewlineType
        {
            LF = 0,
            CR = 1,
            CRLF = 2,
        }

        public enum SerialNewlineType
        {
            LF = 0,
            CR = 1,
            CRLF = 2,
        }

        public InstrumentConnectionTypeEnum InstrumentConnectionType { get; set; }

        /// <summary>
        /// Bool to store if the instrument in question is on a GPIB bus.
        /// Pretty much just adds some specific commands for selecting the address
        /// </summary>
        public bool SerialIsGPIB { get; set; }

        /// <summary>
        /// The address of the instrument on the GPIB bus.
        /// </summary>
        public int SerialGPIBAddress { get; set;}

        /// <summary>
        /// The serial port the instrument (or interface) is connected at.
        /// </summary>
        public string SerialPort { get; set; }
        
        public int SerialBaudrate { get; set; }

        /// <summary>
        /// What character(s) to use for newline
        /// </summary>
        public SerialNewlineType SerialNewline { get; set; }

        /// <summary>
        /// Whether to force DTR to true
        /// </summary>
        public bool SerialDTR { get; set; }

        public string IPAddress { get; set; }

        public int IPPort { get; set; }

        public string VISAResourceName { get; set; }

        public int[] MotoTrboKeys { get; set; }

        public int MotoTrboDelta { get; set; }

        /// <summary>
        /// Danger mode! Enables settings that could be dangerous or are for development purposes.
        /// </summary>
        public bool DangerMode { get; set; }

        public bool EnableExtendedFreqTest { get; set; }

        /// <summary>
        /// Disables model number checking in tests, will force all tests to be run regardless if the radio supports it.
        /// </summary>
        public bool DisableModelChecking { get; set; }

        public Settings()
        {
            Version = 1;
            InstrumentType = InstrumentTypeEnum.HP_8900;
            InstrumentConnectionType = InstrumentConnectionTypeEnum.Serial;
            SerialPort = "";
            SerialBaudrate = 115200;
            SerialIsGPIB = false;
            SerialGPIBAddress = 0;
            IPAddress = "";
            IPPort = 0;
            VISAResourceName = "";
            MotoTrboKeys = new int[] { 0, 0, 0, 0};
            MotoTrboDelta = 0;
            DangerMode = false;
            EnableExtendedFreqTest = false;
            DisableModelChecking = false;
        }
    }
}
