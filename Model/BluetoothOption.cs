using System;
using System.Net;

namespace RemoteController.Model
{
    public class BluetoothOption : IDeviceOption
    {
        public BluetoothOption(Bluetooth.BluetoothDeviceInfo deviceInfo)
        {
            DeviceName = deviceInfo.DeviceName;
            ServiceClass = deviceInfo.ClassOfDevice.Service;
            Address = deviceInfo.DeviceAddress;
            ServiceId = Device.ServiceClassId;
        }

        public BluetoothOption()
        {

        }

        public string ServiceName { get; set; }

        public string DeviceName { get; set; }

        public Guid ServiceId { get; set; }

        public ServiceClass ServiceClass { get; set; }

        public Bluetooth.BluetoothAddress Address { get; set; }

        public RemoteDeviceType Type => RemoteDeviceType.Bluetooth;

        public EndPoint GetEndPoint()
        {
            return new Bluetooth.BluetoothEndPoint(Address, ServiceId);
        }

        public override string ToString()
        {
            return DeviceName;
        }
    }
}
