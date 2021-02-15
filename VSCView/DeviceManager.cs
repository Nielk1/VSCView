using ExtendInput.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VSCView
{
    public class DeviceManager
    {
        private ExtendInput.DeviceManager deviceManager;

        public event ControllerChangeEventHandler ControllerAdded;
        public event DeviceChangeEventHandler ControllerRemoved;

        public DeviceManager()
        {
            deviceManager = new ExtendInput.DeviceManager();
            deviceManager.ControllerAdded += OnControllerAdded;
            deviceManager.ControllerRemoved += OnControllerRemoved;
        }

        private void OnControllerAdded(object sender, IController e)
        {
            ControllerChangeEventHandler threadSafeEventHandler = ControllerAdded;
            threadSafeEventHandler?.Invoke(this, e);
        }

        private void OnControllerRemoved(object sender, ExtendInput.DeviceProvider.IDevice e)
        {
            DeviceChangeEventHandler threadSafeEventHandler = ControllerRemoved;
            threadSafeEventHandler?.Invoke(this, e);
        }

        public void ScanNow()
        {
            deviceManager.ScanNow();
        }
    }

    public delegate void ControllerChangeEventHandler(object sender, IController e);
    public delegate void DeviceChangeEventHandler(object sender, ExtendInput.DeviceProvider.IDevice e);
}
