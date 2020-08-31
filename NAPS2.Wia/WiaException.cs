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
                    return "There are no documents in the document feeder.";
                case WiaErrorCodes.NO_DEVICE_AVAILABLE:
                    return "No scanner device was found. Make sure the device is online, connected to the PC, and has the correct driver installed on the PC.";
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
                    return "The device is warming up.";
                case WiaErrorCodes.PAPER_PROBLEM:
                    return "An unspecified problem occurred with the scanner's document feeder.";
                case WiaErrorCodes.USER_INTERVENTION:
                    return "There is a problem with the WIA device. Make sure that the device is turned on, online, and any cables are properly connected.";
            }

            return $"WIA error code {errorCode:X}";
        }
    }
}
