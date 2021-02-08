namespace RemoteController.Win32.Hooks
{
    public readonly struct Display
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public uint DpiX { get; }
        public uint DpiY { get; }

        public Display(int x, int y, int width, int height, uint dpiX, uint dpiY)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            DpiX = dpiX;
            DpiY = dpiY;
        }

        public Display(int width, int height)
        {
            X = 0;
            Y = 0;
            Width = width;
            Height = height;
            DpiX = 96;
            DpiY = 96;
        }

        public Dpi GetDpi()
        {
            return new Dpi((int)DpiX, (int)DpiY);
        }

        public override string ToString()
        {
            return $"{Width}x{Height}";
        }
    }


    public readonly struct Dpi
    {
        public const int DefaultDpi = 96;

        public static readonly Dpi Default = new Dpi(DefaultDpi, DefaultDpi);

        public readonly int X;
        public readonly int Y;

        public Dpi(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
