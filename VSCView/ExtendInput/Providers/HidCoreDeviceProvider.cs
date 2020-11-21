using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ExtendInput.Providers
{
    [CoreDeviceProvider(TypeString = "HID", SupportsAutomaticDetection = true, SupportsManualyQuery = true, RequiresManualConfiguration = false)]
    public class HidCoreDeviceProvider : ICoreDeviceProvider
    {
        public event DeviceChangeEventHandler DeviceAdded;
        public event DeviceChangeEventHandler DeviceRemoved;

        HashSet<HidSharp.HidDevice> KnownDevices = new HashSet<HidSharp.HidDevice>();
        object lock_device_list = new object();

        public HidCoreDeviceProvider()
        {
            HidSharp.DeviceList.Local.Changed += DeviceListChanged;
        }

        public void ScanNow()
        {
            lock (lock_device_list)
            {
                try
                {
                    HashSet<HidSharp.Device> AllCurrentDevices = new HashSet<HidSharp.Device>(HidSharp.DeviceList.Local.GetHidDevices());

                    foreach (HidSharp.HidDevice device in KnownDevices.ToList())
                    {
                        if (!AllCurrentDevices.Contains(device))
                        {
                            //string FriendlyName = string.Empty;
                            //try
                            //{
                            //    FriendlyName = device.GetFriendlyName();
                            //}
                            //catch (IOException) { }
                            //Debug.WriteLine($"Device Removed: {device.DevicePath.PadRight(100)} \"{FriendlyName}\"");
                            //Debug.WriteLine($"Device Removed: {device.DevicePath.PadRight(100)} \"{device}\"");

                            KnownDevices.Remove(device);
                            DeviceChangeEventHandler threadSafeEventHandler = DeviceRemoved;
                            threadSafeEventHandler?.Invoke(this, new HidDevice(device));
                        }
                    }

                    foreach (HidSharp.HidDevice device in AllCurrentDevices.ToList())
                    {
                        if (!KnownDevices.Contains(device))
                        {
                            //string FriendlyName = string.Empty;
                            //try
                            //{
                            //    FriendlyName = device.GetFriendlyName();
                            //}
                            //catch(IOException) { }
                            //Debug.WriteLine($"Device Added: {device.DevicePath.PadRight(100)} \"{FriendlyName}\"");
                            //Debug.WriteLine($"Device Added: {device.DevicePath.PadRight(100)} \"{device}\"");

                            KnownDevices.Add(device);
                            DeviceChangeEventHandler threadSafeEventHandler = DeviceAdded;
                            threadSafeEventHandler?.Invoke(this, new HidDevice(device));
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

    public delegate void DeviceChangeEventHandler(object sender, IDevice e);
    public interface ICoreDeviceProvider
    {
        event DeviceChangeEventHandler DeviceAdded;
        event DeviceChangeEventHandler DeviceRemoved;

        void ScanNow();
    }
    public class CoreDeviceProviderAttribute : Attribute
    {
        public string TypeString { get; set; }
        public bool SupportsAutomaticDetection { get; set; }
        public bool SupportsManualyQuery { get; set; }
        public bool RequiresManualConfiguration { get; set; }
    }
}
