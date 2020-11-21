using ExtendInput.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendInput.Providers
{
    public class XInputDevice : IDevice
    {
        public string DevicePath { get { return $"SharpDX.XInput.Controller({internalDevice.UserIndex})"; } }// internalDevice.DevicePath; } }
        public int ProductId { get { return 0; } }//internalDevice.ProductID; } }
        public int VendorId { get { return 0; } }//internalDevice.VendorID; } }



        private SharpDX.XInput.Controller internalDevice;
        private bool IsOpen = false;

        public XInputDevice(SharpDX.XInput.Controller internalDevice)
        {
            this.internalDevice = internalDevice;
        }

        public bool WriteReport(byte[] data)
        {
            //try
            {
                //GetStream().Write(data);
                //return true;
            }
            //catch
            {
                return false;
            }
        }

        public bool WriteFeatureData(byte[] data)
        {
            //try
            //{
                //int maxLen = internalDevice.GetMaxFeatureReportLength();
                //GetStream().SetFeature(data);
            //    return true;
            //}
            //catch
            {
                return false;
            }
        }

        public bool ReadFeatureData(out byte[] data, byte reportId = 0)
        {
            data = new byte[0];
            /*data = new byte[internalDevice.GetMaxFeatureReportLength()];
            try
            {
                data[0] = reportId;
                byte[] buffer = new byte[data.Length];
                GetStream().GetFeature(data);
                return true;
            }
            catch*/
            {
                return false;
            }
        }

        public void Dispose()
        {
            //if (MonitorDeviceEvents) MonitorDeviceEvents = false;
            //if (IsOpen) CloseDevice();
        }

        public string ReadSerialNumber()
        {
            return string.Empty;// internalDevice.GetSerialNumber();
        }

        bool reading = false;
        object readingLock = new object();
        Thread readingThread = null;
        public void StartReading()
        {
            lock (readingLock)
            {
                if (ControllerNameUpdated == null)
                    reading = false;

                if (reading)
                    return;

                reading = true;

                readingThread = new Thread(() =>
                {
                    while (reading)
                    {
                        if (ControllerNameUpdated == null)
                        {
                            break;
                        }

                        try
                        {
                            var State = internalDevice.GetState();

                            DeviceReportEvent threadSafeEvent = ControllerNameUpdated;

                            byte[] RawData = new byte[12];
                            RawData[0] = (byte)(ushort)State.Gamepad.Buttons;
                            RawData[1] = (byte)((ushort)State.Gamepad.Buttons >> 8);
                            RawData[2] = State.Gamepad.LeftTrigger;
                            RawData[3] = State.Gamepad.RightTrigger;
                            RawData[4] = (byte)State.Gamepad.LeftThumbX;
                            RawData[5] = (byte)(State.Gamepad.LeftThumbX >> 8);
                            RawData[6] = (byte)State.Gamepad.LeftThumbY;
                            RawData[7] = (byte)(State.Gamepad.LeftThumbY >> 8);
                            RawData[8] = (byte)State.Gamepad.RightThumbX;
                            RawData[9] = (byte)(State.Gamepad.RightThumbX >> 8);
                            RawData[10] = (byte)State.Gamepad.RightThumbY;
                            RawData[11] = (byte)(State.Gamepad.RightThumbY >> 8);

                            threadSafeEvent?.Invoke(RawData, 0);

                            Thread.Sleep(1000 / 60);
                        }
                        catch
                        {
                            reading = false;
                        }
                    }
                    reading = false;
                });
                readingThread.Start();
            }
        }

        public void StopReading()
        {
            lock (readingLock)
            {
                reading = false;
            }
        }

        public string UniqueKey => $"{this.GetType().UnderlyingSystemType.GUID} {this.DevicePath}";

        bool IEquatable<IDevice>.Equals(IDevice other)
        {
            return this.UniqueKey == other.UniqueKey;
        }

        public event DeviceReportEvent ControllerNameUpdated;
    }
}
