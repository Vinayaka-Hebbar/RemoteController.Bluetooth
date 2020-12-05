namespace RemoteController.Win32.Hooks
{
    public class MousePoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        public MousePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

    }
}
