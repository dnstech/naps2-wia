using System;
using System.Runtime.Serialization;

namespace NAPS2.Wia
{
    [Serializable]
    public class WiaException : Exception
    {
        public static void Check(uint hresult)
        {
            if (hresult != 0)
            {
                throw new WiaException(hresult);
            }
        }

        protected WiaException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public WiaException(uint errorCode) : base(MessageForErrorCode(errorCode))
        {
            ErrorCode = errorCode;
        }

        public uint ErrorCode { get; set; }

        public static string MessageForErrorCode(uint errorCode)
        {
            switch (errorCode)
            {
                case WiaErrorCodes.PAPER_EMPTY:
                    return "Paper empty";
                case WiaErrorCodes.NO_DEVICE_AVAILABLE:
                    return "No device available";
                case WiaErrorCodes.OFFLINE:
                    return "The device is offline";
                case WiaErrorCodes.PAPER_JAM:
                    return "Paper jam";
                case WiaErrorCodes.BUSY:
                    return "Device is busy";
                case WiaErrorCodes.COVER_OPEN:
                    return "Paper cover is open";
                case WiaErrorCodes.COMMUNICATION:
                    return "Device communication error";
                case WiaErrorCodes.LOCKED:
                    return "Device is locked";
                case WiaErrorCodes.INCORRECT_SETTING:
                    return "Incorrect setting";
                case WiaErrorCodes.LAMP_OFF:
                    return "Lamp is off";
                case WiaErrorCodes.WARMING_UP:
                    return "Device is warming up";
            }

            return $"WIA error code {errorCode:X}";
        }
    }
}
