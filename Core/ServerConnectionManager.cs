using RemoteController.Bluetooth;
using RemoteController.Win32.Hooks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteController.Core
{
    public class ServerConnectionManager : IDisposable
    {
        private readonly BluetoothEndPoint endPoint;

        public ServerConnectionManager(BluetoothEndPoint endPoint)
        {
            this.endPoint = endPoint;
        }

        public bool IsConnected { get; private set; }

        public Task MoveScreenRight()
        {
            throw new NotImplementedException();
        }

        public Task MoveScreenLeft()
        {
            throw new NotImplementedException();
        }

        public Task MouseWheel(int deltaX, int deltaY)
        {
            throw new NotImplementedException();
        }

        internal Task<object> MouseDown(MouseButton button)
        {
            throw new NotImplementedException();
        }

        internal Task<object> MouseUp(MouseButton button)
        {
            throw new NotImplementedException();
        }

        internal void Start()
        {
            throw new NotImplementedException();
        }

        internal Task<object> MouseMove(double virtualX, double virtualY)
        {
            throw new NotImplementedException();
        }

        internal Task<object> Clipboard(string value)
        {
            throw new NotImplementedException();
        }

        internal Task<object> ClientCheckin(string clientName, IList<VirtualScreen> screens)
        {
            throw new NotImplementedException();
        }

        internal Task<object> KeyDown(Key key)
        {
            throw new NotImplementedException();
        }

        internal Task<object> KeyUp(Key key)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}