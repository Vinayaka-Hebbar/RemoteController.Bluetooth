namespace RemoteController.Bluetooth
{
    /// <summary>
    /// Represents the type of the element in the SDP record binary format, 
    /// and is stored as the higher 5 bits of the header byte.
    /// </summary>
    /// <remarks>
    /// There is an identifier for each major type: String vs UUID vs unsigned integer.
    /// There are various sizes of UUID and integer type for instance, the resultant
    /// types are listed in enum <see cref="T:InTheHand.Net.Bluetooth.ElementType"/>.
    /// </remarks>
    public enum ElementTypeDescriptor //: byte
    {
        Unknown = -1,
        //--
        Nil = 0,
        UnsignedInteger = 1,
        TwosComplementInteger = 2,
        Uuid = 3,
        TextString = 4,
        Boolean = 5,
        ElementSequence = 6,
        ElementAlternative = 7,
        Url = 8
    }//enum
}