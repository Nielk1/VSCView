using ExtendInput.DeviceProvider;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using VSCView;

namespace VSCView.Controller
{
    public class DualSenseController : IController
    {
        int stateUsageLock = 0;
        public event ControllerNameUpdateEvent ControllerNameUpdated;
        public bool HasMotion => _controller.HasMotion;

        #region DATA STRUCTS

        public ExtendInput.Controls.ControllerState GetState()
        {
            return _controller.GetState();
        }
        #endregion

        private ExtendInput.Controller.IController _controller;
        public IDevice DeviceHackRef => _controller.DeviceHackRef;

        public delegate void StateUpdatedEventHandler(object sender, ExtendInput.Controls.ControllerState e);
        public event StateUpdatedEventHandler StateUpdated;
        protected virtual void OnStateUpdated(ExtendInput.Controls.ControllerState e)
        {
            StateUpdated?.Invoke(this, e);
        }

        public DualSenseController(ExtendInput.Controller.DualSenseController controller)
        {
            _controller = controller;

            //_device.ControllerNameUpdated += OnReport;
        }

        public void Initalize()
        {
            _controller.Initalize();
        }

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
                //case ExtendInput.Controller.EConnectionType.Dongle: g.DrawImage(VSCView.Properties.Resources.icon_ds4_dongle, 0, 0, 16, 16); break;
                case ExtendInput.Controller.EConnectionType.USB: g.DrawImage(VSCView.Properties.Resources.icon_usb, 0, 0, 16, 16); break;
                case ExtendInput.Controller.EConnectionType.Bluetooth: g.DrawImage(VSCView.Properties.Resources.icon_bt, 0, 0, 16, 16); break;
            }

            g.DrawImage(VSCView.Properties.Resources.icon_ds4, 16 + 4, 0, 16, 16);

            return Icon;
        }
    }
}
