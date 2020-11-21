namespace RemoteController.Win32.Hooks
{
    /// <summary>
    /// Represents the state of a mouse wheel.
    /// </summary>
    public struct MouseScroll : System.IEquatable<MouseScroll>
    {
        /// <summary>
        /// Gets the absolute horizontal offset of the wheel,
        /// or 0 if no horizontal scroll wheel exists.
        /// </summary>
        /// <value>The x.</value>
        public float X { get; internal set; }

        /// <summary>
        /// Gets the absolute vertical offset of the wheel,
        /// or 0 if no vertical scroll wheel exists.
        /// </summary>
        /// <value>The y.</value>
        public float Y { get; internal set; }

        /// <param name="left">A <see cref="MouseScroll"/> instance to test for equality.</param>
        /// <param name="right">A <see cref="MouseScroll"/> instance to test for equality.</param>
        public static bool operator ==(MouseScroll left, MouseScroll right)
        {
            return left.Equals(right);
        }

        /// <param name="left">A <see cref="MouseScroll"/> instance to test for inequality.</param>
        /// <param name="right">A <see cref="MouseScroll"/> instance to test for inequality.</param>
        public static bool operator !=(MouseScroll left, MouseScroll right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="MouseScroll"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="MouseScroll"/>.</returns>
        public override string ToString()
        {
            return string.Format("[X={0:0.00}, Y={1:0.00}]", X, Y);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="MouseScroll"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="MouseScroll"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="MouseScroll"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="MouseScroll"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return
                obj is MouseScroll &&
                Equals((MouseScroll)obj);
        }

        /// <summary>
        /// Determines whether the specified <see cref="MouseScroll"/> is equal to the current <see cref="MouseScroll"/>.
        /// </summary>
        /// <param name="other">The <see cref="MouseScroll"/> to compare with the current <see cref="MouseScroll"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="MouseScroll"/> is equal to the current
        /// <see cref="MouseScroll"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(MouseScroll other)
        {
            return X == other.X && Y == other.Y;
        }
    }
}