namespace RemoteController.Core
{
    public interface IDisplay
    {
        int Height { get; }
        int Width { get; }
        int X { get; }
        int Y { get; }
    }
}
