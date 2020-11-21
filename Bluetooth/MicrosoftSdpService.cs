using RemoteController.Win32;
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace RemoteController.Bluetooth
{/// <exclude/>
    public static class MicrosoftSdpService
    {
        #region Service
        /// <exclude/>
        /// <summary>
        /// Remove a SDP record as added by <see cref="SetService"/>.
        /// </summary>
        /// <param name="handle">The handle.
        /// </param>
        /// <param name="sdpRecord">The raw record, presumably not actually used by the stack.
        /// </param>
        public static void RemoveService(IntPtr handle, byte[] sdpRecord)
        {
            BTHNS_SETBLOB blob = new BTHNS_SETBLOB(sdpRecord)
            {
                Handle = handle
            };

            WSAQUERYSET qs = new WSAQUERYSET
            {
                dwSize = WqsOffset.StructLength_60,
                dwNameSpace = WqsOffset.NsBth_16
            };
            System.Diagnostics.Debug.Assert(Marshal.SizeOf(qs) == qs.dwSize, "WSAQUERYSET SizeOf == dwSize");

            GCHandle hBlob = GCHandle.Alloc(blob.ToByteArray(), GCHandleType.Pinned);

            BLOB b = new BLOB(blob.Length, hBlob.AddrOfPinnedObject());

            GCHandle hb = GCHandle.Alloc(b, GCHandleType.Pinned);

            qs.lpBlob = hb.AddrOfPinnedObject();

            try
            {
                int hresult = NativeMethods.WSASetService(ref qs, WSAESETSERVICEOP.RNRSERVICE_DELETE, 0);
                ThrowSocketExceptionForHR(hresult);
            }
            finally
            {
                hb.Free();
                hBlob.Free();

                //release blob and associated GCHandles
                blob.Dispose();
            }
        }

        internal static void ThrowSocketExceptionForHR(int errorCode)
        {
            if (errorCode < 0)
            {
                int socketerror = Marshal.GetLastWin32Error();

                throw new SocketException(socketerror);
            }
        }

        /// <exclude/>
        /// <summary>
        /// Add a SDP record.
        /// </summary>
        /// -
        /// <param name="sdpRecord">An array of <see cref="T:System.Byte"/>
        /// containing the complete SDP record.
        /// </param>
        /// <param name="cod">A <see cref="T:InTheHand.Net.Bluetooth.ServiceClass"/>
        /// containing any bits to set in the devices Class of Device value.
        /// </param>
        /// -
        /// <returns>A handle representing the record, pass to 
        /// <see cref="RemoveService"/> to remote the record.
        /// </returns>
        public static IntPtr SetService(byte[] sdpRecord, ServiceClass cod)
        {
            BTHNS_SETBLOB blob = new BTHNS_SETBLOB(sdpRecord);
            //added for XP - adds class of device bits
#if WinXP
            blob.CodService = (uint)cod;
#endif

            WSAQUERYSET qs = new WSAQUERYSET
            {
                dwSize = WqsOffset.StructLength_60,
                dwNameSpace = WqsOffset.NsBth_16
            };
            System.Diagnostics.Debug.Assert(Marshal.SizeOf(qs) == qs.dwSize, "WSAQUERYSET SizeOf == dwSize");
            GCHandle hBlob = GCHandle.Alloc(blob.ToByteArray(), GCHandleType.Pinned);

            BLOB b = new BLOB(blob.Length, hBlob.AddrOfPinnedObject());

            GCHandle hb = GCHandle.Alloc(b, GCHandleType.Pinned);

            qs.lpBlob = hb.AddrOfPinnedObject();

            try
            {
                int hresult = NativeMethods.WSASetService(ref qs, WSAESETSERVICEOP.RNRSERVICE_REGISTER, 0);
                ThrowSocketExceptionForHR(hresult);
            }
            finally
            {
                hb.Free();
                hBlob.Free();
            }

            IntPtr handle = blob.Handle;
            blob.Dispose();

            return handle;
        }
        #endregion
    }
}
