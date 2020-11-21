﻿using RemoteController.Bluetooth.Core;
using System;
using System.Runtime.InteropServices;

namespace RemoteController.Bluetooth.Win32
{
    internal sealed class Win32BluetoothAuthentication
    {
        string _pin;
        IntPtr _handle = IntPtr.Zero;
        BluetoothAuthenticationCallbackEx _callback;

        public ulong Address { get; set; }

        public Win32BluetoothAuthentication(BluetoothAddress address, string pin)
        {
            Address = address;
            _pin = pin;
            BLUETOOTH_DEVICE_INFO device = BLUETOOTH_DEVICE_INFO.Create();
            device.Address = address;
            NativeMethods.BluetoothGetDeviceInfo(IntPtr.Zero, ref device);
            _callback = new BluetoothAuthenticationCallbackEx(Callback);

            int result = NativeMethods.BluetoothRegisterForAuthenticationEx(ref device, out _handle, _callback, IntPtr.Zero);
        }

        private bool Callback(IntPtr pvParam, ref BLUETOOTH_AUTHENTICATION_CALLBACK_PARAMS pAuthCallbackParams)
        {
            switch (pAuthCallbackParams.authenticationMethod)
            {
                case BluetoothAuthenticationMethod.Passkey:
                case BluetoothAuthenticationMethod.NumericComparison:
                    BLUETOOTH_AUTHENTICATE_RESPONSE__NUMERIC_COMPARISON_PASSKEY_INFO nresponse = new BLUETOOTH_AUTHENTICATE_RESPONSE__NUMERIC_COMPARISON_PASSKEY_INFO
                    {
                        authMethod = pAuthCallbackParams.authenticationMethod,
                        bthAddressRemote = pAuthCallbackParams.deviceInfo.Address,
                        numericComp_passkey = pAuthCallbackParams.Numeric_Value_Passkey
                    };
                    return NativeMethods.BluetoothSendAuthenticationResponseEx(IntPtr.Zero, ref nresponse) == 0;

                case BluetoothAuthenticationMethod.Legacy:
                    BLUETOOTH_AUTHENTICATE_RESPONSE__PIN_INFO response = new BLUETOOTH_AUTHENTICATE_RESPONSE__PIN_INFO();
                    response.authMethod = pAuthCallbackParams.authenticationMethod;
                    response.bthAddressRemote = pAuthCallbackParams.deviceInfo.Address;
                    response.pinInfo.pin = new byte[16];
                    System.Text.Encoding.ASCII.GetBytes(_pin).CopyTo(response.pinInfo.pin, 0);
                    response.pinInfo.pinLength = _pin.Length;

                    return NativeMethods.BluetoothSendAuthenticationResponseEx(IntPtr.Zero, ref response) == 0;
            }

            return false;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                NativeMethods.BluetoothUnregisterAuthentication(_handle);

                disposedValue = true;
            }
        }

        ~Win32BluetoothAuthentication()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    [StructLayout(LayoutKind.Sequential, Size = 52)]
    internal struct BLUETOOTH_AUTHENTICATE_RESPONSE__PIN_INFO // see above
    {
        internal ulong bthAddressRemote;
        internal BluetoothAuthenticationMethod authMethod;
        internal BLUETOOTH_PIN_INFO pinInfo;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        private byte[] _padding;
        internal byte negativeResponse;
    }

    [StructLayout(LayoutKind.Sequential, Size = 52)]
    internal struct BLUETOOTH_AUTHENTICATE_RESPONSE__OOB_DATA_INFO // see above
    {
        internal ulong bthAddressRemote;
        internal BluetoothAuthenticationMethod authMethod;
        internal BLUETOOTH_OOB_DATA_INFO oobInfo;
        internal byte negativeResponse;
    }

    [StructLayout(LayoutKind.Sequential, Size = 52)]
    internal struct BLUETOOTH_AUTHENTICATE_RESPONSE__NUMERIC_COMPARISON_PASSKEY_INFO // see above
    {
        internal ulong bthAddressRemote;
        internal BluetoothAuthenticationMethod authMethod;
        internal uint numericComp_passkey;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        private byte[] _padding;

        internal byte negativeResponse;
    }

    /// <summary>
    /// The BLUETOOTH_PIN_INFO structure contains information used for authentication via PIN.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 20)]
    internal struct BLUETOOTH_PIN_INFO
    {
        public const int BTH_MAX_PIN_SIZE = 16;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = BTH_MAX_PIN_SIZE)]
        internal byte[] pin;
        internal int pinLength;
    }

    /// <summary>
    /// The BLUETOOTH_OOB_DATA_INFO structure contains data used to authenticate prior to establishing an Out-of-Band device pairing.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    internal struct BLUETOOTH_OOB_DATA_INFO
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal byte[] C;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal byte[] R;
    }
}
