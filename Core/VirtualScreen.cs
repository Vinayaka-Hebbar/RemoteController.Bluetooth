using RemoteController.Win32.Hooks;

namespace RemoteController.Core
{
    public class VirtualScreen
    {
        public static readonly VirtualScreen Empty = new VirtualScreen(string.Empty, Dpi.Default);
        
        public VirtualScreen(string client, Dpi dpi)
        {
            Client = client;
            Dpi = dpi;
            ScaleX = dpi.X / Dpi.DefaultDpi;
            ScaleY = dpi.Y / Dpi.DefaultDpi;
        }
        
        public int LocalX { get; set; }
        public int LocalY { get; set; }
        public string Client { get; }

        public int ScaleX { get; }
        public int ScaleY { get; }

        public Dpi Dpi { get; }

        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

}
