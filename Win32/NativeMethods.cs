using System;
using System.Runtime.InteropServices;

namespace RemoteController.Win32
{

    [return: MarshalAs(UnmanagedType.Bool)]
    internal delegate bool BluetoothAuthenticationCallbackEx(IntPtr pvParam, ref BLUETOOTH_AUTHENTICATION_CALLBACK_PARAMS pAuthCallbackParams);

    [return: MarshalAs(UnmanagedType.Bool)] // Does this have any effect?
    internal delegate bool BluetoothAuthenticationCallback(IntPtr pvParam, ref BLUETOOTH_DEVICE_INFO bdi);


    public static partial class NativeMethods
    {
        internal const string Tab = "    ";

        // from winuser.h
        private const int GWL_STYLE = -16,
                          WS_MAXIMIZEBOX = 0x10000,
                          WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        public extern static int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public extern static int SetWindowLong(IntPtr hwnd, int index, int value);

        internal static void HideMinimizeAndMaximizeButtons(System.Windows.Window window)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
        }
        private const string bthpropsDll = "bthprops.cpl";
        private const string irpropsDll = "Irprops.cpl";
        private const string wsDll = "ws2_32.dll";

        [DllImport(User32Lib)]
        internal static extern IntPtr GetActiveWindow();

        // Requires Vista SP2 or later
        [DllImport(bthpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int BluetoothRegisterForAuthenticationEx(ref BLUETOOTH_DEVICE_INFO pbtdi, out IntPtr phRegHandle, BluetoothAuthenticationCallbackEx pfnCallback, IntPtr pvParam);

        [DllImport(bthpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothUnregisterAuthentication(IntPtr hRegHandle);

        [DllImport(bthpropsDll, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern int BluetoothSendAuthenticationResponseEx(IntPtr hRadio, ref BLUETOOTH_AUTHENTICATE_RESPONSE__PIN_INFO pauthResponse);

        [DllImport(bthpropsDll, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern int BluetoothSendAuthenticationResponseEx(IntPtr hRadio, ref BLUETOOTH_AUTHENTICATE_RESPONSE__OOB_DATA_INFO pauthResponse);

        [DllImport(bthpropsDll, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern int BluetoothSendAuthenticationResponseEx(IntPtr hRadio, ref BLUETOOTH_AUTHENTICATE_RESPONSE__NUMERIC_COMPARISON_PASSKEY_INFO pauthResponse);

        [DllImport(bthpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int BluetoothGetDeviceInfo(IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi);

        [DllImport(bthpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int BluetoothAuthenticateDeviceEx(IntPtr hwndParentIn, IntPtr hRadioIn, ref BLUETOOTH_DEVICE_INFO pbtdiInout, byte[] pbtOobData, BluetoothAuthenticationRequirements authenticationRequirement);

        /// <summary>
        /// The BluetoothAuthenticateDevice function sends an authentication request to a remote Bluetooth device.
        /// </summary>
        /// <param name="hwndParent">The window to parent the authentication wizard.
        /// If NULL, the wizard will be parented off the desktop.</param>
        /// <param name="hRadio">A valid local radio handle, or NULL. If NULL, authentication is attempted on all local radios; if any radio succeeds, the function call succeeds.</param>
        /// <param name="pbtdi">A structure of type BLUETOOTH_DEVICE_INFO that contains the record of the Bluetooth device to be authenticated.</param>
        /// <param name="pszPasskey">A Personal Identification Number (PIN) to be used for device authentication. If set to NULL, the user interface is displayed and and the user must follow the authentication process provided in the user interface. If pszPasskey is not NULL, no user interface is displayed. If the passkey is not NULL, it must be a NULL-terminated string. For more information, see the Remarks section.</param>
        /// <param name="ulPasskeyLength">The size, in characters, of pszPasskey.
        /// The size of pszPasskey must be less than or equal to BLUETOOTH_MAX_PASSKEY_SIZE.</param>
        /// <returns></returns>
        [DllImport(irpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int BluetoothAuthenticateDevice(IntPtr hwndParent, IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi, string pszPasskey, int ulPasskeyLength);

        [DllImport(irpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern uint BluetoothRegisterForAuthentication(
            ref BLUETOOTH_DEVICE_INFO pbtdi,
            out BluetoothAuthenticationRegistrationHandle phRegHandle,
            BluetoothAuthenticationCallback pfnCallback,
            IntPtr pvParam);

        //Requires Vista SP2 or later
        [DllImport(bthpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern uint BluetoothRegisterForAuthenticationEx(
            ref BLUETOOTH_DEVICE_INFO pbtdi,
            out BluetoothAuthenticationRegistrationHandle phRegHandle,
            BluetoothAuthenticationCallbackEx pfnCallback,
            IntPtr pvParam);

        [DllImport(irpropsDll, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern int BluetoothSendAuthenticationResponse(IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi, string pszPasskey);

        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern int BluetoothRemoveDevice(ref ulong pAddress);

        // Radio
        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern IntPtr BluetoothFindFirstRadio(ref BLUETOOTH_FIND_RADIO_PARAMS pbtfrp, out IntPtr phRadio);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothFindNextRadio(IntPtr hFind, out IntPtr phRadio);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothFindRadioClose(IntPtr hFind);

        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern int BluetoothGetRadioInfo(IntPtr hRadio, ref BLUETOOTH_RADIO_INFO pRadioInfo);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothIsConnectable(IntPtr hRadio);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothIsDiscoverable(IntPtr hRadio);


        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothEnableDiscovery(IntPtr hRadio, bool fEnabled);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothEnableIncomingConnections(IntPtr hRadio, bool fEnabled);

        // Discovery
        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern IntPtr BluetoothFindFirstDevice(ref BLUETOOTH_DEVICE_SEARCH_PARAMS pbtsp, ref BLUETOOTH_DEVICE_INFO pbtdi);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothFindNextDevice(IntPtr hFind, ref BLUETOOTH_DEVICE_INFO pbtdi);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothFindDeviceClose(IntPtr hFind);

        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern int BluetoothEnumerateInstalledServices(IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi, ref int pcServices, byte[] pGuidServices);
        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern int BluetoothSetServiceState(IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi, ref Guid pGuidService, uint dwServiceFlags);

        [DllImport(Kernal32Lib, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr handle);

        //SetService
        [DllImport(wsDll, EntryPoint = "WSASetService", SetLastError = true)]
        internal static extern int WSASetService(ref WSAQUERYSET lpqsRegInfo, WSAESETSERVICEOP essoperation, int dwControlFlags);

        // Picker
        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothSelectDevices(ref BLUETOOTH_SELECT_DEVICE_PARAMS pbtsdp);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothSelectDevicesFree(ref BLUETOOTH_SELECT_DEVICE_PARAMS pbtsdp);

        internal delegate bool PFN_DEVICE_CALLBACK(IntPtr pvParam, ref BLUETOOTH_DEVICE_INFO pDevice);

    }

    /// <summary>
    /// The BLUETOOTH_AUTHENTICATION_CALLBACK_PARAMS structure contains specific configuration information about the Bluetooth device responding to an authentication request.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct BLUETOOTH_AUTHENTICATION_CALLBACK_PARAMS
    {
        /// <summary>
        /// A BLUETOOTH_DEVICE_INFO structure that contains information about a Bluetooth device.
        /// </summary>
        internal BLUETOOTH_DEVICE_INFO deviceInfo;

        /// <summary>
        /// A BLUETOOTH_AUTHENTICATION_METHOD enumeration that defines the authentication method utilized by the Bluetooth device.
        /// </summary>
        internal BluetoothAuthenticationMethod authenticationMethod;

        /// <summary>
        /// A BLUETOOTH_IO_CAPABILITY enumeration that defines the input/output capabilities of the Bluetooth device.
        /// </summary>
        internal BluetoothIoCapability ioCapability;

        /// <summary>
        /// A AUTHENTICATION_REQUIREMENTS specifies the 'Man in the Middle' protection required for authentication.
        /// </summary>
        internal BluetoothAuthenticationRequirements authenticationRequirements;

        //union{
        //    ULONG   Numeric_Value;
        //    ULONG   Passkey;
        //};

        /// <summary>
        /// A ULONG value used for Numeric Comparison authentication.
        /// or
        /// A ULONG value used as the passkey used for authentication.
        /// </summary>
        internal uint Numeric_Value_Passkey;
    }
}
