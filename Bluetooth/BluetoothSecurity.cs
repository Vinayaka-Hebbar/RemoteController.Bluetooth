using RemoteController.Win32;
using System;
using System.Diagnostics;

namespace RemoteController.Bluetooth
{
    public sealed class BluetoothSecurity
    {
        private readonly System.Collections.Hashtable authenticators = new System.Collections.Hashtable();

        public static bool PairRequest(BluetoothAddress device, string pin)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }
            if (device.IsNull)
            {
                throw new ArgumentNullException("device", "A non-blank address must be specified.");
            }
            //use other constructor to ensure struct size is set
            BLUETOOTH_DEVICE_INFO bdi = new BLUETOOTH_DEVICE_INFO(device);

            //string length, but allow for null pins for UI
            int length = 0;
            if (pin != null)
            {
                length = pin.Length;
            }
            int result = NativeMethods.BluetoothAuthenticateDevice(IntPtr.Zero, IntPtr.Zero, ref bdi, pin, length);

            if (result != 0)
            {
                //determine error cause from "result"...
                // ERROR_INVALID_PARAMETER      87
                // WAIT_TIMEOUT                258
                // ERROR_NOT_AUTHENTICATED    1244
                Debug.WriteLine("PairRequest/BAD failed with: " + result);
                return false;
            }
            return true;
        }

        public static bool RemoveDevice(BluetoothAddress device)
        {
            if (device.IsNull)
                throw new ArgumentNullException("device");
            ulong addr = device;
            return NativeMethods.BluetoothRemoveDevice(ref addr) == 0;
        }


        #region Set PIN
        /// <summary>
        /// This function stores the personal identification number (PIN) for the Bluetooth device.
        /// </summary>
        /// <param name="device">Address of remote device.</param>
        /// <param name="pin">Pin, alphanumeric string of between 1 and 16 ASCII characters.</param>
        /// <remarks><para>On Windows CE platforms this calls <c>BthSetPIN</c>,
        /// its MSDN remarks say:
        /// </para>
        /// <para>&#x201C;Stores the pin for the Bluetooth device identified in pba.
        /// The active connection to the device is not necessary, nor is the presence
        /// of the Bluetooth controller. The PIN is persisted in the registry until
        /// BthRevokePIN is called.
        /// </para>
        /// <para>&#x201C;While the PIN is stored, it is supplied automatically
        /// after the PIN request is issued by the authentication mechanism, so the
        /// user will not be prompted for it. Typically, for UI-based devices, you
        /// would set the PIN for the duration of authentication, and then revoke
        /// it after authentication is complete.&#x201D;
        /// </para>
        /// <para>See also 
        /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.RevokePin(InTheHand.Net.BluetoothAddress)"/>
        /// </para>
        /// </remarks>
        /// <returns>True on success, else False.</returns>
        /// <seealso cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.RevokePin(InTheHand.Net.BluetoothAddress)"/>
        public bool SetPin(BluetoothAddress device, string pin)
        {
            if (device.IsNull)
                throw new ArgumentNullException("device");
            if (pin == null)
                throw new ArgumentNullException("pin");
            //remove existing listener
            if (authenticators.ContainsKey(device))
            {
                BluetoothAuthentication bwa = (BluetoothAuthentication)authenticators[device];
                authenticators.Remove(device);
                bwa.Dispose();
            }
            authenticators.Add(device, new BluetoothAuthentication(device, pin));
            return true;
        }
        #endregion

        #region Revoke PIN
        /// <summary>
        /// This function revokes the personal identification number (PIN) for the Bluetooth device.
        /// </summary>
        /// <remarks><para>On Windows CE platforms this calls <c>BthRevokePIN</c>,
        /// its MSDN remarks say:
        /// </para>
        /// <para>&#x201C;When the PIN is revoked, it is removed from registry.
        /// The active connection to the device is not necessary, nor is the presence
        /// of the Bluetooth controller.&#x201D;
        /// </para>
        /// <para>On Windows CE platforms this removes any pending BluetoothWin32Authentication object but does not remove the PIN for an already authenticated device.
        /// Use RemoveDevice to ensure a pairing is completely removed.</para>
        /// <para>See also 
        /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.SetPin(InTheHand.Net.BluetoothAddress,System.String)"/>
        /// </para>
        /// </remarks>
        /// <param name="device">The remote device.</param>
        /// <returns>True on success, else False.</returns>
        /// <seealso cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.SetPin(InTheHand.Net.BluetoothAddress,System.String)"/>
        public bool RevokePin(BluetoothAddress device)
        {
            if (device.IsNull)
                throw new ArgumentNullException("device");
            if (authenticators.ContainsKey(device))
            {
                BluetoothAuthentication bwa = (BluetoothAuthentication)authenticators[device];
                authenticators.Remove(device);
                bwa.Dispose();
                return true;
            }
            return false;
        }
        #endregion
    }
}
