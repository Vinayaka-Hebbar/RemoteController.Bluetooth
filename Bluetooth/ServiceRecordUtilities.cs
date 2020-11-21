using System;

namespace RemoteController.Bluetooth
{
    internal static class ServiceRecordUtilities
    {
        internal static bool IsUuid32Value(Guid protocolGuid)
        {
            byte[] bytes = protocolGuid.ToByteArray();
            bytes[0] = bytes[1] = bytes[2] = bytes[3] = 0;
            Guid copyWithZeros = new Guid(bytes);
            return copyWithZeros.Equals(BluetoothService.BluetoothBase);
        }

        internal static uint GetAsUuid32Value(Guid protocolGuid) //BluetoothGuidHelper.
        {
            if (!IsUuid32Value(protocolGuid))
                throw new ArgumentException("Guid is not a Bluetooth UUID.");
            byte[] bytes = protocolGuid.ToByteArray();
            uint u32 = BitConverter.ToUInt32(bytes, 0);
            return u32;
        }

        //HACK HackProtocolId
        internal enum HackProtocolId : short
        {
            Sdp = 1,
            Rfcomm = 3,
            Obex = 8,
            Bnep = 0x0F,
            Hidp = 0x11,
            L2Cap = 0x0100
        }
    }
}
