namespace NAPS2.Wia
{
    /// <summary>
    /// Error code constants.
    /// <a href="https://docs.microsoft.com/en-us/windows/desktop/wia/-wia-error-codes">WIA Error Codes</a>
    /// <a href="https://docs.microsoft.com/en-us/windows/win32/adsi/generic-com-error-codes">Generic COM Error Codes.</a>
    /// </summary>
    public static class WiaErrorCodes
    {
        public const uint BUSY = 0x80210006;
        public const uint COVER_OPEN = 0x80210016;
        public const uint DEVICE_COMMUNICATION = 0x8021000A;
        public const uint DEVICE_LOCKED = 0x8021000D;
        public const uint EXCEPTION_IN_DRIVER = 0x8021000E;
        public const uint GENERAL_ERROR = 0x80210001;
        public const uint INCORRECT_HARDWARE_SETTING = 0x8021000C;
        public const uint INVALID_COMMAND = 0x8021000B;
        public const uint INVALID_DRIVER_RESPONSE = 0x8021000F;
        public const uint ITEM_DELETED = 0x80210009;
        public const uint LAMP_OFF = 0x80210017;
        public const uint MAXIMUM_PRINTER_ENDORSER_COUNTER = 0x80210021;
        public const uint MULTI_FEED = 0x80210020;
        public const uint OFFLINE = 0x80210005;
        public const uint PAPER_EMPTY = 0x80210003;
        public const uint PAPER_JAM = 0x80210002;
        public const uint PAPER_PROBLEM = 0x80210004;
        public const uint WARMING_UP = 0x80210007;
        public const uint USER_INTERVENTION = 0x80210008;

        public const uint NO_DEVICE_AVAILABLE = 0x80210015;

        /// <summary>Operation aborted. </summary>
        public const uint COM_ABORT = 0x80004004;

        /// <summary>Unspecified error. Sometimes reported when there is a paper jam.</summary>
        public const uint COM_FAIL = 0x80004005;

        /// <summary>Interface not supported. </summary>
        public const uint COM_NOINTERFACE = 0x80004002;

        /// <summary>Not implemented. </summary>
        public const uint COM_NOTIMPL = 0x80004001;

        /// <summary>Invalid pointer. </summary>
        public const uint COM_POINTER = 0x80004003;

        /// <summary>Catastrophic failure.</summary>
        public const uint COM_UNEXPECTED = 0x8000FFFF;
    }
}
