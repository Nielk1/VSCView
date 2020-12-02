using ExtendInput.DeviceProvider;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using VSCView;

namespace VSCView.Controller
{
    public class XInputController : IController
    {
        //public EConnectionType ConnectionType => EConnectionType.Unknown;

        private ExtendInput.Controller.IController _controller;
        int stateUsageLock = 0;

        public event ControllerNameUpdateEvent ControllerNameUpdated;
        public IDevice DeviceHackRef => _controller.DeviceHackRef;

        public bool HasMotion => _controller.HasMotion;

        int Initalized;
        public XInputController(ExtendInput.Controller.XInputController controller)
        {
            _controller = controller;

            //_device.ControllerNameUpdated += OnReport;
        }

        public Image GetIcon()
        {
            Image Icon = new Bitmap(32 + 4, 16);
            Graphics g = Graphics.FromImage(Icon);

            return Icon;
        }

        public string GetName()
        {
            return _controller.GetName();
        }

        #region DATA STRUCTS

        public ExtendInput.Controls.ControllerState GetState()
        {
            return _controller.GetState();
        }
        #endregion

        public void Identify()
        {
            
        }

        public void Initalize()
        {
            _controller.Initalize();
        }

        public void DeInitalize()
        {
            _controller.DeInitalize();
        }
    }
}