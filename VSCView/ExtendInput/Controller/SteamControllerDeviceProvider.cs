using ExtendInput.Providers;
using System.Linq;
using VSCView;

namespace ExtendInput.Controller
{
    public class SteamControllerDeviceProvider : IDeviceProvider
    {
        public IController NewDevice(IDevice device)
        {
            HidDevice _device = device as HidDevice;

            if (_device == null)
                return null;

            if (_device.VendorId != SteamController.VendorId)
                return null;

            if (!new int[] {
                SteamController.ProductIdDongle,
                SteamController.ProductIdWired,
                SteamController.ProductIdBT,
                SteamController.ProductIdChell,
            }.Contains(_device.ProductId))
                return null;

            {
                string devicePath = _device.DevicePath.ToString();

                EConnectionType ConType = EConnectionType.Unknown;
                SteamController.EControllerType CtrlType = SteamController.EControllerType.ReleaseV1;
                switch (_device.ProductId)
                {
                    case SteamController.ProductIdBT:
                        if (!devicePath.Contains("col03")) return null; // skip anything that isn't the controller's custom HID device
                        ConType = EConnectionType.Bluetooth;
                        break;
                    case SteamController.ProductIdWired:
                        if (!devicePath.Contains("mi_02")) return null; // skip anything that isn't the controller's custom HID device
                        ConType = EConnectionType.USB;
                        break;
                    case SteamController.ProductIdDongle:
                        if (devicePath.Contains("mi_00")) return null; // skip the dongle itself
                        ConType = EConnectionType.Dongle;
                        break;
                    case SteamController.ProductIdChell:
                        ConType = EConnectionType.USB;
                        CtrlType = SteamController.EControllerType.Chell;
                        break;
                }

                SteamController ctrl = new SteamController(_device, ConType, CtrlType);
                ctrl.HalfInitalize();
                return ctrl;
            }
        }
    }
}
