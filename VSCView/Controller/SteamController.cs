using ExtendInput.DeviceProvider;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using VSCView;

namespace VSCView.Controller
{
    public class SteamController : IController
    {
        public bool HasMotion => _controller.HasMotion;

        public bool SensorsEnabled;
        private HidDevice _device;
        int stateUsageLock = 0, reportUsageLock = 0;

        public ExtendInput.Controls.ControllerState GetState()
        {
            return _controller.GetState();
        }

#if Serial
        public string Serial { get; private set; }
#endif

        private ExtendInput.Controller.IController _controller;
        public IDevice DeviceHackRef => _controller.DeviceHackRef;
        

        public delegate void StateUpdatedEventHandler(object sender, ExtendInput.Controls.ControllerState e);
        public event StateUpdatedEventHandler StateUpdated;
        protected virtual void OnStateUpdated(ExtendInput.Controls.ControllerState e)
        {
            StateUpdated?.Invoke(this, e);
        }

        // TODO for now it is safe to assume the startup connection type is correct, however, in the future we will need to have connection events trigger a recheck of the type or something once the V2 controller is out (if ever)
        public SteamController(ExtendInput.Controller.SteamController controller)
        {
            _controller = controller;

            //_device.ControllerNameUpdated += OnReport;
        }

        public void Initalize()
        {
            _controller.Initalize();
        }

        public event ControllerNameUpdateEvent ControllerNameUpdated;

        public void DeInitalize()
        {
            _controller.DeInitalize();
        }

        public void Identify()
        {
            _controller.Identify();
        }

        public string GetName()
        {
            return _controller.GetName();
        }

        public Image GetIcon()
        {
            Image Icon = new Bitmap(32 + 4, 16);
            Graphics g = Graphics.FromImage(Icon);

            switch (_controller.ConnectionType)
            {
                case ExtendInput.Controller.EConnectionType.Dongle: g.DrawImage(VSCView.Properties.Resources.icon_wireless, 0, 0, 16, 16); break;
                case ExtendInput.Controller.EConnectionType.USB: g.DrawImage(VSCView.Properties.Resources.icon_usb, 0, 0, 16, 16); break;
                case ExtendInput.Controller.EConnectionType.Bluetooth: g.DrawImage(VSCView.Properties.Resources.icon_bt, 0, 0, 16, 16); break;
            }

            switch((_controller as ExtendInput.Controller.SteamController).ControllerType)
            {
                case ExtendInput.Controller.SteamController.EControllerType.ReleaseV1: g.DrawImage(VSCView.Properties.Resources.icon_sc, 16 + 4, 0, 16, 16); break;
                case ExtendInput.Controller.SteamController.EControllerType.ReleaseV2: g.DrawImage(VSCView.Properties.Resources.icon_sc, 16 + 4, 0, 16, 16); break;
                case ExtendInput.Controller.SteamController.EControllerType.Chell: g.DrawImage(VSCView.Properties.Resources.icon_chell, 16 + 4, 0, 16, 16); break;
            }

            return Icon;
        }

        /*private void DeviceAttachedHandler()
        {
            lock (controllerStateLock)
            {
                _attached = true;
                Console.WriteLine("VSC Address Attached");
                _device.ReadReport(OnReport);
            }
        }

        private void DeviceRemovedHandler()
        {
            lock (controllerStateLock)
            {
                _attached = false;
                Console.WriteLine("VSC Address Removed");
            }
        }*/
    }
}
