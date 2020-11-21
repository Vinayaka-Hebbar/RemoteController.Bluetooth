using RemoteController.Win32.Hooks;
using System;
using System.Collections.Generic;

namespace RemoteController.Core
{
    public class ServerEventDispatcher
    {
        private readonly ServerConnectionManager _manager;

        public ServerEventDispatcher(ServerConnectionManager manager)
        {
            _manager = manager;
        }

        public void MoveScreenRight()
        {
            if (!_manager.IsConnected)
                return;
            _manager.MoveScreenRight().ContinueWith(task1 =>
            {
                if (task1.IsFaulted)
                {
                    Console.WriteLine("There was an error calling send: {0}", task1.Exception.GetBaseException());
                }

            });
        }
        public void MoveScreenLeft()
        {
            if (!_manager.IsConnected)
                return;
            _manager.MoveScreenLeft().ContinueWith(task1 =>
            {
                if (task1.IsFaulted)
                {
                    Console.WriteLine("There was an error calling send: {0}", task1.Exception.GetBaseException());
                }

            });
        }

        public void Clipboard(string value)
        {
            if (!_manager.IsConnected)
                return;
            _manager.Clipboard(value).ContinueWith(task1 =>
            {
                if (task1.IsFaulted)
                {
                    Console.WriteLine("There was an error calling send: {0}", task1.Exception.GetBaseException());
                }

            });
        }

        public void MouseWheel(int deltaX, int deltaY)
        {
            if (!_manager.IsConnected)
                return;
            _manager.MouseWheel(deltaX, deltaY).ContinueWith(task1 =>
            {
                if (task1.IsFaulted)
                {
                    Console.WriteLine("There was an error calling send: {0}", task1.Exception.GetBaseException());
                }

            });
        }

        public void MouseDown(MouseButton button)
        {
            if (!_manager.IsConnected)
                return;
            _manager.MouseDown(button).ContinueWith(task1 =>
            {
                if (task1.IsFaulted)
                {
                    Console.WriteLine("There was an error calling send: {0}", task1.Exception.GetBaseException());
                }
                else
                {

                }
            });
        }

        public void MouseUp(MouseButton button)
        {
            if (!_manager.IsConnected)
                return;
            _manager.MouseUp(button).ContinueWith(task1 =>
            {
                if (task1.IsFaulted)
                {
                    Console.WriteLine("There was an error calling send: {0}", task1.Exception.GetBaseException());
                }
                else
                {

                }
            });
        }

        public void MouseMove(double virtualX, double virtualY)
        {
            if (!_manager.IsConnected)
                return;
            _manager.MouseMove(virtualX, virtualY).ContinueWith(task1 =>
            {
                if (task1.IsFaulted)
                {
                    Console.WriteLine("There was an error calling send: {0}", task1.Exception.GetBaseException());
                }
                else
                {

                }
            });
        }

        public void KeyDown(Key key)
        {
            if (!_manager.IsConnected)
                return;
            _manager.KeyDown(key).ContinueWith(task1 =>
            {
                if (task1.IsFaulted)
                {
                    Console.WriteLine("There was an error calling send: {0}", task1.Exception.GetBaseException());
                }
                else
                {

                }
            });
        }

        public void KeyUp(Key key)
        {
            if (!_manager.IsConnected)
                return;
            _manager.KeyUp(key).ContinueWith(task1 =>
            {
                if (task1.IsFaulted)
                {
                    Console.WriteLine("There was an error calling send: {0}", task1.Exception.GetBaseException());
                }
                else
                {

                }
            });
        }

        public void ClientCheckin(string clientName, IList<VirtualScreen> screens)
        {
            //check in this client
            if (!_manager.IsConnected)
                return;
            _manager.ClientCheckin(clientName, screens).ContinueWith(task1 =>
            {
                if (task1.IsFaulted)
                {
                    Console.WriteLine("There was an error calling send: {0}", task1.Exception.GetBaseException());

                }
                else
                {

                }
            }).Wait();
        }
    }
}