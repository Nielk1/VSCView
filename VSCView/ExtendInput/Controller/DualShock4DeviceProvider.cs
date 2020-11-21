using ExtendInput.Providers;
using System.Linq;
using VSCView;

namespace ExtendInput.Controller
{
    public class DualShock4DeviceProvider : IDeviceProvider
    {
        public IController NewDevice(IDevice device)
        {
            HidDevice _device = device as HidDevice;

            if (_device == null)
                return null;

            if (_device.VendorId != DualShock4Controller.VendorId)
                return null;

            if (!new int[] {
                DualShock4Controller.ProductIdDongle,
                DualShock4Controller.ProductIdWired,
                DualShock4Controller.ProductIdWiredV2,
            }.Contains(_device.ProductId))
                return null;

            string bt_hid_id = @"00001124-0000-1000-8000-00805f9b34fb";

            string devicePath = _device.DevicePath.ToString();

            EConnectionType ConType = EConnectionType.Unknown;
            switch (_device.ProductId)
            {
                case DualShock4Controller.ProductIdWired:
                case DualShock4Controller.ProductIdWiredV2:
                    if (devicePath.Contains(bt_hid_id))
                    {
                        ConType = EConnectionType.Bluetooth;
                    }
                    else
                    {
                        ConType = EConnectionType.USB;
                    }
                    break;
                case DualShock4Controller.ProductIdDongle:
                    ConType = EConnectionType.Dongle;
                    break;
            }

            DualShock4Controller ctrl = new DualShock4Controller(_device, ConType);
            ctrl.HalfInitalize();
            return ctrl;
        }
    }
}
