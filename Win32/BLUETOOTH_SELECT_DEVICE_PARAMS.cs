using RemoteController.Bluetooth;
using System;
using System.Runtime.InteropServices;

namespace RemoteController.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BLUETOOTH_SELECT_DEVICE_PARAMS
    {
        int dwSize;

        int cNumOfClasses;
        IntPtr prgClassOfDevices;
        [MarshalAs(UnmanagedType.LPWStr)]
        string pszInfo;
        public IntPtr hwndParent;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fForceAuthentication;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fShowAuthenticated;
        [MarshalAs(UnmanagedType.Bool)]
        bool fShowRemembered;
        [MarshalAs(UnmanagedType.Bool)]
        bool fShowUnknown;
        [MarshalAs(UnmanagedType.Bool)]
        bool fAddNewDeviceWizard;
        [MarshalAs(UnmanagedType.Bool)]
        bool fSkipServicesPage;
        NativeMethods.PFN_DEVICE_CALLBACK pfnDeviceCallback;
        IntPtr pvParam;
        public int cNumDevices;
        public IntPtr pDevices;

        public void Reset()
        {
            dwSize = Marshal.SizeOf(this);
            cNumOfClasses = 0;
            prgClassOfDevices = IntPtr.Zero;
            pszInfo = null;
            hwndParent = IntPtr.Zero;
            fForceAuthentication = false;
            fShowAuthenticated = true;
            fShowRemembered = true;
            fShowUnknown = true;
            fAddNewDeviceWizard = false;
            fSkipServicesPage = false;
            pfnDeviceCallback = null;
            pvParam = IntPtr.Zero;
            cNumDevices = 0;
            pDevices = IntPtr.Zero;
        }

        public void SetClassOfDevices(ClassOfDevice[] classOfDevices)
        {
            if (prgClassOfDevices != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(prgClassOfDevices);
                prgClassOfDevices = IntPtr.Zero;
            }

            if (classOfDevices.Length == 0)
            {
                cNumOfClasses = 0;
                prgClassOfDevices = IntPtr.Zero;
            }
            else
            {
                cNumOfClasses = classOfDevices.Length;
                prgClassOfDevices = Marshal.AllocHGlobal(8 * classOfDevices.Length);
                for (int i = 0; i < cNumOfClasses; i++)
                {
                    Marshal.WriteInt32(prgClassOfDevices, i * 8, (int)classOfDevices[i].Value);

                    //prgClassOfDevices[i] = new BLUETOOTH_COD_PAIRS();
                    //prgClassOfDevices[i].ulCODMask = classOfDevices[i].Value;
                    //prgClassOfDevices[i].pcszDescription = classOfDevices[i].ToString();
                }
            }
        }

        public void SetFilter(Predicate<BluetoothDeviceInfo> filterFn,
            NativeMethods.PFN_DEVICE_CALLBACK msftFilterFn)
        {
            pfnDeviceCallback = msftFilterFn;
        }

        public BLUETOOTH_DEVICE_INFO[] Devices
        {
            get
            {
                if (cNumDevices > 0)
                {
                    BLUETOOTH_DEVICE_INFO[] devs = new BLUETOOTH_DEVICE_INFO[cNumDevices];

                    for (int idevice = 0; idevice < cNumDevices; idevice++)
                    {
                        devs[idevice] = (BLUETOOTH_DEVICE_INFO)Marshal.PtrToStructure(new IntPtr(pDevices.ToInt64() + (idevice * Marshal.SizeOf(typeof(BLUETOOTH_DEVICE_INFO)))), typeof(BLUETOOTH_DEVICE_INFO));
                    }

                    return devs;
                }

                return new BLUETOOTH_DEVICE_INFO[0];
            }
        }

        public BLUETOOTH_DEVICE_INFO Device
        {
            get
            {
                if (cNumDevices > 0)
                {
                    BLUETOOTH_DEVICE_INFO device = (BLUETOOTH_DEVICE_INFO)Marshal.PtrToStructure(pDevices, typeof(BLUETOOTH_DEVICE_INFO));
                    return device;
                }
                return new BLUETOOTH_DEVICE_INFO();
            }
        }
    }
}