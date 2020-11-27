﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VSCView.Controller;

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

        private void OnControllerAdded(object sender, ExtendInput.Controller.IController e)
        {
            IController d = null;
            if (e is ExtendInput.Controller.SteamController)
                d = new SteamController(e as ExtendInput.Controller.SteamController);
            if (e is ExtendInput.Controller.DualShock4Controller)
                d = new DualShock4Controller(e as ExtendInput.Controller.DualShock4Controller);
            if (e is ExtendInput.Controller.XInputController)
                d = new XInputController(e as ExtendInput.Controller.XInputController);
            //if (d == null)
            //    d = new GenericController(e);

            ControllerChangeEventHandler threadSafeEventHandler = ControllerAdded;
            threadSafeEventHandler?.Invoke(this, d);
        }

        private void OnControllerRemoved(object sender, ExtendInput.Providers.IDevice e)
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
    public delegate void DeviceChangeEventHandler(object sender, ExtendInput.Providers.IDevice e);
}