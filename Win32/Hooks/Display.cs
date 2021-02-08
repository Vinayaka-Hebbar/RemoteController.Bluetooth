namespace RemoteController.Win32.Hooks
{
    public readonly struct Display
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public uint ScaleX { get; }

        public uint ScaleY { get; }

        public Display(int x, int y, int width, int height, uint dpiX, uint dpiY)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            ScaleX = dpiX / 96;
            ScaleY = dpiY / 96;
        }

        public Display(int width, int height)
        {
            X = 0;
            Y = 0;
            Width = width;
            Height = height;
            ScaleX = 1;
            ScaleY = 1;
        }

        public override string ToString()
        {
            return $"{Width}x{Height}";
        }
    }
}
