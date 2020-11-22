namespace RemoteController.Core
{
    public class VirtualScreen
    {
        public VirtualScreen(string client)
        {
            Client = client;
        }

        public double LocalX { get; set; }
        public double LocalY { get; set; }
        public string Client { get; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }
}
