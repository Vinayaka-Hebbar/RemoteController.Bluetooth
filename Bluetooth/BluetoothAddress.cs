using System;
using System.Globalization;

namespace RemoteController.Bluetooth
{
    /// <summary>
    /// Represents a Bluetooth device address.
    /// </summary>
    [Serializable]
    public readonly struct BluetoothAddress : IComparable<BluetoothAddress>, IEquatable<BluetoothAddress>, IFormattable
    {
        [NonSerialized]
        private readonly byte[] data;

        /// <summary>
        /// Provides a null Bluetooth address.
        /// </summary>
        public static readonly BluetoothAddress None = new BluetoothAddress(0UL);

        /// <summary>
        /// Initializes a new instance of the BluetoothAddress class with the specified address.
        /// </summary>
        /// <param name="address">Int64 representation of the address.</param>
        [Obsolete("Use unsigned long for widest compatibility", false)]
        public BluetoothAddress(long address)
        {
            data = new byte[8];
            BitConverter.GetBytes(address).CopyTo(data, 0);
        }

        /// <summary>
        /// Initializes a new instance of the BluetoothAddress class with the specified address.
        /// </summary>
        /// <param name="address">UInt64 representation of the address.</param>
        public BluetoothAddress(ulong address)
        {
            data = new byte[8];
            BitConverter.GetBytes(address).CopyTo(data, 0);
        }

        public bool IsNull => data == null;

        /// <summary>
        /// Initializes a new instance of the BluetoothAddress class with the specified address.
        /// </summary>
        /// <param name="addressBytes">Address as 6 byte array.</param>
        public BluetoothAddress(byte[] address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (address.Length == 6 || address.Length == 8)
            {
                data = new byte[8];
                Buffer.BlockCopy(address, 0, data, 0, 6);
            }
            else
            {
                throw new ArgumentException("Address must be six bytes long.", "address");
            }
        }

        #region SAP
        /// <summary>
        /// Significant address part.
        /// </summary>
        public uint Sap
        {
            get
            {
                return BitConverter.ToUInt32(data, 0);
            }
        }
        #endregion

        #region NAP
        /// <summary>
        /// Non-significant address part.
        /// </summary>
        public ushort Nap
        {
            get
            {
                return BitConverter.ToUInt16(data, 4);
            }
        }
        #endregion


        public static implicit operator ulong(BluetoothAddress address)
        {
            return BitConverter.ToUInt64(address.data, 0);
        }

        public static implicit operator BluetoothAddress(ulong address)
        {
            return new BluetoothAddress(address);
        }

        /// <summary>
        /// Returns the value as a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return data;
        }

        /// <summary>
        /// Compares two BluetoothAddress instances for equality.
        /// </summary>
        /// <param name="obj">The BluetoothAddress to compare with the current instance.</param>
        /// <returns>true if obj is a BluetoothAddress and equal to the current instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is BluetoothAddress bta)
            {
                return data == bta.data;
            }

            return base.Equals(obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return ((ulong)this).GetHashCode();
        }

        public int CompareTo(BluetoothAddress other)
        {
            if (other != null)
            {
                ulong data = this;
                return data.CompareTo(other);
            }

            return -1;
        }

        /// <summary>
        /// Returns a String representation of the value of this BluetoothAddress instance, according to the provided format specifier.
        /// </summary>
        /// <param name="format">A single format specifier that indicates how to format the value of this address. 
        /// The format parameter can be "N", "C", or "P". 
        /// If format is null or the empty string (""), "N" is used.</param>
        /// <param name="formatProvider">Ignored.</param>
        /// <returns>A String representation of the value of this BluetoothAddress.</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString(format);
        }

        /// <summary>
        /// Returns a String representation of the value of this BluetoothAddress instance, according to the provided format specifier.
        /// </summary>
        /// <param name="format">A single format specifier that indicates how to format the value of this address. 
        /// The format parameter can be "N", "C", or "P". 
        /// If format is null or the empty string (""), "N" is used.</param>
        /// <returns>A String representation of the value of this BluetoothAddress.</returns>
        public string ToString(string format)
        {
            string separator;

            if (string.IsNullOrEmpty(format))
            {
                separator = string.Empty;
            }
            else
            {
                switch (format.ToUpper(CultureInfo.InvariantCulture))
                {
                    case "8":
                    case "N":
                        separator = string.Empty;
                        break;
                    case "C":
                        separator = ":";
                        break;
                    case "P":
                        separator = ".";
                        break;
                    default:
                        throw new FormatException("Invalid format specified - must be either \"N\", \"C\", \"P\", \"\" or null.");
                }
            }

            byte[] data = ToByteArray();

            System.Text.StringBuilder result = new System.Text.StringBuilder(18);

            if (format == "8")
            {
                result.Append(data[7].ToString("X2") + separator);
                result.Append(data[6].ToString("X2") + separator);
            }

            result.Append(data[5].ToString("X2") + separator);
            result.Append(data[4].ToString("X2") + separator);
            result.Append(data[3].ToString("X2") + separator);
            result.Append(data[2].ToString("X2") + separator);
            result.Append(data[1].ToString("X2") + separator);
            result.Append(data[0].ToString("X2"));

            return result.ToString();
        }

        public override string ToString()
        {
            return ToString("N");
        }

        /// <summary>
        /// Converts the string representation of an address to it's <see cref="BluetoothAddress"/> equivalent.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="bluetoothString">A string containing an address to convert.</param>
        /// <param name="result">When this method returns, contains the <see cref="BluetoothAddress"/> equivalent to the address contained in s, if the conversion succeeded, or null (Nothing in Visual Basic) if the conversion failed.
        /// The conversion fails if the s parameter is null or is not of the correct format.</param>
        /// <returns>true if s is a valid Bluetooth address; otherwise, false.</returns>
        public static bool TryParse(string bluetoothString, out BluetoothAddress result)
        {
            Exception ex = ParseInternal(bluetoothString, out result);
            if (ex != null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Converts the string representation of a Bluetooth address to a new <see cref="BluetoothAddress"/> instance.
        /// </summary>
        /// <param name="bluetoothString">A string containing an address to convert.</param>
        /// <returns>New <see cref="BluetoothAddress"/> instance.</returns>
        /// <remarks>Address must be specified in hex format optionally separated by the colon or period character e.g. 000000000000, 00:00:00:00:00:00 or 00.00.00.00.00.00.</remarks>
        /// <exception cref="T:System.ArgumentNullException">bluetoothString is null.</exception>
        /// <exception cref="T:System.FormatException">bluetoothString is not a valid Bluetooth address.</exception>
        public static BluetoothAddress Parse(string bluetoothString)
        {
            Exception ex = ParseInternal(bluetoothString, out BluetoothAddress result);
            if (ex != null)
                throw ex;
            else
                return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Returned to caller.")]
        static Exception ParseInternal(string bluetoothString, out BluetoothAddress result)
        {
            const Exception Success = null;
            result = None;

            if (string.IsNullOrEmpty(bluetoothString))
            {
                return new ArgumentNullException(nameof(bluetoothString));
            }

            if (bluetoothString.IndexOf(":", StringComparison.Ordinal) > -1)
            {
                // assume address in standard hex format 00:00:00:00:00:00

                // check length
                if (bluetoothString.Length != 17)
                {
                    return new FormatException("bluetoothString is not a valid Bluetooth address.");
                }

                try
                {
                    byte[] babytes = new byte[8];
                    // split on colons
                    string[] sbytes = bluetoothString.Split(':');
                    for (int ibyte = 0; ibyte < 6; ibyte++)
                    {
                        // parse hex byte in reverse order
                        babytes[ibyte] = byte.Parse(sbytes[5 - ibyte], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    }
                    result = new BluetoothAddress(babytes);
                    return Success;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }
            else if (bluetoothString.IndexOf(".", StringComparison.Ordinal) > -1)
            {
                // assume address in uri hex format 00.00.00.00.00.00
                // check length
                if (bluetoothString.Length != 17)
                {
                    return new FormatException("bluetoothString is not a valid Bluetooth address.");
                }

                try
                {
                    byte[] babytes = new byte[8];
                    // split on periods
                    string[] sbytes = bluetoothString.Split('.');
                    for (int ibyte = 0; ibyte < 6; ibyte++)
                    {
                        // parse hex byte in reverse order
                        babytes[ibyte] = byte.Parse(sbytes[5 - ibyte], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    }
                    result = new BluetoothAddress(babytes);
                    return Success;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }
            else
            {
                // assume specified as long integer
                if ((bluetoothString.Length < 12) | (bluetoothString.Length > 16))
                {
                    return new FormatException("bluetoothString is not a valid Bluetooth address.");
                }
                try
                {
                    result = new BluetoothAddress(ulong.Parse(bluetoothString, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                    return Success;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }
        }

        public bool Equals(BluetoothAddress other)
        {
            if (other.IsNull)
                return false;

            return data == other.data;
        }

        public static bool operator ==(BluetoothAddress x, BluetoothAddress y)
        {
            if ((x.IsNull) && (y.IsNull))
            {
                return true;
            }

            if (x.data == y.data)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns an indication whether the values of two specified <see cref="BluetoothAddress"/> objects are not equal.
        /// </summary>
        /// -
        /// <param name="x">A <see cref="BluetoothAddress"/> or <see langword="null"/>.</param>
        /// <param name="y">A <see cref="BluetoothAddress"/> or <see langword="null"/>.</param>
        /// -
        /// <returns><c>true</c> if the value of the two instance is different;
        /// otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(BluetoothAddress x, BluetoothAddress y)
        {
            return !(x == y);
        }
    }
}
