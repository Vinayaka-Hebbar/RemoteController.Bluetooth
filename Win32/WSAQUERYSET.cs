using System;
using System.Runtime.InteropServices;

namespace RemoteController.Win32
{
    [StructLayout(LayoutKind.Sequential, Size = 60)]
    internal struct WSAQUERYSET
    {
        public int dwSize;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpszServiceInstanceName;
        public IntPtr lpServiceClassId;
        IntPtr lpVersion;
        IntPtr lpszComment;
        public int dwNameSpace;
        IntPtr lpNSProviderId;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpszContext;
        int dwNumberOfProtocols;
        IntPtr lpafpProtocols;
        IntPtr lpszQueryString;
        public int dwNumberOfCsAddrs;
        public IntPtr lpcsaBuffer;
        int dwOutputFlags;
        public IntPtr lpBlob;
    }

    internal struct BLOB
    {
        public int cbSize;
        public IntPtr pBlobData;

        internal BLOB(int size, IntPtr data)
        {
            cbSize = size;
            pBlobData = data;
        }
    }

    static class WqsOffset
    {
        public static readonly int dwSize_0 = 0;
        public static readonly int dwNameSpace_20 = 5 * IntPtr.Size;
        public static readonly int lpcsaBuffer_48 = 12 * IntPtr.Size;
        public static readonly int dwOutputFlags_52 = 13 * IntPtr.Size;
        public static readonly int lpBlob_56 = 14 * IntPtr.Size;
        //
        public static readonly int StructLength_60 = 15 * IntPtr.Size;
        //
        public const int NsBth_16 = 16;

        static bool s_doneAssert;

        [System.Diagnostics.Conditional("DEBUG")]
        public static void AssertCheckLayout()
        {
            if (s_doneAssert)
                return;
            s_doneAssert = true;
            System.Diagnostics.Debug.Assert(WqsOffset.dwNameSpace_20
                            == Marshal.OffsetOf(typeof(WSAQUERYSET), "dwNameSpace").ToInt64(), "offset dwNameSpace");
            System.Diagnostics.Debug.Assert(WqsOffset.lpcsaBuffer_48
                == Marshal.OffsetOf(typeof(WSAQUERYSET), "lpcsaBuffer").ToInt64(), "offset lpcsaBuffer");
            System.Diagnostics.Debug.Assert(WqsOffset.dwOutputFlags_52
                == Marshal.OffsetOf(typeof(WSAQUERYSET), "dwOutputFlags").ToInt64(), "offset dwOutputFlags");
            System.Diagnostics.Debug.Assert(WqsOffset.lpBlob_56
                == Marshal.OffsetOf(typeof(WSAQUERYSET), "lpBlob").ToInt64(), "offset lpBlob");
            //
            System.Diagnostics.Debug.Assert(WqsOffset.StructLength_60
                == Marshal.SizeOf(typeof(WSAQUERYSET)), "StructLength");
        }

    }//class
}