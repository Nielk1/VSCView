using HidLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace VSCView
{
    public class SteamController : IController
    {
        public const int VendorId = 0x28DE; // 10462
        public const int ProductIdDongle = 0x1142; // 4418;
        public const int ProductIdWired = 0x1102; // 4354
        public const int ProductIdChell = 0x1101; // 4353
        public const int ProductIdBT = 0x1106; // 4358

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
            Unknown,
            Chell,
            ReleaseV1,
            ReleaseV2,
        }

        public ControllerState GetState()
        {
            if (0 == Interlocked.Exchange(ref stateUsageLock, 1))
            {
                ControllerState newState = (ControllerState)State.Clone();

                /*ControllerState newState = new ControllerState();
                newState.ButtonsOld = (SteamControllerButtons)State.ButtonsOld.Clone();

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

                //newState.DataStuck = State.DataStuck;*/

                State = newState;
                Interlocked.Exchange(ref stateUsageLock, 0);
            }
            return State;
        }
        #endregion

        
        public string Serial { get; private set; }

        ControllerState State = new ControllerState();
        ControllerState OldState = new ControllerState();

        int Initalized;

        public EConnectionType ConnectionType { get; private set; }
        public EControllerType ControllerType { get; private set; }

        public delegate void StateUpdatedEventHandler(object sender, ControllerState e);
        public event StateUpdatedEventHandler StateUpdated;
        protected virtual void OnStateUpdated(ControllerState e)
        {
            StateUpdated?.Invoke(this, e);
        }

        // TODO for now it is safe to assume the startup connection type is correct, however, in the future we will need to have connection events trigger a recheck of the type or something once the V2 controller is out (if ever)
        public SteamController(HidDevice device, EConnectionType connection = EConnectionType.Unknown, EControllerType type = EControllerType.Unknown)
        {
            /*
            State.ButtonQuads["primary"] = new ControlButtonQuad();
            State.ButtonPairs["bumper"] = new ControlButtonPair();
            State.TriggerPairs["primary"] = new ControlTriggerPair();
            State.ButtonPairs["menu"] = new ControlButtonPair();
            State.ButtonPairs["grip"] = new ControlButtonPair();
            State.Buttons["home"] = new ControlButton();
            State.Sticks["left"] = new ControlStick(HasClick: true);
            State.Touch["left"] = new ControlTouch(TouchCount: 1, HasClick: true);
            State.Touch["right"] = new ControlTouch(TouchCount: 1, HasClick: true);
            */

            State.Controls["quad_left"] = new ControlDPad(/*4*/);
            State.Controls["quad_right"] = new ControlButtonQuad(EOrientation.Diamond);
            State.Controls["bumpers"] = new ControlButtonPair();
            State.Controls["triggers"] = new ControlTriggerPair(HasStage2: true);
            State.Controls["menu"] = new ControlButtonPair();
            State.Controls["grip"] = new ControlButtonPair();
            State.Controls["home"] = new ControlButton();
            State.Controls["stick_left"] = new ControlStick(HasClick: true);
            State.Controls["touch_left"] = new ControlTouch(TouchCount: 1, HasClick: true);
            State.Controls["touch_right"] = new ControlTouch(TouchCount: 1, HasClick: true);

            State.ButtonsOld = new SteamControllerButtons();

            _device = device;
            ConnectionType = connection;
            ControllerType = type;

            Initalized = 0;
        }

        public void Initalize()
        {
            if (Initalized > 1) return;

            HalfInitalize();

            //_attached = _device.IsConnected;

            Initalized = 2;
            _device.ReadReport(OnReport);
        }

        public void HalfInitalize()
        {
            if (Initalized > 0) return;

            // open the device overlapped read so we don't get stuck waiting for a report when we write to it
            _device.OpenDevice(DeviceMode.Overlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead | ShareMode.ShareWrite);

            //_device.Inserted += DeviceAttachedHandler;
            //_device.Removed += DeviceRemovedHandler;

            //_device.MonitorDeviceEvents = true;

            Initalized = 1;
        }

        public event ControllerNameUpdateEvent ControllerNameUpdated;

        public void DeInitalize()
        {
            if (Initalized == 0) return;

            //_device.Inserted -= DeviceAttachedHandler;
            //_device.Removed -= DeviceRemovedHandler;

            //_device.MonitorDeviceEvents = false;

            Initalized = 0;
            _device.CloseDevice();
        }

        private object FeatureReportLocalLock = new object();
        public byte[] GetFeatureReport(byte[] reportData)
        {
            if (reportData.Length < 2) return null;

            lock (FeatureReportLocalLock)
            {
                var result = _device.WriteFeatureData(reportData);

                if (!result) return null;

                byte[] reply;
                bool success;
                do
                {
                    success = _device.ReadFeatureData(out reply, 0);
                } while (success && reply[1] != reportData[1]);
                return reply;
            }
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
                //var result = GetFeatureReport(reportData) != null;
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
                //var result = GetFeatureReport(reportData) != null;
                SensorsEnabled = false;
                return result;
            }
            return false;
        }

        public void Identify()
        {
            PlayMelody(Melody.Rise_and_Shine);
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

            //return GetFeatureReport(reportData) != null;
            var result = _device.WriteFeatureData(reportData);
            return result;
        }
        
        public bool UpdateSerial()
        {
            byte[] reportData = new byte[64];
            reportData[1] = 0xAE; // 0xAE = get serial
            reportData[2] = 0x15; // 0x15 = length of data to be written
            reportData[3] = 0x01;

            Thread.Sleep(1000); // why do we need this? race condition?
            var result = _device.WriteFeatureData(reportData);
            Thread.Sleep(1000); // why do we need this? race condition?
            var reply = GetFeatureReport(reportData);

            //byte[] reply;
            //bool success = _device.ReadFeatureData(out reply, 0);

            if (reply == null || reply[1] != 0xae || reply[2] != 0x15 || reply[3] != 0x01)
            {
                Serial = null;
                ControllerNameUpdated?.Invoke();
                return false;
            }
            Serial = System.Text.Encoding.UTF8.GetString(reply.Skip(4).Take(10).ToArray());
            ControllerNameUpdated?.Invoke();
            return true;
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

        public string GetName()
        {
            /*
            List<string> NameParts = new List<string>();

            byte[] ManufacturerBytes;
            _device.ReadManufacturer(out ManufacturerBytes); // Sony Interactive Entertainment
            string Manufacturer = System.Text.Encoding.Unicode.GetString(ManufacturerBytes)?.Trim('\0');
            NameParts.Add(Manufacturer);

            byte[] ProductBytes;
            _device.ReadProduct(out ProductBytes); // DUALSHOCK®4 USB Wireless Adaptor
            string Product = System.Text.Encoding.Unicode.GetString(ProductBytes)?.Trim('\0');
            NameParts.Add(Product);

            if(string.IsNullOrWhiteSpace(Serial))
            {
                NameParts.Add(_device.DevicePath);
            }
            else
            {
                NameParts.Add(Serial);
            }

            return string.Join(@" | ", NameParts.Where(dr => !string.IsNullOrWhiteSpace(dr)).Select(dr => dr.Replace("&", "&&")));
            */

            string retVal = "Valve Steam Controller";
            switch(ControllerType)
            {
                case EControllerType.ReleaseV1: retVal += " 1001"; break;
                case EControllerType.ReleaseV2: retVal += " V2"; break;
                case EControllerType.Chell: retVal += " Chell"; break;
            }
            retVal += $" [{Serial ?? "No ID"}]";
            return retVal;
        }

        private void OnReport(HidReport report)
        {
            if (Initalized < 2) return;

            if (0 == Interlocked.Exchange(ref reportUsageLock, 1))
            {
                OldState = State; // shouldn't this be a clone?
                                    //if (_attached == false) { return; }

                switch (ConnectionType)
                {
                    case EConnectionType.Bluetooth:
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

                                        (State.Controls["quad_right"] as ControlButtonQuad).Button2 = (report.Data[8] & 128) == 128;
                                        (State.Controls["quad_right"] as ControlButtonQuad).Button3 = (report.Data[8] & 64) == 64;
                                        (State.Controls["quad_right"] as ControlButtonQuad).Button1 = (report.Data[8] & 32) == 32;
                                        (State.Controls["quad_right"] as ControlButtonQuad).Button0 = (report.Data[8] & 16) == 16;
                                        (State.Controls["bumpers"] as ControlButtonPair).Button0 = (report.Data[8] & 8) == 8;
                                        (State.Controls["bumpers"] as ControlButtonPair).Button1 = (report.Data[8] & 4) == 4;
                                        (State.Controls["triggers"] as ControlTriggerPair).Stage2_0 = (report.Data[8] & 2) == 2;
                                        (State.Controls["triggers"] as ControlTriggerPair).Stage2_1 = (report.Data[8] & 1) == 1;

                                        (State.Controls["grip"] as ControlButtonPair).Button0 = (report.Data[9] & 128) == 128;
                                        (State.Controls["menu"] as ControlButtonPair).Button1 = (report.Data[9] & 64) == 64;
                                        (State.Controls["home"] as ControlButton).Button0 = (report.Data[9] & 32) == 32;
                                        (State.Controls["menu"] as ControlButtonPair).Button0 = (report.Data[9] & 16) == 16;

                                        if (ControllerType == EControllerType.Chell)
                                        {
                                            State.ButtonsOld.Touch0 = (report.Data[9] & 0x01) == 0x01;
                                            State.ButtonsOld.Touch1 = (report.Data[9] & 0x02) == 0x02;
                                            State.ButtonsOld.Touch2 = (report.Data[9] & 0x04) == 0x04;
                                            State.ButtonsOld.Touch3 = (report.Data[9] & 0x08) == 0x08;
                                        }
                                        else
                                        {
                                            /*
                                            (State.Controls["quad_left"] as ControlButtonQuad).Button0 = (report.Data[9] & 1) == 1;
                                            (State.Controls["quad_left"] as ControlButtonQuad).Button1 = (report.Data[9] & 2) == 2;
                                            (State.Controls["quad_left"] as ControlButtonQuad).Button2 = (report.Data[9] & 8) == 8;
                                            (State.Controls["quad_left"] as ControlButtonQuad).Button3 = (report.Data[9] & 4) == 4;
                                            */

                                            // these are mutually exclusive in the raw data, so let's act like they are in the code too, even though they use 4 bits
                                            if ((report.Data[9] & 1) == 1)
                                            {
                                                (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.North;
                                            }
                                            else if ((report.Data[9] & 2) == 2)
                                            {
                                                (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.East;
                                            }
                                            else if ((report.Data[9] & 8) == 8)
                                            {
                                                (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.South;
                                            }
                                            else if ((report.Data[9] & 4) == 4)
                                            {
                                                (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.West;
                                            }
                                            else
                                            {
                                                (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.None;
                                            }
                                        }
                                        bool LeftAnalogMultiplexMode = (report.Data[10] & 128) == 128;
                                        bool LeftStickClick = (report.Data[10] & 64) == 64;
                                        (State.Controls["stick_left"] as ControlStick).Click = LeftStickClick;
                                        bool Unknown = (report.Data[10] & 32) == 32; // what is this?
                                        bool RightPadTouch = (report.Data[10] & 16) == 16;
                                        bool LeftPadTouch = (report.Data[10] & 8) == 8;
                                        (State.Controls["touch_right"] as ControlTouch).Click = (report.Data[10] & 4) == 4;
                                        bool ThumbOrLeftPadPress = (report.Data[10] & 2) == 2; // what is this even for?
                                        (State.Controls["grip"] as ControlButtonPair).Button1 = (report.Data[10] & 1) == 1;

                                        (State.Controls["triggers"] as ControlTriggerPair).Analog0 = (float)report.Data[11] / byte.MaxValue;
                                        (State.Controls["triggers"] as ControlTriggerPair).Analog1 = (float)report.Data[12] / byte.MaxValue;

                                        if (LeftAnalogMultiplexMode)
                                        {
                                            if (LeftPadTouch)
                                            {
                                                State.ButtonsOld.LeftPadTouch = true;
                                                float LeftPadX = (float)BitConverter.ToInt16(report.Data, 16) / Int16.MaxValue;
                                                float LeftPadY = (float)BitConverter.ToInt16(report.Data, 18) / Int16.MaxValue;
                                                (State.Controls["touch_left"] as ControlTouch).AddTouch(0, true, LeftPadX, LeftPadY, 0);
                                            }
                                            else
                                            {
                                                (State.Controls["stick_left"] as ControlStick).X = (float)BitConverter.ToInt16(report.Data, 16) / Int16.MaxValue;
                                                (State.Controls["stick_left"] as ControlStick).Y = (float)BitConverter.ToInt16(report.Data, 18) / Int16.MaxValue;
                                            }
                                        }
                                        else
                                        {
                                            if (LeftPadTouch)
                                            {
                                                float LeftPadX = (float)BitConverter.ToInt16(report.Data, 16) / Int16.MaxValue;
                                                float LeftPadY = (float)BitConverter.ToInt16(report.Data, 18) / Int16.MaxValue;
                                                (State.Controls["touch_left"] as ControlTouch).AddTouch(0, true, LeftPadX, LeftPadY, 0);
                                            }
                                            else
                                            {
                                                (State.Controls["stick_left"] as ControlStick).X = (float)BitConverter.ToInt16(report.Data, 16) / Int16.MaxValue;
                                                (State.Controls["stick_left"] as ControlStick).Y = (float)BitConverter.ToInt16(report.Data, 18) / Int16.MaxValue;
                                                (State.Controls["touch_left"] as ControlTouch).AddTouch(0, false, 0, 0, 0);
                                            }

                                            (State.Controls["touch_left"] as ControlTouch).Click = ThumbOrLeftPadPress && !LeftStickClick;
                                        }

                                        float RightPadX = (float)BitConverter.ToInt16(report.Data, 20) / Int16.MaxValue;
                                        float RightPadY = (float)BitConverter.ToInt16(report.Data, 22) / Int16.MaxValue;

                                        (State.Controls["touch_right"] as ControlTouch).AddTouch(0, RightPadTouch, RightPadX, RightPadY, 0);

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

                                        switch(ConnectionStateV)
                                        {
                                            case ConnectionState.CONNECT:
                                            case ConnectionState.DISCONNECT:
                                                UpdateSerial(); // this doesn't work if the controller is not already polling, obviously.  Might need to explicitly code in dongle watching or something.
                                                break;
                                        }

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

                ControllerState NewState = GetState();
                OnStateUpdated(NewState);
                Interlocked.Exchange(ref reportUsageLock, 0);
            }
            _device.ReadReport(OnReport);
        }

        public Image GetIcon()
        {
            Image Icon = new Bitmap(32 + 4, 16);
            Graphics g = Graphics.FromImage(Icon);

            switch (ConnectionType)
            {
                case EConnectionType.Dongle: g.DrawImage(Properties.Resources.icon_wireless, 0, 0, 16, 16); break;
                case EConnectionType.USB: g.DrawImage(Properties.Resources.icon_usb, 0, 0, 16, 16); break;
                case EConnectionType.Bluetooth: g.DrawImage(Properties.Resources.icon_bt, 0, 0, 16, 16); break;
            }

            switch(ControllerType)
            {
                case EControllerType.ReleaseV1: g.DrawImage(Properties.Resources.icon_sc, 16 + 4, 0, 16, 16); break;
                case EControllerType.ReleaseV2: g.DrawImage(Properties.Resources.icon_sc, 16 + 4, 0, 16, 16); break;
                case EControllerType.Chell: g.DrawImage(Properties.Resources.icon_chell, 16 + 4, 0, 16, 16); break;
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

    public class SteamControllerFactory : IControllerFactory
    {
        public IController[] GetControllers()
        {
            List<HidDevice> _devices = HidDevices.Enumerate(SteamController.VendorId, SteamController.ProductIdDongle, SteamController.ProductIdWired, /*SteamController.ProductIdBT,*/ SteamController.ProductIdChell).ToList();
            List<SteamController> ControllerList = new List<SteamController>();
            //string wired_m = "&pid_1102&mi_02";
            //string dongle_m = "&pid_1142&mi_01";
            //string dongle_m1 = "&pid_1142&mi_01";
            //string dongle_m2 = "&pid_1142&mi_02";
            //string dongle_m3 = "&pid_1142&mi_03";
            //string dongle_m4 = "&pid_1142&mi_04";
            //string bt_m = "_PID&1106_";
            //string chell_m = "&pid_1101";
            // we should never have holes, this entire dictionary is just because I don't know if I can trust the order I get the HID devices
            for (int i = 0; i < _devices.Count; i++)
            {
                if (_devices[i] != null)
                {
                    HidDevice _device = _devices[i];
                    string devicePath = _device.DevicePath.ToString();

                    EConnectionType ConType = EConnectionType.Unknown;
                    SteamController.EControllerType CtrlType = SteamController.EControllerType.ReleaseV1;
                    switch (_device.Attributes.ProductId)
                    {
                        //case SteamController.ProductIdBT:
                        //    ConType = EConnectionType.Bluetooth;
                        //    break;
                        case SteamController.ProductIdWired:
                            ConType = EConnectionType.USB;
                            break;
                        case SteamController.ProductIdDongle:
                            if (devicePath.Contains("mi_00")) continue;
                            ConType = EConnectionType.Dongle;
                            break;
                        case SteamController.ProductIdChell:
                            ConType = EConnectionType.USB;
                            CtrlType = SteamController.EControllerType.Chell;
                            break;
                    }

                    SteamController ctrl = new SteamController(_device, ConType, CtrlType);
                    ctrl.HalfInitalize();
                    new Thread(() =>
                    {
                        ctrl.UpdateSerial();
                    }).Start();
                    ControllerList.Add(ctrl);
                }
            }

            return ControllerList.OrderByDescending(dr => dr.ConnectionType).ThenBy(dr => dr.GetName()).ToArray();
        }
    }
}
