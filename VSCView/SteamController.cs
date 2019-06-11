using HidLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace VSCView
{
    public class SteamController
    {
        private const int VendorId = 0x28DE; // 10462
        private const int ProductIdWireless = 0x1142; // 4418;
        private const int ProductIdWired = 0x1102; // 4354
        private const int ProductIdChell = 0x1101; // 4353
        //private const int ProductIdBT = 0x1106; // 4358

        public bool SensorsEnabled;
        private HidDevice _device;
        int stateUsageLock = 0, reportUsageLock = 0;

        #region DATA STRUCTS
        public enum VSCEventType
        {
            CONTROL_UPDATE = 0x01,
            CONNECTION_DETAIL = 0x03,
            BATTERY_UPDATE = 0x04,
        }

        public enum ConnectionState
        {
            DISCONNECT = 0x01,
            CONNECT = 0x02,
            PAIRING = 0x03,
        }

        public enum Melody : UInt32
        {
            Warm_and_Happy = 0x00,
            Invader = 0x01,
            Controller_Confirmed = 0x02,
            Victory = 0x03,
            Rise_and_Shine = 0x04,
            Shorty = 0x05,
            Warm_Boot = 0x06,
            Next_Level = 0x07,
            Shake_it_off = 0x08,
            Access_Denied = 0x09,
            Deactivate = 0x0a,
            Discovery = 0x0b,
            Triumph = 0x0c,
            The_Mann = 0x0d,
        }

        public enum EControllerType
        {
            Chell,
            ReleaseV1,
            ReleaseV2,
        }

        public class SteamControllerButtons : ICloneable
        {
            public bool A { get; set; }
            public bool B { get; set; }
            public bool X { get; set; }
            public bool Y { get; set; }

            public bool LeftBumper { get; set; }
            public bool LeftTrigger { get; set; }

            public bool RightBumper { get; set; }
            public bool RightTrigger { get; set; }

            public bool LeftGrip { get; set; }
            public bool RightGrip { get; set; }

            public bool Start { get; set; }
            public bool Steam { get; set; }
            public bool Select { get; set; }

            public bool Down { get; set; }
            public bool Left { get; set; }
            public bool Right { get; set; }
            public bool Up { get; set; }

            public bool StickClick { get; set; }
            public bool LeftPadTouch { get; set; }
            public bool LeftPadClick { get; set; }
            public bool RightPadTouch { get; set; }
            public bool RightPadClick { get; set; }

            public bool Touch0 { get; set; }
            public bool Touch1 { get; set; }
            public bool Touch2 { get; set; }
            public bool Touch3 { get; set; }

            public virtual object Clone()
            {
                SteamControllerButtons buttons = (SteamControllerButtons)base.MemberwiseClone();

                buttons.A = A;
                buttons.B = B;
                buttons.X = X;
                buttons.Y = Y;

                buttons.LeftBumper = LeftBumper;
                buttons.LeftTrigger = LeftTrigger;

                buttons.RightBumper = RightBumper;
                buttons.RightTrigger = RightTrigger;

                buttons.LeftGrip = LeftGrip;
                buttons.RightGrip = RightGrip;

                buttons.Start = Start;
                buttons.Steam = Steam;
                buttons.Select = Select;

                buttons.Down = Down;
                buttons.Left = Left;
                buttons.Right = Right;
                buttons.Up = Up;

                buttons.StickClick = StickClick;
                buttons.LeftPadTouch = LeftPadTouch;
                buttons.LeftPadClick = LeftPadClick;
                buttons.RightPadTouch = RightPadTouch;
                buttons.RightPadClick = RightPadClick;

                buttons.Touch0 = Touch0;
                buttons.Touch1 = Touch1;
                buttons.Touch2 = Touch2;
                buttons.Touch3 = Touch3;

                return buttons;
            }
        }

        public class SteamControllerState
        {
            public SteamControllerButtons Buttons { get; set; }

            public byte LeftTrigger { get; set; }
            public byte RightTrigger { get; set; }

            public Int32 LeftStickX { get; set; }
            public Int32 LeftStickY { get; set; }
            public Int32 LeftPadX { get; set; }
            public Int32 LeftPadY { get; set; }
            public Int32 RightPadX { get; set; }
            public Int32 RightPadY { get; set; }

            public Int16 AccelerometerX { get; set; }
            public Int16 AccelerometerY { get; set; }
            public Int16 AccelerometerZ { get; set; }
            public Int16 AngularVelocityX { get; set; }
            public Int16 AngularVelocityY { get; set; }
            public Int16 AngularVelocityZ { get; set; }
            public Int16 OrientationW { get; set; }
            public Int16 OrientationX { get; set; }
            public Int16 OrientationY { get; set; }
            public Int16 OrientationZ { get; set; }

            public bool DataStuck { get; set; }
        }

        public SteamControllerState GetState()
        {
            if (0 == Interlocked.Exchange(ref stateUsageLock, 1))
            {
                SteamControllerState newState = new SteamControllerState();
                newState.Buttons = (SteamControllerButtons)State.Buttons.Clone();

                newState.LeftTrigger = State.LeftTrigger;
                newState.RightTrigger = State.RightTrigger;

                newState.LeftStickX = State.LeftStickX;
                newState.LeftStickY = State.LeftStickY;
                newState.LeftPadX = State.LeftPadX;
                newState.LeftPadY = State.LeftPadY;
                newState.RightPadX = State.RightPadX;
                newState.RightPadY = State.RightPadY;

                newState.AccelerometerX = State.AccelerometerX;
                newState.AccelerometerY = State.AccelerometerY;
                newState.AccelerometerZ = State.AccelerometerZ;
                newState.AngularVelocityX = State.AngularVelocityX;
                newState.AngularVelocityY = State.AngularVelocityY;
                newState.AngularVelocityZ = State.AngularVelocityZ;
                newState.OrientationW = State.OrientationW;
                newState.OrientationX = State.OrientationX;
                newState.OrientationY = State.OrientationY;
                newState.OrientationZ = State.OrientationZ;

                //newState.DataStuck = State.DataStuck;

                State = newState;
                Interlocked.Exchange(ref stateUsageLock, 0);
            }
            return State;
        }
        #endregion

        public enum EConnectionType
        {
            Unknown,
            Wireless,
            USB,
            BT,
            Chell,
        }

        SteamControllerState State = new SteamControllerState();
        SteamControllerState OldState = new SteamControllerState();

        bool Initalized;

        public EConnectionType ConnectionType { get; private set; }
        public EControllerType ControllerType { get; private set; }

        public delegate void StateUpdatedEventHandler(object sender, SteamControllerState e);
        public event StateUpdatedEventHandler StateUpdated;
        protected virtual void OnStateUpdated(SteamControllerState e)
        {
            StateUpdated?.Invoke(this, e);
        }

        public SteamController(HidDevice device, EConnectionType connection = SteamController.EConnectionType.Unknown, EControllerType type = EControllerType.ReleaseV1)
        {
            State.Buttons = new SteamControllerButtons();

            _device = device;
            ConnectionType = connection;
            ControllerType = type;

            Initalized = false;
        }

        public void Initalize()
        {
            if (Initalized) return;

            // open the device overlapped read so we don't get stuck waiting for a report when we write to it
            _device.OpenDevice(DeviceMode.Overlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead | ShareMode.ShareWrite);

            //_device.Inserted += DeviceAttachedHandler;
            //_device.Removed += DeviceRemovedHandler;

            //_device.MonitorDeviceEvents = true;

            Initalized = true;

            //_attached = _device.IsConnected;

            _device.ReadReport(OnReport);
        }

        public void DeInitalize()
        {
            if (!Initalized) return;

            //_device.Inserted -= DeviceAttachedHandler;
            //_device.Removed -= DeviceRemovedHandler;

            //_device.MonitorDeviceEvents = false;

            Initalized = false;
            _device.CloseDevice();
        }

        public bool EnableGyroSensors()
        {
            if (!SensorsEnabled)
            {
                byte[] reportData = new byte[64];
                reportData[1] = 0x87; // 0x87 = register write command
                reportData[2] = 0x03; // 0x03 = length of data to be written (data + 1 empty bit)
                reportData[3] = 0x30; // 0x30 = register of Gyro data
                reportData[4] = 0x10 | 0x08 | 0x04; // enable raw Gyro, raw Accel, and Quaternion data
                Debug.WriteLine("Attempting to reenable MPU accelerometer sensor");
                var result = _device.WriteFeatureData(reportData);
                SensorsEnabled = true;
                return result;
            }
            return false;
        }

        public bool ResetGyroSensors()
        {
            if (SensorsEnabled)
            {
                byte[] reportData = new byte[64];
                reportData[1] = 0x87; // 0x87 = register write command
                reportData[2] = 0x03; // 0x03 = length of data to be written (data + 1 empty bit)
                reportData[3] = 0x30; // 0x30 = register of Gyro data
                reportData[4] = 0x10 | 0x04; // enable raw Gyro, raw Accel, and Quaternion data
                Debug.WriteLine("Attempting to restore default sensor state");
                var result = _device.WriteFeatureData(reportData);
                SensorsEnabled = false;
                return result;
            }
            return false;
        }

        public bool PlayMelody(Melody melody)
        {
            byte[] reportData = new byte[64];
            reportData[1] = 0xB6; // 0xB6 = play melody
            reportData[2] = 0x04; // 0x04 = length of data to be written
            byte[] data = BitConverter.GetBytes((UInt32)melody);
            reportData[3] = data[0];
            reportData[4] = data[1];
            reportData[5] = data[2];
            reportData[6] = data[3];
            Debug.WriteLine("Triggering Melody");
            
            var result = _device.WriteFeatureData(reportData);
            return result;
        }

        public bool CheckSensorDataStuck()
        {
            return (OldState != null &&
                State.AccelerometerX == 0 &&
                State.AccelerometerY == 0 &&
                State.AccelerometerZ == 0 ||
                State.AccelerometerX == OldState.AccelerometerX &&
                State.AccelerometerY == OldState.AccelerometerY &&
                State.AccelerometerZ == OldState.AccelerometerZ ||
                State.AngularVelocityX == OldState.AngularVelocityX &&
                State.AngularVelocityY == OldState.AngularVelocityY &&
                State.AngularVelocityZ == OldState.AngularVelocityZ
            );
        }

        public string GetDevicePath()
        {
            return _device.DevicePath;
        }

        public static SteamController[] GetControllers()
        {
            List<HidDevice> _devices = HidDevices.Enumerate(VendorId, ProductIdWireless, ProductIdWired/*, ProductIdBT*/, ProductIdChell).ToList();
            List<SteamController> ControllerList = new List<SteamController>();
            string wired_m = "&pid_1102&mi_02";
            //string dongle_m = "&pid_1142&mi_01";
            string dongle_m1 = "&pid_1142&mi_01";
            string dongle_m2 = "&pid_1142&mi_02";
            string dongle_m3 = "&pid_1142&mi_03";
            string dongle_m4 = "&pid_1142&mi_04";
            //string bt_m = "_PID&1106_";
            string chell_m = "&pid_1101";
            // we should never have holes, this entire dictionary is just because I don't know if I can trust the order I get the HID devices
            for (int i = 0; i < _devices.Count; i++)
            {
                if (_devices[i] != null)
                {
                    HidDevice _device = _devices[i];
                    string devicePath = _device.DevicePath.ToString();

                    if (devicePath.Contains(wired_m))
                    {
                        ControllerList.Add(new SteamController(_device, EConnectionType.USB, EControllerType.ReleaseV1));
                    }
                    else if (devicePath.Contains(dongle_m1)
                          || devicePath.Contains(dongle_m2)
                          || devicePath.Contains(dongle_m3)
                          || devicePath.Contains(dongle_m4))
                    {
                        ControllerList.Add(new SteamController(_device, EConnectionType.Wireless, EControllerType.ReleaseV1));
                    }
                    //else// if (devicePath.Contains(bt_m))
                    //{
                    //    ControllerList.Add(new SteamController(_device, EConnectionType.BT));
                    //}
                    else if (devicePath.Contains(chell_m))
                    {
                        ControllerList.Add(new SteamController(_device, EConnectionType.Chell, EControllerType.Chell));
                    }
                }
            }

            return ControllerList.OrderByDescending(dr => dr.ConnectionType).ThenBy(dr => dr.GetDevicePath()).ToArray();
        }

        private void OnReport(HidReport report)
        {
            if (!Initalized) return;

            if (0 == Interlocked.Exchange(ref reportUsageLock, 1))
            {
                OldState = State;
                //if (_attached == false) { return; }

                switch (ConnectionType)
                {
                    case EConnectionType.BT:
                        {
                            byte Unknown1 = report.Data[0]; // always 0xC0?
                            VSCEventType EventType = (VSCEventType)report.Data[1];

                            Console.WriteLine($"Unknown Packet {report.Data.Length}\t{BitConverter.ToString(report.Data)}");
                        }
                        break;
                    default:
                        {
                            byte Unknown1 = report.Data[0]; // always 0x01?
                            byte Unknown2 = report.Data[1]; // always 0x00?
                            VSCEventType EventType = (VSCEventType)report.Data[2];

                            switch (EventType)
                            {
                                case 0: // not sure what this is but wired controllers do it
                                    break;
                                case VSCEventType.CONTROL_UPDATE:
                                    {
                                        //report.Data[3] // 0x3C?

                                        UInt32 PacketIndex = BitConverter.ToUInt32(report.Data, 4);

                                        State.Buttons.A = (report.Data[8] & 128) == 128;
                                        State.Buttons.X = (report.Data[8] & 64) == 64;
                                        State.Buttons.B = (report.Data[8] & 32) == 32;
                                        State.Buttons.Y = (report.Data[8] & 16) == 16;
                                        State.Buttons.LeftBumper = (report.Data[8] & 8) == 8;
                                        State.Buttons.RightBumper = (report.Data[8] & 4) == 4;
                                        State.Buttons.LeftTrigger = (report.Data[8] & 2) == 2;
                                        State.Buttons.RightTrigger = (report.Data[8] & 1) == 1;

                                        State.Buttons.LeftGrip = (report.Data[9] & 128) == 128;
                                        State.Buttons.Start = (report.Data[9] & 64) == 64;
                                        State.Buttons.Steam = (report.Data[9] & 32) == 32;
                                        State.Buttons.Select = (report.Data[9] & 16) == 16;

                                        if (ControllerType == EControllerType.Chell)
                                        {
                                            State.Buttons.Touch0 = (report.Data[9] & 0x01) == 0x01;
                                            State.Buttons.Touch1 = (report.Data[9] & 0x02) == 0x02;
                                            State.Buttons.Touch2 = (report.Data[9] & 0x04) == 0x04;
                                            State.Buttons.Touch3 = (report.Data[9] & 0x08) == 0x08;
                                        }
                                        else
                                        {
                                            State.Buttons.Down = (report.Data[9] & 8) == 8;
                                            State.Buttons.Left = (report.Data[9] & 4) == 4;
                                            State.Buttons.Right = (report.Data[9] & 2) == 2;
                                            State.Buttons.Up = (report.Data[9] & 1) == 1;
                                        }
                                        bool LeftAnalogMultiplexMode = (report.Data[10] & 128) == 128;
                                        State.Buttons.StickClick = (report.Data[10] & 64) == 64;
                                        bool Unknown = (report.Data[10] & 32) == 32; // what is this?
                                        State.Buttons.RightPadTouch = (report.Data[10] & 16) == 16;
                                        bool LeftPadTouch = (report.Data[10] & 8) == 8;
                                        State.Buttons.RightPadClick = (report.Data[10] & 4) == 4;
                                        bool ThumbOrLeftPadPress = (report.Data[10] & 2) == 2; // what is this even for?
                                        State.Buttons.RightGrip = (report.Data[10] & 1) == 1;

                                        State.LeftTrigger = report.Data[11];
                                        State.RightTrigger = report.Data[12];

                                        if (LeftAnalogMultiplexMode)
                                        {
                                            if (LeftPadTouch)
                                            {
                                                State.Buttons.LeftPadTouch = true;
                                                State.Buttons.LeftPadClick = ThumbOrLeftPadPress;
                                                State.LeftPadX = BitConverter.ToInt16(report.Data, 16);
                                                State.LeftPadY = BitConverter.ToInt16(report.Data, 18);
                                            }
                                            else
                                            {
                                                State.LeftStickX = BitConverter.ToInt16(report.Data, 16);
                                                State.LeftStickY = BitConverter.ToInt16(report.Data, 18);
                                            }
                                        }
                                        else
                                        {
                                            if (LeftPadTouch)
                                            {
                                                State.Buttons.LeftPadTouch = true;
                                                State.LeftPadX = BitConverter.ToInt16(report.Data, 16);
                                                State.LeftPadY = BitConverter.ToInt16(report.Data, 18);
                                            }
                                            else
                                            {
                                                State.Buttons.LeftPadTouch = false;
                                                State.LeftStickX = BitConverter.ToInt16(report.Data, 16);
                                                State.LeftStickY = BitConverter.ToInt16(report.Data, 18);
                                                State.LeftPadX = 0;
                                                State.LeftPadY = 0;
                                            }

                                            State.Buttons.LeftPadClick = ThumbOrLeftPadPress && !State.Buttons.StickClick;
                                        }

                                        State.RightPadX = BitConverter.ToInt16(report.Data, 20);
                                        State.RightPadY = BitConverter.ToInt16(report.Data, 22);

                                        /*
                                        State.DataStuck = CheckSensorDataStuck();
                                        if (!SensorsEnabled || DataStuck) { EnableGyroSensors(); }
                                        */

                                        State.AccelerometerX = BitConverter.ToInt16(report.Data, 28);
                                        State.AccelerometerY = BitConverter.ToInt16(report.Data, 30);
                                        State.AccelerometerZ = BitConverter.ToInt16(report.Data, 32);
                                        State.AngularVelocityX = BitConverter.ToInt16(report.Data, 34);
                                        State.AngularVelocityY = BitConverter.ToInt16(report.Data, 36);
                                        State.AngularVelocityZ = BitConverter.ToInt16(report.Data, 38);
                                        State.OrientationW = BitConverter.ToInt16(report.Data, 40);
                                        State.OrientationX = BitConverter.ToInt16(report.Data, 42);
                                        State.OrientationY = BitConverter.ToInt16(report.Data, 44);
                                        State.OrientationZ = BitConverter.ToInt16(report.Data, 46);
                                    }
                                    break;

                                case VSCEventType.CONNECTION_DETAIL:
                                    {
                                        //report.Data[3] // 0x01?

                                        // Connection detail. 0x01 for disconnect, 0x02 for connect, 0x03 for pairing request.
                                        ConnectionState ConnectionStateV = (ConnectionState)report.Data[4];

                                        if (report.Data[4] == 0x01)
                                        {
                                            byte[] tmpBytes = new byte[4];
                                            tmpBytes[1] = report.Data[5];
                                            tmpBytes[2] = report.Data[6];
                                            tmpBytes[3] = report.Data[7];

                                            //BitConverter.ToUInt32(tmpBytes, 0); // Timestamp
                                        }
                                    }
                                    break;

                                case VSCEventType.BATTERY_UPDATE:
                                    {
                                        //report.Data[3] // 0x0B?

                                        UInt32 PacketIndex = BitConverter.ToUInt32(report.Data, 4);

                                        // only works if controller is configured to send this data

                                        // millivolts
                                        UInt16 BatteryVoltage = BitConverter.ToUInt16(report.Data, 8);
                                        //BitConverter.ToUInt16(report.Data, 10); // UNKNOWN, stuck at 100
                                    }
                                    break;

                                default:
                                    {
                                        Console.WriteLine($"Unknown Packet Type {(int)EventType:D3} of length {report.Data.Length}\t{BitConverter.ToString(report.Data)}");
                                    }
                                    break;
                            }
                        }
                        break; ;
                }

                SteamControllerState NewState = GetState();
                OnStateUpdated(NewState);
                Interlocked.Exchange(ref reportUsageLock, 0);

                _device.ReadReport(OnReport);
            }
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
