namespace RemoteController.Core
{
    public class VirtualScreen
    {
        public static readonly VirtualScreen Empty = new VirtualScreen(string.Empty);

        public VirtualScreen(string client)
        {
            Client = client;
        }

        public int LocalX { get; set; }
        public int LocalY { get; set; }
        public string Client { get; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
