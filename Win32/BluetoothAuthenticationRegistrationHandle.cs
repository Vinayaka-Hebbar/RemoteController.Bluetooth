using System.Runtime.InteropServices;

namespace RemoteController.Win32
{
    // Or should we derive from Microsoft.Win32.SafeHandles.SafeHandleMinusOneIsInvalid?
    internal sealed class BluetoothAuthenticationRegistrationHandle
        : Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
    {
        object m_objectToKeepAlive, m_objectToKeepAlive2;
#if DEBUG
        WeakReference m_weakRef, m_weakRef2;
#endif

        public BluetoothAuthenticationRegistrationHandle()
            : base(true)
        { }

        protected override bool ReleaseHandle()
        {
            bool success = NativeMethods.BluetoothUnregisterAuthentication(handle);
            int gle = Marshal.GetLastWin32Error();
            System.Diagnostics.Debug.Assert(success,
                "BluetoothUnregisterAuthentication returned false, GLE="
                + gle.ToString() + "=0x" + gle.ToString("X"));
            return success;
        }

        internal void SetObjectToKeepAlive(object objectToKeepAlive, object objectToKeepAlive2)
        {
            m_objectToKeepAlive = objectToKeepAlive;
            m_objectToKeepAlive2 = objectToKeepAlive2;
#if DEBUG
            m_weakRef = new WeakReference(m_objectToKeepAlive);
            m_weakRef2 = new WeakReference(m_objectToKeepAlive);
#endif
        }

    }//class
}