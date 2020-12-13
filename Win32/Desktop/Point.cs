using System.Runtime.InteropServices;

namespace RemoteController.Win32.Desktop
{
    /// <summary>
    /// Win API struct providing coordinates for a single point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        /// <summary>
        /// X coordinate.
        /// </summary>
        public int X;
        /// <summary>
        /// Y coordinate.
        /// </summary>
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public System.Windows.Point ToPoint()
        {
            return new System.Windows.Point(X, Y);
        }
    }
}
