namespace RemoteController.Bluetooth
{
    /// <summary>
    /// Configures what type of element will be added by the <see cref="ServiceRecordBuilder"/>
    /// for the <see cref="AttributeIds.ProtocolDescriptorList"/> 
    /// attribute.
    /// </summary>
    /// -
    /// <remarks><para>Used with the <see cref="ServiceRecordBuilder.ProtocolType"/>
    /// property.
    /// </para>
    /// </remarks>
    public enum BluetoothProtocolDescriptorType
    {
        /// <summary>
        /// No PDL attribute will be added.
        /// </summary>
        None,
        /// <summary>
        /// A standard L2CAP element will be added.
        /// </summary>
        L2Cap,
        /// <summary>
        /// A standard RFCOMM element will be added.
        /// </summary>
        Rfcomm,
        /// <summary>
        /// A standard GOEP (OBEX) element will be added.
        /// </summary>
        GeneralObex
    }
}