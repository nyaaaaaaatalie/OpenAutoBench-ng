namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    /// <summary>
    /// XCMP Message Types (the first byte in the message after the length)
    /// </summary>
    public enum MsgType : byte
    {
        REQUEST = 0x0,
        RESPONSE = 0x8
    }

    /// <summary>
    /// XCMP Opcodes
    /// </summary>
    public enum Opcode : UInt16
    {
        SOFTPOT = 0x01,
        TRANSMIT_CONFIG = 0x02,
        RECEIVE_CONFIG = 0x03,
        TRANSMIT = 0x04,
        RECEIVE = 0x05,
        TX_POWER_LEVEL_INDEX = 0x06,
        PREEMPH_DEEMPH = 0x07,
        SQUELCH_CONTROL = 0x08,
        VOLUME_CONTROL = 0x09,
        RX_FREQUENCY = 0x0A,
        TX_FREQUENCY = 0x0B,
        ENTER_TEST_MODE = 0x0C,
        RADIO_RESET = 0x0D,
        RADIO_STATUS = 0x0E,
        VERSION_INFO = 0x0F,
        MODEL_NUMBER = 0x10,
        SERIAL_NUMBER = 0x11,
        READ_UUID = 0x12,
        ENCRYPTION_ALGID = 0x13,
        DATA_XFER_TO_ENC_MODULE = 0x14,
        ENC_MODULE_BOOT_MODE = 0x15,
        RX_BER_CONTROL = 0x16,
        RX_BER_SYNC_REPORT = 0x17,

        AFC_CONTROL = 0x1C,
        ATTEN_CONTROL = 0x1E,
        IQME_UPDTE = 0x29,

        ISH_READ = 0x100,

        ENTER_BOOT_MODE = 0x200,
    }

    /// <summary>
    /// XCMP Result Codes
    /// </summary>
    public enum Result : byte
    {
        SUCCESS = 0x00,
        FAILURE = 0x01,
        INCORRECT_MODE = 0x02,
        OPCODE_NOT_SUPPORTED = 0x03,
        INVALID_PARAMETER = 0x04,
        REPLY_TOO_BIG = 0x05,
        SECURITY_LOCKED = 0x06,

        FACTORY_INFO_MAX_TYPES = 0x08,

        SOFTPOT_OP_NOT_SUPPORTED = 0x40,
        SOFTPOT_TYPE_NOT_SUPPORTED = 0x41,
        SOFTPOT_VALUE_OUT_OF_RANGE = 0x42,

        FLASH_WRITE_FAILURE = 0x80,
        ISH_ITEM_NOT_FOUND = 0x81,
        ISH_OFFSET_OUT_OF_RANGE = 0x82,
        ISH_INSUFFICIENT_SPACE = 0x83,
        ISH_PARTITION_NOT_EXIST = 0x84,
        ISH_PARTITION_READ_ONLY = 0x85,
        ISH_REORG_NEEDED = 0x86,
    }

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
        READ = 0x00,
        WRITE = 0x01,
        UPDATE = 0x02,
        READ_ALL = 0x03,
        WRITE_ALL = 0x04,
        AUTOTUNE = 0x05,
        READ_MIN = 0x06,
        READ_MAX = 0x07,
        READ_ALL_FREQ = 0x08,
    }

    public enum SoftpotBEROperation : byte
    {
        BER_DISABLE = 0x00,
        BER_ENABLE_SINGLE = 0x01,
        BER_ENABLE_CONTINUOUS = 0x02
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

    public enum TransmitConfig : byte
    {
        TxPaBiasTuneMode = 1,
        HpdTxPaBiasTuneMode = 2,
        TxAnalogCsq = 16,
        AnalogModulationBalanceLowFrequencyTone = 17,
        AnalogModulationBalanceHighFrequencyTone = 18,
        TxAnalogTpl = 19,
        TxAnalogDpl = 20,
        TxDigitalVoice = 32,
        TxDtpStandardToneTestPattern = 33,
        DtpApcoRaw = 34,
        DtpStandardTxSymbolRatePattern = 35,
        DtpStandardTxLowDeviationPattern = 36,
        DtpStandardTxTestPattern = 37,
        DtpStandardTxC4fmModulationFidelity = 38,
        DtpApcoSilentFrame = 39,
        DtpDm0 = 40,
        DtpDm1 = 41,
        F21033TestPatternPhysical = 80,
        F21033TestPatternLmac = 81,
        F2MbtLcHdrTestPattern = 82,
        F2DigitalVoice = 83,
        F2StandardTxTestPattern = 84,
        F2StandardTxSymbolRatePatternHighDeviation = 85,
        F2StandardTxLowDeviationPattern = 86,
        F2StandardTxC4fmModulationFidelityPattern = 87,
        Phase2Digital1031TxTestPattern = 112,
        Phase2StandardTxHighDeviation = 113,
        Phase2StandardTxLowDeviation = 114,
        Phase2Digital1031TxTestPatternLmac = 117,
        Symmetrical1031TestPattern = 119
    }

    public enum ReceiveConfig : byte
    {
        RxAnalogCsq = 16,
        RxAnalogTpl = 19,
        RxAnalogDpl = 20,
        RxDigitalVoice = 32,
        RxDtpStandardToneTestPattern = 33,
        DtpStandardInterferenceTestPattern = 37,
        RxHpdDtpO153Qpsk = 42,
        RxHpdDtpO15316qam = 43,
        RxHpdDtpO15364qam = 44,
        F2TestPattern = 80,
        DigitalTestPattern1031 = 112,
        PhaseIiDchTestPattern = 119,
        UndefinedRxConfigSource = 255
    }

    public enum Bandwidth : byte
    {
        BW_6_25kHz = 0x16,
        BW_12_5kHz = 0x32,
        BW_25kHz = 0x64
    }

    public enum TxDeviation : byte
    {
        Default = 0x00,
        NoModulation = 0x01
    }

    public enum TxPowerLevel : byte
    {
        High = 0x00,
        Mid = 0x01,
        Low = 0x02,
        LowNew = 0x03,
        Undefined = 0xFF
    }

    public enum TxMicrophone : byte
    {
        InternalUnmuted = 0x00,
        InternalMuted = 0x01,
        ExternalUnmuted = 0x02,
        ExternalMuted = 0x03
    }

    public enum RxModulation : byte
    {
        C4FM = 0x00,
        CQPSK = 0x01,
        WidePulse = 0x02,
        Undefined = 0xFF
    }

    public enum RxSpeaker : byte
    {
        ExternalUnmuted = 0x00,
        ExternalMuted = 0x01,
        ExternalActiveRX = 0x02,
        InternalUnmuted = 0x10,
        InternalMuted = 0x11,
        InternalActiveRX = 0x12
    }

    public enum RxBerTestPattern : byte
    {
        DIGITAL_VOICE = 0x20,
        P25_1011 = 0x21,
        STD_HIGH_DEV = 0x23,
        STD_LOW_DEV = 0x24,
        STD_CCITT_V52 = 0x25,
        STD_MOD_FIDELITY = 0x26,
        APCO_SILENT =0x27
    }

    public enum RxBerTestMode : byte
    {
        DISABLED = 0x00,
        SINGLE = 0x01,
        CONTINUOUS = 0x02
    }

    public enum RxBerSyncStatus : byte
    {
        SYNCED = 0x00,
        LOST = 0x01,
        NO_SYNC = 0x02
    }
}
