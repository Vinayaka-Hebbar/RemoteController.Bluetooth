namespace RemoteController.Win32
{
    internal enum Win32Error
    {
        ERROR_SUCCESS = 0,
        ERROR_FILE_NOT_FOUND = 2,
        ERROR_INSUFFICIENT_BUFFER = 0x7A, //122
        ERROR_SERVICE_DOES_NOT_EXIST = 1060,
        ERROR_DEVICE_NOT_CONNECTED = 1167,
        ERROR_NOT_FOUND = 1168,
        ERROR_NOT_AUTHENTICATED = 1244,
    }
}
