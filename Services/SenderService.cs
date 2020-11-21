using RemoteController.Bluetooth;
using RemoteController.Model;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace RemoteController.Services
{
    public class SenderService : TaskService
    {
        public readonly Device Device;
        public SenderService(Device device, Guid id) : base(id)
        {
            Device = device;
        }

        protected override void DoWork(CancellationToken cancellationToken)
        {
            BluetoothClient client = new BluetoothClient();
            BluetoothEndPoint ep = new BluetoothEndPoint(Device.DeviceInfo.DeviceAddress, Id);
            TextReader stream = TextReader.Synchronized(Console.In);
            try
            {
                // connecting
                client.Connect(ep);
                char[] buffer = new char[1024];
                while (true)
                {
                    try
                    {
                        // if all is ok to send
                        if (client.Connected == false)
                        {
                            break;
                            // acknowledge
                            // write the data in the stream
                            //var buffer = System.Text.Encoding.UTF8.GetBytes(content);
                            //client.Socket.Send(buffer, 0, buffer.Length, System.Net.Sockets.SocketFlags.None);
                        }
                        if (stream.Read(buffer, 0, 1024) > 0)
                        {

                        }
                    }
                    catch (SocketException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        // the error will be ignored and the send data will report as not sent
                        // for understood the type of the error, handle the exception
                    }
                }
            }
            catch (SocketException) { }
            finally
            {
                client.Close();
            }
        }
    }
}
