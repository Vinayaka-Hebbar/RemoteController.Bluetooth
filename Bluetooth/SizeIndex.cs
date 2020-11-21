namespace RemoteController.Bluetooth
{
    /// <summary>
    /// Represents the size of the SDP element in the record binary format,
    /// and is stored as the lower 3 bits of the header byte.
    /// </summary>
    /// <seealso cref="ServiceRecordParser.SplitHeaderByte(byte, ElementTypeDescriptor@,SizeIndex@)"/>
    /// <seealso cref="ServiceRecordParser.GetSizeIndex(byte)"/>
    public enum SizeIndex //: byte
    {
        LengthOneByteOrNil = 0,
        LengthTwoBytes = 1,
        LengthFourBytes = 2,
        LengthEightBytes = 3,
        LengthSixteenBytes = 4,
        //
        AdditionalUInt8 = 5,
        AdditionalUInt16 = 6,
        AdditionalUInt32 = 7,
    }//enum
}