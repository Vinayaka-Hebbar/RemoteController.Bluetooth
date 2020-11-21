namespace RemoteController.Bluetooth
{
    /// <summary>
    /// Represents the types that an SDP element can hold.
    /// </summary>
    /// <remarks>
    /// <para>
    /// (Is a logical combination of the <see cref="ElementTypeDescriptor"/>
    /// field which defines the major type and the size field in the binary format; and
    /// the size field being made up of the <see cref="T:InTheHand.Net.Bluetooth.SizeIndex"/>
    /// field and any additional length bytes.
    /// </para>
    /// <para>Note, the values here are not the numerical bitwise combination of the 
    /// <see cref="ElementTypeDescriptor"/> and 
    /// <see cref="T:InTheHand.Net.Bluetooth.SizeIndex"/> fields as they appear 
    /// in the encoded protocol.  It was simpler to assign arbitrary values here as 
    /// firstly we wanted zero to be the 'Unknown' value, which conflicts with Nil's
    /// bitwise value; but also because the TextString, sequence and Url types can 
    /// have various SizeIndex values and thus they wouldn&#x2019;t be easily 
    /// representable by one value here).
    /// </para>
    /// </remarks>
    public enum ElementType
    {
        Unknown = 0,

        //--
        // TypeDescriptor.Nil = 0,
        //--
        Nil = 20,

        //--
        // TypeDescriptor.UnsignedInteger = 1,
        //--
        UInt8,
        UInt16,
        UInt32,
        UInt64,
        UInt128,

        //--
        // TypeDescriptor.TwosComplementInteger = 2,
        //--
        Int8 = 30,
        Int16,
        Int32,
        Int64,
        Int128,

        //--
        // TypeDescriptor.Uuid = 3,
        //--
        Uuid16 = 40,
        Uuid32,
        Uuid128,

        //--
        // TypeDescriptor.TextString = 4,
        //--
        TextString,

        //--
        // TypeDescriptor.Boolean = 5,
        //--
        Boolean,

        //--
        // TypeDescriptor.DataElementSequence = 6,
        //--
        ElementSequence,

        //--
        // TypeDescriptor.DataElementAlternative = 7,
        //--
        ElementAlternative,

        //--
        // TypeDescriptor.Url = 8
        //--
        Url,
    }//enum
}