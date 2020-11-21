﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteController.Bluetooth
{
    /// <summary>
    /// Holds an SDP data element.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>A Service Element hold the data in a SDP Service Record.  It can 
    /// hold various types of data, being like the &#x2018;variant&#x2019; type in some
    /// environments.  Each <see cref="ServiceAttribute"/> in
    /// a <see cref="ServiceRecord"/> holds its content in a
    /// Service Element.
    /// </para>
    /// <para>The types currently defined in the Service Discovery specification
    /// include unsigned and signed integers 
    /// of various sizes (8-bit, 16-bit etc), UUIDs in the full 128-bit form or
    /// in the 16 and 32-bit forms, TextString, Url etc.  An element can itself
    /// also contain a list of element, either as a &#x2018;sequence&#x2019; or an
    /// &#x2018;alternative&#x2019;, and thus an attribute can contain a tree of values,
    /// e.g. as used by the 
    /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ProtocolDescriptorList"/>
    /// attribute.
    /// </para>
    /// <para>The type that an element is holding can be accessed with the 
    /// <see cref="P:InTheHand.Net.Bluetooth.ServiceElement.ElementTypeDescriptor"/> and 
    /// <see cref="P:InTheHand.Net.Bluetooth.ServiceElement.ElementType"/> properties which
    /// are of type <see cref="ElementTypeDescriptor"/> and
    /// <see cref="ElementType"/> respectively, the former being 
    /// the &#x2018;major&#x2019; type e.g. 
    /// <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.UnsignedInteger"/>, and
    /// the latter the &#x2018;minor&#x2019; type e.g. 
    /// <see cref="F:InTheHand.Net.Bluetooth.ElementType.UInt16"/>.
    /// </para>
    /// <para>The element's value can be accessed in various ways, either directly 
    /// in its internal form through its <see cref="P:InTheHand.Net.Bluetooth.ServiceElement.Value"/>
    /// property.  It has return type <see cref="T:System.Object"/> so the value 
    /// will have to be cast before use, see the <c>UInt16</c> example below.  There
    /// are also a number of type-specific methods, e.g. 
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsElementArray"/>,
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsUuid"/>, 
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsString(System.Text.Encoding)"/>
    /// etc.  Each will throw an <see cref="T:System.InvalidOperationException"/>
    /// if the element is not of a suitable type.  The complete set is:</para>
    /// <list type="table">
    /// <listheader><term><see cref="ElementType"/></term>
    /// <description>Access method, or .NET Type for direct access</description>
    /// </listheader>
    /// <item><term><c>Nil</c></term>
    /// <description><see langword="null"/></description></item>
    /// 
    /// <item><term><c>Uint8</c></term><description><see cref="T:System.Byte"/></description></item>
    /// <item><term><c>Uint16</c></term><description><see cref="T:System.UInt16"/></description></item>
    /// <item><term><c>Uint32</c></term><description><see cref="T:System.UInt32"/></description></item>
    /// <item><term><c>Uint64</c></term><description>Currently unsupported.</description></item>
    /// <item><term><c>Uint128</c></term><description>Currently unsupported.</description></item>
    /// 
    /// <item><term><c>Int8</c></term><description><see cref="T:System.SByte"/></description></item>
    /// <item><term><c>Int16</c></term><description><see cref="T:System.Int16"/></description></item>
    /// <item><term><c>Int32</c></term><description><see cref="T:System.Int32"/></description></item>
    /// <item><term><c>Int64</c></term><description>Currently unsupported.</description></item>
    /// <item><term><c>Int128</c></term><description>Currently unsupported.</description></item>
    ///
    /// <item><term><c>Uuid16</c></term><description>Via <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsUuid"/>, or as <see cref="T:System.UInt16"/></description></item>
    /// <item><term><c>Uuid32</c></term><description>Via <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsUuid"/>, or as <see cref="T:System.UInt16"/></description></item>
    /// <item><term><c>Uuid128</c></term><description>Via <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsUuid"/></description></item>
    /// 
    /// <item><term><c>TextString</c></term><description>With 
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsString(System.Text.Encoding)"/>
    /// or <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsStringUtf8"/> etc.
    /// The underlying value can be an array of bytes, or as a <see cref="T:System.String"/>
    /// the <see cref="ServiceRecordParser"/> will set an
    /// array of bytes, whereas a manually created record will likely contain a
    /// <see cref="T:System.String"/>.
    /// </description></item>
    /// 
    /// <item><term><c>Boolean</c></term><description><see cref="T:System.Boolean"/></description></item>
    /// 
    /// <item><term><c>ElementSequence</c></term><description>With
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsElementArray"/> or
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsElementList"/>
    /// </description></item>
    /// <item><term><c>ElementSequence</c></term><description>-"-</description></item>
    ///
    /// <item><term><c>Url</c></term><description>Via <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsUri"/>,
    /// can be stored interally as <see cref="T:System.Uri"/> or as an array of bytes
    /// </description></item>
    /// </list>
    /// 
    /// <para>Note that there are no access 
    /// methods for the numeric type for instance so the 
    /// <see cref="P:InTheHand.Net.Bluetooth.ServiceElement.Value"/> property will have
    /// to be used e.g.
    /// <code lang="C#">
    /// // ElementType is UInt16
    /// ushort x = (ushort)element.Value;
    /// </code>
    /// or
    /// <code lang="C#">
    /// // ElementType is UInt16
    /// Dim x As UShort = CUShort(element.Value);
    /// </code>
    /// </para>
    /// <para>Additional type-specific methods can be added as required, in fact the 
    /// full set of 19+ could be added, it just requires implementation and test&#x2026;
    /// </para>
    /// </remarks>
    public sealed class ServiceElement
    {
        //--------------------------------------------------------------
        ElementType m_type;
        ElementTypeDescriptor m_etd;
        object m_rawValue;
        //
        private const bool _strictStringDecoding = true;


        //--------------------------------------------------------------

        /// <overloads>
        /// Initializes a new instance of the <see cref="ServiceElement"/> class.
        /// </overloads>
        /// -
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceElement"/> class.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>The type of the object passed in the <paramref name="value"/> parameter
        /// <strong>must</strong> suit the type of the element.  For instance if the element type is 
        /// <see cref="F:InTheHand.Net.Bluetooth.ElementType.UInt8"/> then the object
        /// passed in must be a <see cref="T:System.Byte"/>, if the element type is
        /// <see cref="F:InTheHand.Net.Bluetooth.ElementType.TextString"/> then the object
        /// must either be a <see cref="T:System.String"/> or the string encoded as 
        /// an array of <see cref="T:System.Byte"/>, 
        /// and if the element type is <see cref="F:InTheHand.Net.Bluetooth.ElementType.Uuid16"/>
        /// then the object passed in must be a <see cref="T:System.UInt16"/>,
        /// etc.
        /// For the full list of types see the class level documentation 
        /// (<see cref="ServiceElement"/>).
        /// </para>
        /// <para>For numerical element types the 
        /// <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.CreateNumericalServiceElement(InTheHand.Net.Bluetooth.ElementType,System.Object)"/>
        /// factory method will accept any integer type and attempt to convert it to the 
        /// required type before creating the <see cref="ServiceElement"/>,
        /// for example for element type <see cref="F:InTheHand.Net.Bluetooth.ElementType.UInt8"/> 
        /// it will accept an <see cref="T:System.Int32"/> parameter and convert
        /// it to a <see cref="T:System.Byte"/> internally.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="type">The type of the element as an ElementType.
        /// </param>
        /// <param name="value">The value for the new element,
        /// <strong>must</strong> suit the type of the element.
        /// See the remarks for more information.
        /// </param>
        /// -
        /// <example>
        /// <code lang="C#">
        /// ServiceElement e
        /// e = new ServiceElement(ElementType.TextString, "Hello world");
        /// e = new ServiceElement(ElementType.TextString, new byte[] { (byte)'h', (byte)'i', });
        /// e = new ServiceElement(ElementType.Uuid16, (UInt16)0x1101);
        ///
        ///
        /// int i = 10;
        /// int j = -1;
        /// 
        /// // Error, Int32 not suitable for element type UInt8.
        /// ServiceElement e0 = new ServiceElement(ElementType.UInt8, i);
        /// 
        /// // Success, Byte value 10 stored.
        /// ServiceElement e1 = ServiceElement.CreateNumericalServiceElement(ElementType.UInt8, i);
        /// 
        /// // Error, -1 not in range of type Byte.
        /// ServiceElement e2 = ServiceElement.CreateNumericalServiceElement(ElementType.UInt8, j);
        /// </code>
        /// </example>
        public ServiceElement(ElementType type, object value)
            : this(GetEtdForType(type), type, value)
        { }
#if V1
    /*
#endif
#pragma warning restore 618
#if V1
    */
#endif

        private static ElementTypeDescriptor GetEtdForType(ElementType type)
        {
            return ServiceRecordParser.GetEtdForType(type);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceElement"/> class.
        /// </summary>
        /// -
        /// <param name="type">The type of the element as an ElementType.
        /// Should be either <c>ElementSequence</c>/<c>ElementAlternative</c> types.
        /// </param>
        /// <param name="childElements">A list of elements.
        /// </param>
        public ServiceElement(ElementType type, IList<ServiceElement> childElements)
            : this(type, (object)childElements)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceElement"/> class.
        /// </summary>
        /// -
        /// <param name="type">The type of the element as an ElementType.
        /// Should be either <c>ElementSequence</c>/<c>ElementAlternative</c> types.
        /// </param>
        /// <param name="childElements">A list of elements.
        /// </param>
        public ServiceElement(ElementType type, params ServiceElement[] childElements)
            : this(CheckTypeSuitsElementParamsArray(type, childElements), (object)childElements)
        { }

        private static ElementType CheckTypeSuitsElementParamsArray(ElementType typePassThru, ServiceElement[] childElements)
        {
            // Throw a more specific error about the params array when the value is null.
            // As error would be thrown by SetValue, but we can report a more helpful message.
            if (childElements != null)
            {
                //
                if (typePassThru != ElementType.ElementSequence && typePassThru != ElementType.ElementAlternative)
                {
                    throw new ArgumentException(ErrorMsgSeqAltTypeNeedElementArray);
                }
            }
            return typePassThru;
        }

        //--------------------------------------------------------------

        /// <summary>
        /// Obsolete, use <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.#ctor(InTheHand.Net.Bluetooth.ElementType,System.Object)"/> instead.
        /// Initializes a new instance of the <see cref="ServiceElement"/> class.
        /// </summary>
        internal ServiceElement(ElementTypeDescriptor etd, ElementType type, object value)
        {
            ServiceRecordParser.VerifyTypeMatchesEtd(etd, type);
            m_type = type;
            m_etd = etd;
            SetValue(value);
        }

        internal void SetValue(object value)
        {
            ElementTypeDescriptor etd = m_etd;
            ElementType type = m_type;
            //
            if (value == null)
            {
                if (etd == ElementTypeDescriptor.ElementSequence || etd == ElementTypeDescriptor.ElementAlternative)
                {
                    throw new ArgumentNullException("value", "Type DataElementSequence and DataElementAlternative need an list of AttributeValue.");
                }
                else if (etd == ElementTypeDescriptor.Nil)
                {
                }
                else if (etd == ElementTypeDescriptor.Unknown)
                {
                }
                else
                {
                    throw new ArgumentNullException("value", "Null not valid for type: '" + type + "'.");
                }
            }
            else
            {
                IList<ServiceElement> asElementList = value as IList<ServiceElement>;
                if (etd == ElementTypeDescriptor.ElementSequence || etd == ElementTypeDescriptor.ElementAlternative)
                {
                    if (asElementList == null)
                    {
                        throw new ArgumentException("Type ElementSequence and ElementAlternative need an list of ServiceElement.");
                    }
                }
                else
                {
                    if (asElementList != null)
                    {
                        throw new ArgumentException("Type ElementSequence and ElementAlternative must be used for an list of ServiceElement.");
                    }
                }
                //--------
                bool validTypeForType;
                if (type == ElementType.Nil)
                {
                    validTypeForType = value == null;
                }
                else if (etd == ElementTypeDescriptor.UnsignedInteger || etd == ElementTypeDescriptor.TwosComplementInteger)
                {
                    switch (type)
                    {
                        case ElementType.UInt8:
                            validTypeForType = value is byte;
                            break;
                        case ElementType.Int8:
                            validTypeForType = value is sbyte;
                            break;
                        case ElementType.UInt16:
                            validTypeForType = value is ushort;
                            break;
                        case ElementType.Int16:
                            validTypeForType = value is short;
                            break;
                        case ElementType.UInt32:
                            validTypeForType = value is uint;
                            break;
                        case ElementType.Int32:
                            validTypeForType = value is int;
                            break;
                        case ElementType.UInt64:
                            validTypeForType = value is ulong;
                            break;
                        case ElementType.Int64:
                            validTypeForType = value is long;
                            break;
                        case ElementType.UInt128:
                        case ElementType.Int128:
                            const int NumBytesIn128bits = 16;
                            if (value is byte[] arr && arr.Length == NumBytesIn128bits)
                            {
                                validTypeForType = true;
                            }
                            else
                            { // HACK UNTESTED
                                validTypeForType = false;
                                throw new ArgumentException(
                                    "Element type '" + type + "' needs a length 16 byte array.");
                            }
                            break;
                        default:
                            System.Diagnostics.Debug.Fail("Unexpected numerical type");
                            validTypeForType = false;
                            break;
                    }//switch
                }
                else if (type == ElementType.Uuid16)
                {
                    validTypeForType = value is ushort;
                }
                else if (type == ElementType.Uuid32)
                {
                    validTypeForType
                        = value is ushort
                        || value is short
                        || value is uint
                        || value is int
                        ;
                }
                else if (type == ElementType.Uuid128)
                {
                    validTypeForType = value is Guid;
                }
                else if (type == ElementType.TextString)
                {
                    validTypeForType = value is byte[] || value is string;
                }
                else if (type == ElementType.Boolean)
                {
                    validTypeForType = value is bool;
                }
                else if (type == ElementType.ElementSequence || type == ElementType.ElementAlternative)
                {
                    validTypeForType = asElementList != null;
                }
                else
                {
                    // if (type == ElementType.Url)
                    System.Diagnostics.Debug.Assert(type == ElementType.Url);
                    validTypeForType = value is byte[] || value is Uri || value is string;
                }
                if (!validTypeForType)
                {
                    throw new ArgumentException("CLR type '" + value.GetType().Name + "' not valid type for element type '" + type + "'.");
                }
            }
            m_rawValue = value;
        }


        //--------------------------------------------------------------

        /// <summary>
        /// Create an instance of <see cref="ServiceElement"/>
        /// but internally converting the numeric value to the required type.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>As noted in the constructor documentation 
        /// (<see cref="M:InTheHand.Net.Bluetooth.ServiceElement.#ctor(InTheHand.Net.Bluetooth.ElementType,System.Object)"/>)
        /// the type of the value supplied <strong>must</strong> exactly match the element's natural type,
        /// the contructor will return an error if that is not the case. This method 
        /// will instead attempt to convert the value to the required type.  It uses 
        /// the <see cref="T:System.IConvertible"/> interface to do the conversion, for
        /// instance if the element type is <c>Uint16</c> then it will cast the input value
        /// to <see cref="T:System.IConvertible"/> and call 
        /// <see cref="M:System.IConvertible.ToUInt16(System.IFormatProvider)"/> on it.
        /// If the value is not convertible to the element type then an 
        /// <see cref="T:System.ArgumentOutOfRangeException"/> will be thrown see below.
        /// </para>
        /// <para>For instance, passing in an C# <c>int</c> / Visual Basic <c>Integer</c>
        /// to the constructor will fail for element types <see cref="F:InTheHand.Net.Bluetooth.ElementType.UInt8"/>
        /// etc, however by using this method it will succeed if the value is in the
        /// correct range.
        /// For example
        /// <code lang="C#">
        /// int i = 10;
        /// int j = -1;
        /// 
        /// // Error, Int32 not suitable for element type UInt8.
        /// ServiceElement e0 = new ServiceElement(ElementType.UInt8, i);
        /// 
        /// // Success, Byte value 10 stored.
        /// ServiceElement e1 = ServiceElement.CreateNumericalServiceElement(ElementType.UInt8, i);
        /// 
        /// // Error, -1 not in range of type Byte.
        /// ServiceElement e2 = ServiceElement.CreateNumericalServiceElement(ElementType.UInt8, j);
        /// </code>
        /// The last example failing with:
        /// <code lang="none">
        /// System.ArgumentOutOfRangeException: Value '-1'  of type 'System.Int32' not valid for element type UInt16.
        ///  ---> System.OverflowException: Value was either too large or too small for a UInt16.
        ///    at System.Convert.ToUInt16(Int32 value)
        ///    at System.Int32.System.IConvertible.ToUInt16(IFormatProvider provider)
        ///    at InTheHand.Net.Bluetooth.ServiceElement.ConvertNumericalValue(ElementType elementType, Object value)
        ///    --- End of inner exception stack trace ---
        ///    at InTheHand.Net.Bluetooth.ServiceElement.ConvertNumericalValue(ElementType elementType, Object value)
        ///    at InTheHand.Net.Bluetooth.ServiceElement.CreateNumericalServiceElement(ElementType elementType, Object value)
        ///    at MiscFeatureTestCs.Main(String[] args)
        /// </code>
        /// </para>
        /// </remarks>
        /// -
        /// <param name="elementType">The type of the element as an ElementType.
        /// Should be one of the <c>UnsignedInteger</c>/<c>TwosComplementInteger</c> types.
        /// </param>
        /// <param name="value">The value for the new element,
        /// should be a numerical type.
        /// </param>
        /// -
        /// <returns>The new element.
        /// </returns>
        /// -
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="elementType"/> is not a numerical type.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The value wasn&#x2019;t convertible to the required type, e.g. if -1 is
        /// passed for element type UInt8, as shown above.
        /// </exception>
        public static ServiceElement CreateNumericalServiceElement(ElementType elementType, object value)
        {
            ElementTypeDescriptor etd = GetEtdForType(elementType);
            if (!(etd == ElementTypeDescriptor.UnsignedInteger
                   || etd == ElementTypeDescriptor.TwosComplementInteger))
            {
                throw new ArgumentException(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    ErrorMsgFmtCreateNumericalGivenNonNumber,
                    etd/*, (int)etd, elementType, (int)elementType*/));
            }
            object valueConverted = ConvertNumericalValue(elementType, value);
            return new ServiceElement(elementType, valueConverted);
        }

        private static object ConvertNumericalValue(ElementType elementType, object value)
        {
            Exception innerEx;
            //
            if (value is byte || value is short || value is int || value is long
                || value is sbyte || value is ushort || value is uint || value is ulong
                || value is Enum)
            {
                try
                {
                    IConvertible cble = (IConvertible)value;
                    IFormatProvider fp = System.Globalization.CultureInfo.InvariantCulture;
                    object naturalTypedValue;
                    switch (elementType)
                    {
                        case ElementType.UInt8:
                            naturalTypedValue = cble.ToByte(fp);
                            break;
                        case ElementType.Int8:
                            naturalTypedValue = cble.ToSByte(fp);
                            break;
                        //--
                        case ElementType.UInt16:
                            naturalTypedValue = cble.ToUInt16(fp);
                            break;
                        case ElementType.Int16:
                            naturalTypedValue = cble.ToInt16(fp);
                            break;
                        //--
                        case ElementType.UInt64:
                            naturalTypedValue = cble.ToUInt64(fp);
                            break;
                        case ElementType.Int64:
                            naturalTypedValue = cble.ToInt64(fp);
                            break;
                        //--
                        case ElementType.UInt32:
                            naturalTypedValue = cble.ToUInt32(fp);
                            break;
                        default:
                            //case ElementType.Int32:
                            System.Diagnostics.Debug.Assert(elementType == ElementType.Int32, "Unexpected numeric type");
                            naturalTypedValue = cble.ToInt32(fp);
                            break;
                    }
                    return naturalTypedValue;
                }
                catch (OverflowException ex)
                {
                    innerEx = ex;
                    //} catch (InvalidCastException ex) {
                    //    innerEx = ex;
                }
            }
            else
            {
                innerEx = null;
            }
            throw ServiceRecordParser.new_ArgumentOutOfRangeException(
                string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "Value '{1}'  of type '{2}' not valid for element type {0}.",
                    elementType, value, value.GetType()), innerEx);
        }

        //--------------------------------------------------------------

        /// <summary>
        /// Gets the type of the element as an <see cref="ElementType"/>.
        /// </summary>
        public ElementType ElementType
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_type; }
        }

        /// <summary>
        /// Gets the SDP Element Type Descriptor of the element
        /// as an <see cref="ElementTypeDescriptor"/>.
        /// </summary>
        public ElementTypeDescriptor ElementTypeDescriptor
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_etd; }
        }

        //------------------------

        /// <summary>
        /// Gets the value of the element as the .NET type it is stored as.
        /// </summary>
        /// <remarks>
        /// In most cases the type-specific property should be used instead, e.g 
        /// <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsElementList"/>, 
        /// <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsUri"/>, 
        /// <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsUuid"/>, etc.
        /// </remarks>
        public object Value
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_rawValue; }
        }


        /// <summary>
        /// Gets the value as a list of <see cref="ServiceElement"/>.
        /// </summary>
        /// -
        /// <returns>The list of elements as an list.
        /// </returns>
        /// -
        /// <exception cref="T:System.InvalidOperationException">
        /// The service element is not of type
        /// <c>ElementType</c>.<see cref="F:InTheHand.Net.Bluetooth.ElementType.ElementSequence"/>
        /// or <see cref="F:InTheHand.Net.Bluetooth.ElementType.ElementAlternative"/>.
        /// </exception>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
#endif
        public IList<ServiceElement> GetValueAsElementList()
        {
            if (m_etd != ElementTypeDescriptor.ElementSequence && m_etd != ElementTypeDescriptor.ElementAlternative)
            {
                throw new InvalidOperationException(ErrorMsgNotSeqAltType);
            }
            // #ctor disallows null value for seq/alt.
            System.Diagnostics.Debug.Assert(m_rawValue != null);
            return (IList<ServiceElement>)m_rawValue;
        }


        /// <summary>
        /// Gets the value as a array of <see cref="ServiceElement"/>.
        /// </summary>
        /// -
        /// <returns>The list of elements as an array.
        /// </returns>
        /// -
        /// <exception cref="T:System.InvalidOperationException">
        /// The service element is not of type
        /// <c>ElementType</c>.<see cref="F:InTheHand.Net.Bluetooth.ElementType.ElementSequence"/>
        /// or <see cref="F:InTheHand.Net.Bluetooth.ElementType.ElementAlternative"/>.
        /// </exception>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
#endif
        public ServiceElement[] GetValueAsElementArray()
        {
            IList<ServiceElement> list = GetValueAsElementList();
            ServiceElement[] arr = new ServiceElement[list.Count];
            GetValueAsElementList().CopyTo(arr, 0);
            return arr;
        }

        //------------------------

        /// <summary>
        /// Gets the value as a <see cref="T:System.Uri"/>.
        /// </summary>
        /// -
        /// <returns>The Url value as a <see cref="T:System.Uri"/>.
        /// </returns>
        /// -
        /// <remarks>
        /// <para>It turns out that we can't trust vendors to add only valid
        /// URLs to their records, for instance the iPhone has an attribute
        /// with value "www.apple.com" which isn't a URL as it has no scheme
        /// part (http://) etc.
        /// </para>
        /// <para>Thus a Url value in an element can be stored in a number of
        /// formats.  If created by the parser then it will be stored as a 
        /// <see cref="T:System.String"/> or as an array of
        /// <see cref="T:System.Byte"/> if property
        /// <see cref="P:InTheHand.Net.Bluetooth.ServiceRecordParser.LazyUrlCreation">ServiceRecordParser.LazyUrlCreation</see>
        /// is set.  If created locally it can be those types or also 
        /// <see cref="T:System.Uri"/> .
        /// </para>
        /// <para>This method will try to convert from those formats to <see cref="T:System.Uri"/>.
        /// If the URL is invalid e.g. has bad characters or is missing the scheme
        /// part etc then an error will occur.  One can instead access the
        /// element's <see cref="P:InTheHand.Net.Bluetooth.ServiceElement.Value"/>
        /// property and expect one of the three types.  When created by the 
        /// parser it will be of type <see cref="T:System.String"/> unless 
        /// <see cref="P:InTheHand.Net.Bluetooth.ServiceRecordParser.LazyUrlCreation"/>
        /// is set.
        /// </para>
        /// </remarks>
        /// -
        /// <exception cref="T:System.InvalidOperationException">
        /// The service element is not of type
        /// <c>ElementType</c>.<see cref="F:InTheHand.Net.Bluetooth.ElementType.Url"/>.
        /// </exception>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
#endif
        public Uri GetValueAsUri()
        {
            if (m_type != ElementType.Url)
            {
                throw new InvalidOperationException(ErrorMsgNotUrlType);
            }
            System.Diagnostics.Debug.Assert(m_rawValue != null);
            Uri asUri = m_rawValue as Uri;
            if (asUri == null)
            {
                byte[] arr = m_rawValue as byte[];
                string str;
                if (arr != null)
                {
                    str = ServiceRecordParser.CreateUriStringFromBytes(arr);
                }
                else
                {
                    str = (string)m_rawValue;
                }
                asUri = new Uri(str);
            }
            return asUri;
        }

        //------------------------

        /// <summary>
        /// Gets the value as a <see cref="T:System.Guid"/>.
        /// </summary>
        /// -
        /// <returns>The UUID value as a <see cref="T:System.Guid"/>.
        /// </returns>
        /// -
        /// <exception cref="T:System.InvalidOperationException">
        /// The service element is not of type
        /// <c>ElementType</c>.<see cref="F:InTheHand.Net.Bluetooth.ElementType.Uuid128"/>.
        /// </exception>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
#endif
        public Guid GetValueAsUuid()
        {
            if (m_etd != ElementTypeDescriptor.Uuid)
            {
                throw new InvalidOperationException(ErrorMsgNotUuidType);
            }
            //
            Guid result;
            if (m_type == ElementType.Uuid16)
            {
                result = BluetoothService.CreateBluetoothUuid((ushort)Value);
                return result;
            }
            else if (m_type == ElementType.Uuid32)
            {
                result = BluetoothService.CreateBluetoothUuid((int)Value);
                return result;
            }
            else
            {
                return (Guid)Value;
            }
        }

        //TODO ((Could have GetValueAsUuid16/32. Check subtype, but convert to short form if is Bluetooth Based.))

        //------------------------

        /// <summary>
        /// Get the value of the <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.TextString"/>,
        /// where it is encoded using the given encoding form.
        /// </summary>
        /// -
        /// <param name="encoding">The <see cref="T:System.Text.Encoding"/>
        /// object to be used to decode the string value
        /// if it has been read as a raw byte array.
        /// </param>
        /// -
        /// <returns>
        /// A <see cref="T:System.String"/> holding the value of the 
        /// <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.TextString"/>
        /// from the service element.
        /// </returns>
        /// -
        /// <exception cref="T:System.InvalidOperationException">
        /// The service element is not of type 
        /// <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.TextString"/>.
        /// </exception>
        public string GetValueAsString(Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (m_type != ElementType.TextString)
            {
                throw new InvalidOperationException(ErrorMsgNotTextStringType);
            }
            //
            string stringAsStored = m_rawValue as string;
            if (stringAsStored != null)
            {
                return stringAsStored;
            }
            //
            byte[] rawBytes = (byte[])m_rawValue;
            string str = encoding.GetString(rawBytes, 0, rawBytes.Length);
            if (str.Length > 0 && str[str.Length - 1] == 0)
            { // dodgy null-termination
                str = str.Substring(0, str.Length - 1);
            }
            return str;
        }

        /// <summary>
        /// Get the value of the <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.TextString"/>,
        /// when it is encoded as specified by the given IETF Charset identifer.
        /// </summary>
        /// -
        /// <remarks>
        /// Note that a strict decoding of the string is carried out 
        /// (except on the NETCF where it is not supported). 
        /// Thus if the value is not in the specified encoding, or has been
        /// encoded incorrectly, then an error will occur.
        /// </remarks>
        /// -
        /// <returns>
        /// A <see cref="T:System.String"/> holding the value of the 
        /// <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.TextString"/>
        /// from the service element.
        /// </returns>
        /// -
        /// <exception cref="T:System.InvalidOperationException">
        /// The service element is not of type 
        /// <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.TextString"/>.
        /// </exception>
        /// <exception cref="T:System.Text.DecoderFallbackException">
        /// If the value in the service element is not a valid string in the given encoding.
        /// </exception>
        public string GetValueAsString(LanguageBaseItem languageBase)
        {
            if (languageBase == null)
            {
                throw new ArgumentNullException("languageBase");
            }
            Encoding enc = languageBase.GetEncoding();
#if ! PocketPC
            if (_strictStringDecoding)
            {
                enc = (Encoding)enc.Clone(); //not in NETCFv1
                enc.DecoderFallback = new DecoderExceptionFallback(); // not in NETCF.
                // Not intended for encoding, but set it anyway.
                enc.EncoderFallback = new EncoderExceptionFallback(); // not in NETCF.
            }
#endif
            return GetValueAsString(enc);
        }

        /// <summary>
        /// Get the value of the <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.TextString"/>,
        /// when it is encoded as UTF-8.
        /// </summary>
        /// -
        /// <remarks>
        /// Note: a strict decoding is used.
        /// Thus if the value is not in UTF-8 encoding or has been
        /// encoded incorrectly an error will occur.
        /// </remarks>
        /// -
        /// <returns>
        /// A <see cref="T:System.String"/> holding the value of the 
        /// <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.TextString"/>
        /// from the service element.
        /// </returns>
        /// -
        /// <exception cref="T:System.Text.DecoderFallbackException">
        /// If the value in the service element is not a valid string in the given encoding.
        /// On NETCF, an <see cref="T:System.ArgumentException"/> is thrown; not that
        /// <see cref="T:System.ArgumentException"/> is the base class of the
        /// <see cref="T:System.Text.DecoderFallbackException"/> exception.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The service element is not of type 
        /// <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.TextString"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "but throws")]
        public string GetValueAsStringUtf8()
        {
            Encoding enc = new UTF8Encoding(false, true); //throwOnInvalidBytes=true
            return GetValueAsString(enc);
        }

        //--------------------------------------------------------------        
        /// <exclude/>
        public const string ErrorMsgNotUuidType = "Element is not of type UUID.";
        /// <exclude/>
        public const string ErrorMsgNotTextStringType = "Not TextString type.";
        /// <exclude/>
        public const string ErrorMsgNotUrlType = "Not Url type.";
        /// <exclude/>
        public const string ErrorMsgNotSeqAltType = "Not Element Sequence or Alternative type.";
        /// <exclude/>
        public const string ErrorMsgSeqAltTypeNeedElementArray = "ElementType Sequence or Alternative needs an array of ServiceElement.";
        /// <exclude/>
        public const string ErrorMsgFmtCreateNumericalGivenNonNumber
            = "Not a numerical type ({0})."; // ET is our own!!
        /// <exclude/>
        public const string ErrorMsgListContainsNotElement
            = "The list contains a element which is not a ServiceElement.";


    }//class
}