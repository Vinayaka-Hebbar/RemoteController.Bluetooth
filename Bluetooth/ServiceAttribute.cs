namespace RemoteController.Bluetooth
{
    /// <summary>
    /// Holds an attribute from an SDP service record.
    /// </summary>
    /// -
    /// <remarks>
    /// Access its SDP Data Element through the 
    /// <see cref="P:InTheHand.Net.Bluetooth.ServiceElement.Value"/> property and read the 
    /// data value through the methods and properties on the returned 
    /// <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/>.
    /// </remarks>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
#endif
    public sealed class ServiceAttribute
    {
        //--------------------------------------------------------------
        readonly ushort m_id;
        readonly ServiceElement m_element;

        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/> class.
        /// </summary>
        /// -
        /// <param name="id">The Attribute Id as a <see cref="T:InTheHand.Net.Bluetooth.ServiceAttributeId"/>.</param>
        /// <param name="value">The value as a <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/>.</param>
        public ServiceAttribute(ushort id, ServiceElement value)
        {
            m_id = id;
            m_element = value;
        }

        //--------------------------------------------------------------

        /// <summary>
        /// Get the Attribute Id for this attribute.
        /// </summary>
        /// -
        /// <remarks>
        /// <note >Id is a <em>unsigned</em> 32-bit integer but we use return it
        /// is a <em>signed</em> 32-bit integer for CLS Compliance reasons.  It
        /// should not thus be used for ordering etc, for example 0xFFFF will sort
        /// before 0x0001 which is backwards.
        /// </note>
        /// </remarks>
        public ushort Id
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_id; }
        }

        /// <summary>
        /// Get the Attribute Id as a number, e.g. for comparison.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Property <see cref="P:Id"/> should be used as an identifier,
        /// but not as a number.  That#x2019;s because the range is <em>unsigned</em>
        /// 32-bit integer but we use return it is a <em>signed</em> 32-bit integer.
        /// Thus an example list will sort as { 0xFFFF, 0x8001, 0x0001, 0x0302 }
        /// when it should sort as { 0x0001, 0x0302, 0x8001,0xFFFF }
        /// </para>
        /// </remarks>
        internal long IdAsOrdinalNumber
        {
            get
            {
                uint u32 = unchecked(m_id);
                return u32;
            }
        }

        /// <summary>
        /// Get the value of this attributes as a <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/>
        /// </summary>
        public ServiceElement Value
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_element; }
        }

    }//class
}