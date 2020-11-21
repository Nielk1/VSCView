using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ExtendInput.Providers
{
    [CoreDeviceProvider(TypeString = "XInput", SupportsAutomaticDetection = true, SupportsManualyQuery = true, RequiresManualConfiguration = false)]
    public class XInputCoreDeviceProvider : ICoreDeviceProvider
    {
        private const int MAX_SLOT = 4;

        public event DeviceChangeEventHandler DeviceAdded;
        public event DeviceChangeEventHandler DeviceRemoved;

        //HashSet<HidSharp.HidDevice> KnownDevices = new HashSet<HidSharp.HidDevice>();
        object lock_device_list = new object();
        SharpDX.XInput.Controller[] Controllers = new SharpDX.XInput.Controller[MAX_SLOT];
        bool[] ControllerActive = new bool[MAX_SLOT];

        public XInputCoreDeviceProvider()
        {
            HidSharp.DeviceList.Local.Changed += DeviceListChanged;
        }

        public void ScanNow()
        {
            lock (lock_device_list)
            {
                try
                {
                    for(int i=0;i< MAX_SLOT;i++)
                    {
                        if (Controllers[i] == null)
                            Controllers[i] = new SharpDX.XInput.Controller((SharpDX.XInput.UserIndex)i);

                        if(ControllerActive[i] != Controllers[i].IsConnected)
                        {
                            if(Controllers[i].IsConnected)
                            {
                                DeviceChangeEventHandler threadSafeEventHandler = DeviceAdded;
                                threadSafeEventHandler?.Invoke(this, new XInputDevice(Controllers[i]));
                            }
                            else
                            {
                                DeviceChangeEventHandler threadSafeEventHandler = DeviceRemoved;
                                threadSafeEventHandler?.Invoke(this, new XInputDevice(Controllers[i]));
                            }
                            ControllerActive[i] = Controllers[i].IsConnected;
                        }
                    }
                }
                catch { }
            }
        }

        private void DeviceListChanged(object sender, HidSharp.DeviceListChangedEventArgs e)
        {
            ScanNow();
        }

        /*public static IEnumerable<HidDevice> Enumerate(int vendorId, params int[] productIds)
        {
            return HidSharp.DeviceList.Local.GetHidDevices(vendorId).Where(dr => productIds.Contains(dr.ProductID)).Select(dr => new HidDevice(dr));
        }*/

        /*public void X()
        {
            HidSharp.DeviceList.Local.Changed
        }*/


    }
}