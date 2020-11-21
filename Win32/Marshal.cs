using System;
using System.Runtime.InteropServices;

namespace RemoteController.Win32
{
    static class Marshal32
    {
        #region Read/Write IntPtr
        /// <summary>
        /// NETCF doesn't have <see cref="M:System.Runtime.InteropServices.Marshal.ReadIntPtr(System.IntPtr,System.Int32)"/>
        /// </summary>
        public static IntPtr ReadIntPtr(IntPtr ptr, int index)
        {
            IntPtr ptrResult;
            if (IntPtr.Size == 8)
            {
#if NETCF && V1
                throw new NotSupportedException("Marshal32.ReadIntPtr 64-bit.");
#else
                long asInt = Marshal.ReadInt64(ptr, index);
                ptrResult = new IntPtr(asInt);
#endif
            }
            else
            {
                int asInt = Marshal.ReadInt32(ptr, index);
                ptrResult = new IntPtr(asInt);
            }
            return ptrResult;
        }

        public static IntPtr ReadIntPtr(byte[] buf, int index)
        {
            IntPtr ptrResult;
            if (IntPtr.Size == 8)
            {
                long asInt = BitConverter.ToInt64(buf, index);
                ptrResult = new IntPtr(asInt);
            }
            else
            {
                int asInt = BitConverter.ToInt32(buf, index);
                ptrResult = new IntPtr(asInt);
            }
            return ptrResult;
        }

        public static void WriteIntPtr(IntPtr ptr, int index, IntPtr value)
        {
            if (IntPtr.Size == 8)
            {
                Marshal.WriteInt64(ptr, index, value.ToInt64());
            }
            else
            {
                Marshal.WriteInt32(ptr, index, value.ToInt32());
            }
        }

        public static void WriteIntPtr(byte[] buf, int index, IntPtr value)
        {
            byte[] ptrBytes;
            if (IntPtr.Size == 8)
            {
                long asInt = value.ToInt64();
                ptrBytes = BitConverter.GetBytes(asInt);
            }
            else
            {
                int asInt = value.ToInt32();
                ptrBytes = BitConverter.GetBytes(asInt);
            }
            ptrBytes.CopyTo(buf, index);
        }
        #endregion
    }
}
