namespace RemoteController.Win32.Hooks
{
    public readonly struct MousePoint
    {
        public int X { get; }
        public int Y { get; }

        public MousePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

    }
}
