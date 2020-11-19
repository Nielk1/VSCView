﻿using HidLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace VSCView
{
    public class DS4Controller : IController
    {
        public const int VendorId = 0x054C;
        public const int ProductIdDongle = 0x0BA0;
        public const int ProductIdWired = 0x05C4; // and BT
        public const int ProductIdWiredV2 = 0x09CC; // and BT

        private const byte _REPORT_STATE_1 = 0x11;
        private const byte _REPORT_STATE_2 = 0x12;
        private const byte _REPORT_STATE_3 = 0x13;
        private const byte _REPORT_STATE_4 = 0x14;
        private const byte _REPORT_STATE_5 = 0x15;
        private const byte _REPORT_STATE_6 = 0x16;
        private const byte _REPORT_STATE_7 = 0x17;
        private const byte _REPORT_STATE_8 = 0x18;
        private const byte _REPORT_STATE_9 = 0x19;

        public bool SensorsEnabled;
        private HidDevice _device;
        int stateUsageLock = 0, reportUsageLock = 0;
        private byte last_touch_timestamp;
        private bool touch_last_frame;
        //private DateTime tmp = DateTime.Now;

        public event ControllerNameUpdateEvent ControllerNameUpdated;

        #region DATA STRUCTS

        public ControllerState GetState()
        {
            if (0 == Interlocked.Exchange(ref stateUsageLock, 1))
            {
                ControllerState newState = (ControllerState)State.Clone();
                /*
                ControllerState newState = new ControllerState();
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

                //newState.DataStuck = State.DataStuck;
                */
                State = newState;
                Interlocked.Exchange(ref stateUsageLock, 0);
            }
            return State;
        }
        #endregion

        public EConnectionType ConnectionType { get; private set; }

        ControllerState State = new ControllerState();
        ControllerState OldState = new ControllerState();

        int Initalized;

        public delegate void StateUpdatedEventHandler(object sender, ControllerState e);
        public event StateUpdatedEventHandler StateUpdated;
        protected virtual void OnStateUpdated(ControllerState e)
        {
            StateUpdated?.Invoke(this, e);
        }

        public DS4Controller(HidDevice device, EConnectionType ConnectionType = EConnectionType.Unknown)
        {
            this.ConnectionType = ConnectionType;

            State.Controls["quad_left"] = new ControlDPad();
            State.Controls["quad_right"] = new ControlButtonQuad();
            State.Controls["bumpers"] = new ControlButtonPair();
            State.Controls["bumpers2"] = new ControlButtonPair();
            State.Controls["triggers"] = new ControlTriggerPair(HasStage2: false);
            State.Controls["menu"] = new ControlButtonPair();
            State.Controls["home"] = new ControlButton();
            State.Controls["stick_left"] = new ControlStick(HasClick: true);
            State.Controls["stick_right"] = new ControlStick(HasClick: true);
            State.Controls["touch_center"] = new ControlTouch(TouchCount: 2, HasClick: true);
            State.Controls["motion"] = new ControlMotion();

            // According to this the normalized domain of the DS4 gyro is 1024 units per rad/s: https://gamedev.stackexchange.com/a/87178

            _device = device;

            Initalized = 0;
        }

        public void Initalize()
        {
            if (Initalized > 1) return;

            HalfInitalize();

            Initalized = 2;
            _device.ReadReport(OnReport);
        }

        public void HalfInitalize()
        {
            if (Initalized > 0) return;

            // open the device overlapped read so we don't get stuck waiting for a report when we write to it
            //_device.OpenDevice(DeviceMode.Overlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead | ShareMode.ShareWrite);
            _device.OpenDevice(DeviceMode.Overlapped, DeviceMode.Overlapped, ShareMode.ShareRead | ShareMode.ShareWrite);

            //_device.Inserted += DeviceAttachedHandler;
            //_device.Removed += DeviceRemovedHandler;

            //_device.MonitorDeviceEvents = true;

            Initalized = 1;
            touch_last_frame = false;

            //_attached = _device.IsConnected;

            if (ConnectionType == EConnectionType.Dongle)
            {
                _device.ReadReport(OnReport);
            }
        }

        public void DeInitalize()
        {
            if (Initalized == 0) return;

            //_device.Inserted -= DeviceAttachedHandler;
            //_device.Removed -= DeviceRemovedHandler;

            //_device.MonitorDeviceEvents = false;

            Initalized = 0;
            _device.CloseDevice();
        }

        public async void Identify()
        {
            HidReport report;
            int offset = 0;
            if (ConnectionType == EConnectionType.Bluetooth)
            {
                report = new HidReport(78)
                {
                    ReportId = 0x11,
                    Data = new byte[] { 0xC0, 0x20, 0x05, 0x00, 0x00, 0xff, 0xff, 0x00, 0x00, 0x00, 0x0f, 0x0f }
                };
                offset = 2;
            }
            else
            {
                report = new HidReport(32)
                {
                    ReportId = 0x05,
                    Data = new byte[] { 0x05, 0x00, 0x00, 0xff, 0xff, 0x00, 0x00, 0x00, 0x0f, 0x0f }
                };
            }

            if (_device.WriteReport(report))
            {
                Thread.Sleep(250);
                report.Data[offset + 0] = 0x01;
                report.Data[offset + 3] = 0x00;
                report.Data[offset + 4] = 0x00;
                _device.WriteReport(report);
                Thread.Sleep(2000);
                report.Data[offset + 0] = 0x04;
                report.Data[offset + 8] = 0x01;
                report.Data[offset + 9] = 0x01;
                _device.WriteReport(report);
            }
        }

        public bool CheckSensorDataStuck()
        {
            return (OldState != null &&
                (State.Controls["motion"] as ControlMotion).AccelerometerX == 0 &&
                (State.Controls["motion"] as ControlMotion).AccelerometerY == 0 &&
                (State.Controls["motion"] as ControlMotion).AccelerometerZ == 0 ||
                (State.Controls["motion"] as ControlMotion).AccelerometerX == (OldState.Controls["motion"] as ControlMotion).AccelerometerX &&
                (State.Controls["motion"] as ControlMotion).AccelerometerY == (OldState.Controls["motion"] as ControlMotion).AccelerometerY &&
                (State.Controls["motion"] as ControlMotion).AccelerometerZ == (OldState.Controls["motion"] as ControlMotion).AccelerometerZ ||
                (State.Controls["motion"] as ControlMotion).AngularVelocityX == (OldState.Controls["motion"] as ControlMotion).AngularVelocityX &&
                (State.Controls["motion"] as ControlMotion).AngularVelocityY == (OldState.Controls["motion"] as ControlMotion).AngularVelocityY &&
                (State.Controls["motion"] as ControlMotion).AngularVelocityZ == (OldState.Controls["motion"] as ControlMotion).AngularVelocityZ
            );
        }

        public string GetName()
        {
            switch (ConnectionType)
            {
                case EConnectionType.Dongle:
                    {
                        bool hasDevice = true;
                        byte[] data;
                        _device.ReadFeatureData(out data, 0xE3);
                        UInt16 local_VID = BitConverter.ToUInt16(data, 1);
                        UInt16 local_PID = BitConverter.ToUInt16(data, 3);
                        string retVal = $"Sony Device <{local_PID:X4}>";//"DUALSHOCK®4 USB Wireless Adaptor";
                        switch (local_VID)
                        {
                            case 0:
                                retVal = "DUALSHOCK®4 USB Wireless Adaptor";
                                hasDevice = false;
                                break;
                            case VendorId:
                                switch (local_PID)
                                {
                                    case ProductIdWired:
                                        retVal = $"Sony DUALSHOCK®4 Controller V1";
                                        break;
                                    case ProductIdWiredV2:
                                        retVal = $"Sony DUALSHOCK®4 Controller V2";
                                        break;
                                        //default:
                                        //    retVal = 
                                        //    break;
                                }
                                break;
                            default:
                                retVal = $"Unknown <{local_VID:X4},{local_PID:X4}>";
                                break;
                        }

                        if(!hasDevice)
                        {
                            return retVal;
                        }

                        string Serial = null;

                        if (_device.Attributes.VendorId == VendorId
                        && (_device.Attributes.ProductId == ProductIdWired || _device.Attributes.ProductId == ProductIdWiredV2 || _device.Attributes.ProductId == ProductIdDongle))
                        {
                            try
                            {
                                _device.ReadFeatureData(out data, 0x12);
                                Serial = string.Join(":", data.Skip(1).Take(6).Reverse().Select(dr => $"{dr:X2}").ToArray());
                                if (Serial == "00:00:00:00:00:00")
                                    Serial = null;
                            }
                            catch { }
                        }
                        if (string.IsNullOrWhiteSpace(Serial))
                        {
                            byte[] SerialNumberBytes;
                            if (_device.ReadSerialNumber(out SerialNumberBytes)) // DUALSHOCK®4 USB Wireless Adaptor
                            {
                                string SerialNumber = System.Text.Encoding.Unicode.GetString(SerialNumberBytes)?.Trim('\0');
                                if (!string.IsNullOrWhiteSpace(SerialNumber))
                                {
                                    Serial = SerialNumber;
                                }
                            }
                        }
                        if (string.IsNullOrWhiteSpace(Serial))
                            Serial = null;

                        return retVal += $" [{Serial ?? "No ID"}]";
                    }
                default:
                    {
                        string retVal = "Sony DUALSHOCK®4 Controller";

                        switch (_device.Attributes.ProductId)
                        {
                            case ProductIdWired:
                                retVal = $"Sony DUALSHOCK®4 Controller V1";
                                break;
                            case ProductIdWiredV2:
                                retVal = $"Sony DUALSHOCK®4 Controller V2";
                                break;
                        }

                        string Serial = null;

                        if (_device.Attributes.VendorId == VendorId
                        && (_device.Attributes.ProductId == ProductIdWired || _device.Attributes.ProductId == ProductIdWiredV2 || _device.Attributes.ProductId == ProductIdDongle))
                        {
                            try
                            {
                                byte[] data;
                                _device.ReadFeatureData(out data, 0x12);
                                Serial = string.Join(":", data.Skip(1).Take(6).Reverse().Select(dr => $"{dr:X2}").ToArray());
                            }
                            catch { }
                        }
                        if (string.IsNullOrWhiteSpace(Serial))
                        {
                            byte[] SerialNumberBytes;
                            if (_device.ReadSerialNumber(out SerialNumberBytes)) // DUALSHOCK®4 USB Wireless Adaptor
                            {
                                string SerialNumber = System.Text.Encoding.Unicode.GetString(SerialNumberBytes)?.Trim('\0');
                                if (!string.IsNullOrWhiteSpace(SerialNumber))
                                {
                                    Serial = SerialNumber;
                                }
                            }
                        }
                        if (string.IsNullOrWhiteSpace(Serial))
                            Serial = null;

                        return retVal += $" [{Serial ?? "No ID"}]"; ;
                    }
            }
        }

        bool DisconnectedBit = false;
        private void OnReport(HidReport report)
        {
            if (Initalized < 1) return;

            if (0 == Interlocked.Exchange(ref reportUsageLock, 1))
            {
                OldState = State; // shouldn't this be a clone?
                //if (_attached == false) { return; }

                int baseOffset = 0;
                bool HasStateData = true;
                if (ConnectionType == EConnectionType.Bluetooth)
                {
                    baseOffset = 2;
                    HasStateData = (report.Data[1] & 0x80) == 0x80;
                }

                if (HasStateData)
                {
                    (State.Controls["stick_left"] as ControlStick).X = (report.Data[baseOffset + 0] - 128) / 128f;
                    (State.Controls["stick_left"] as ControlStick).Y = (report.Data[baseOffset + 1] - 128) / 128f;
                    (State.Controls["stick_right"] as ControlStick).X = (report.Data[baseOffset + 2] - 128) / 128f;
                    (State.Controls["stick_right"] as ControlStick).Y = (report.Data[baseOffset + 3] - 128) / 128f;

                    (State.Controls["quad_right"] as ControlButtonQuad).ButtonN = (report.Data[baseOffset + 4] & 128) == 128;
                    (State.Controls["quad_right"] as ControlButtonQuad).ButtonE = (report.Data[baseOffset + 4] & 64) == 64;
                    (State.Controls["quad_right"] as ControlButtonQuad).ButtonS = (report.Data[baseOffset + 4] & 32) == 32;
                    (State.Controls["quad_right"] as ControlButtonQuad).ButtonW = (report.Data[baseOffset + 4] & 16) == 16;

                    switch ((report.Data[baseOffset + 4] & 0x0f))
                    {
                        case 0: (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.North; break;
                        case 1: (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.NorthEast; break;
                        case 2: (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.East; break;
                        case 3: (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.SouthEast; break;
                        case 4: (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.South; break;
                        case 5: (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.SouthWest; break;
                        case 6: (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.West; break;
                        case 7: (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.NorthWest; break;
                        default: (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.None; break;
                    }

                    (State.Controls["stick_right"] as ControlStick).Click = (report.Data[baseOffset + 5] & 128) == 128;
                    (State.Controls["stick_left"] as ControlStick).Click = (report.Data[baseOffset + 5] & 64) == 64;
                    (State.Controls["menu"] as ControlButtonPair).Right = (report.Data[baseOffset + 5] & 32) == 32;
                    (State.Controls["menu"] as ControlButtonPair).Left = (report.Data[baseOffset + 5] & 16) == 16;
                    (State.Controls["bumpers2"] as ControlButtonPair).Right = (report.Data[baseOffset + 5] & 8) == 8;
                    (State.Controls["bumpers2"] as ControlButtonPair).Left = (report.Data[baseOffset + 5] & 4) == 4;
                    (State.Controls["bumpers"] as ControlButtonPair).Right = (report.Data[baseOffset + 5] & 2) == 2;
                    (State.Controls["bumpers"] as ControlButtonPair).Left = (report.Data[baseOffset + 5] & 1) == 1;

                    // counter
                    // bld.Append((report.Data[baseOffset + 6] & 0xfc).ToString().PadLeft(3, '0'));

                    (State.Controls["home"] as ControlButton).Button0 = (report.Data[baseOffset + 6] & 0x1) == 0x1;
                    (State.Controls["touch_center"] as ControlTouch).Click = (report.Data[baseOffset + 6] & 0x2) == 0x2;
                    (State.Controls["triggers"] as ControlTriggerPair).L_Analog = (float)report.Data[baseOffset + 7] / byte.MaxValue;
                    (State.Controls["triggers"] as ControlTriggerPair).R_Analog = (float)report.Data[baseOffset + 8] / byte.MaxValue;

                    // GyroTimestamp
                    //bld.Append(BitConverter.ToUInt16(report.Data, baseOffset + 9).ToString().PadLeft(5));
                    // FIX: (timestamp * 16) / 3

                    // Battery Power Level
                    //bld.Append(report.Data[baseOffset + 11].ToString("X2") + "   ");

                    (State.Controls["motion"] as ControlMotion).AngularVelocityX = BitConverter.ToInt16(report.Data, baseOffset + 12);
                    (State.Controls["motion"] as ControlMotion).AngularVelocityZ = BitConverter.ToInt16(report.Data, baseOffset + 14);
                    (State.Controls["motion"] as ControlMotion).AngularVelocityY = BitConverter.ToInt16(report.Data, baseOffset + 16);
                    (State.Controls["motion"] as ControlMotion).AccelerometerX = BitConverter.ToInt16(report.Data, baseOffset + 18);
                    (State.Controls["motion"] as ControlMotion).AccelerometerY = BitConverter.ToInt16(report.Data, baseOffset + 20);
                    (State.Controls["motion"] as ControlMotion).AccelerometerZ = BitConverter.ToInt16(report.Data, baseOffset + 22);

                    // ??
                    // bld.Append(report.Data[baseOffset + 27].ToString("X2"));

                    //State.Inputs.? = (report.Data[baseOffset + 29] & 128) == 128;
                    //State.Inputs.Mic = (report.Data[baseOffset + 29] & 64) == 64;
                    //State.Inputs.Headphone = (report.Data[baseOffset + 29] & 32) == 32;
                    //State.Inputs.PowerCable = (report.Data[baseOffset + 29] * 16) == 16;

                    //int bat = report.Data[baseOffset + 29] & 0x0f;
                    //bool plugged = (report.Data[baseOffset + 29] & 0x10) == 0x10;

                    // ??
                    // bld.Append(report.Data[baseOffset + 30].ToString("X2"));
                    // bld.Append(report.Data[baseOffset + 31].ToString("X2") + " ");

                    bool DisconnectedFlag = (report.Data[baseOffset + 30] & 0x04) == 0x04;
                    if (DisconnectedFlag != DisconnectedBit)
                    {
                        DisconnectedBit = DisconnectedFlag;
                        ControllerNameUpdated?.Invoke();
                    }

                    int TouchDataCount = report.Data[baseOffset + 32];

                    for (int FingerCounter = 0; FingerCounter < TouchDataCount; FingerCounter++)
                    {
                        byte touch_timestamp = report.Data[baseOffset + 33 + (FingerCounter * 9)]; // Touch Pad Counter
                                                                                                   //DateTime tmp_now = DateTime.Now;

                        bool Finger1 = (report.Data[baseOffset + 34 + (FingerCounter * 9)] & 0x80) != 0x80;
                        byte Finger1Index = (byte)(report.Data[baseOffset + 34 + (FingerCounter * 9)] & 0x7f);
                        int F1X = report.Data[baseOffset + 35 + (FingerCounter * 9)]
                                               | ((report.Data[baseOffset + 36 + (FingerCounter * 9)] & 0xF) << 8);
                        int F1Y = ((report.Data[baseOffset + 36 + (FingerCounter * 9)] & 0xF0) >> 4)
                                                | (report.Data[baseOffset + 37 + (FingerCounter * 9)] << 4);

                        bool Finger2 = (report.Data[baseOffset + 38 + (FingerCounter * 9)] & 0x80) != 0x80;
                        byte Finger2Index = (byte)(report.Data[baseOffset + 38 + (FingerCounter * 9)] & 0x7f);
                        int F2X = report.Data[baseOffset + 39 + (FingerCounter * 9)]
                                               | ((report.Data[baseOffset + 40 + (FingerCounter * 9)] & 0xF) << 8);
                        int F2Y = ((report.Data[baseOffset + 40 + (FingerCounter * 9)] & 0xF0) >> 4)
                                                | (report.Data[baseOffset + 41 + (FingerCounter * 9)] << 4);

                        byte TimeDelta = touch_last_frame ? GetOverflowedDelta(last_touch_timestamp, touch_timestamp) : (byte)0;

                        //Console.WriteLine($"{TimeDelta} {(tmp_now - tmp).Milliseconds}");

                        (State.Controls["touch_center"] as ControlTouch).AddTouch(0, Finger1, (F1X / 1919f) * 2f - 1f, (F1Y / 942f) * 2f - 1f, TimeDelta);
                        (State.Controls["touch_center"] as ControlTouch).AddTouch(1, Finger2, (F2X / 1919f) * 2f - 1f, (F2Y / 942f) * 2f - 1f, TimeDelta);

                        last_touch_timestamp = touch_timestamp;
                        //tmp = tmp_now;
                    }

                    touch_last_frame = TouchDataCount > 0;

                    ControllerState NewState = GetState();
                    OnStateUpdated(NewState);
                }
                Interlocked.Exchange(ref reportUsageLock, 0);

                if (ConnectionType == EConnectionType.Dongle && DisconnectedBit)
                    Thread.Sleep(1000); // if we're a dongle and we're not connected we might only be partially initalized, so slow roll our read
                _device.ReadReport(OnReport);
            }
        }

        // Pure function
        private byte GetOverflowedDelta(byte prev, byte cur, uint overflow = byte.MaxValue + 1)
        {
            uint _cur = cur;
            while(_cur < prev)
                _cur += overflow;
            return (byte)(_cur - prev);
        }

        public Image GetIcon()
        {
            Image Icon = new Bitmap(32 + 4, 16);
            Graphics g = Graphics.FromImage(Icon);

            switch (ConnectionType)
            {
                case EConnectionType.Dongle: g.DrawImage(Properties.Resources.icon_ds4_dongle, 0, 0, 16, 16); break;
                case EConnectionType.USB: g.DrawImage(Properties.Resources.icon_usb, 0, 0, 16, 16); break;
                case EConnectionType.Bluetooth: g.DrawImage(Properties.Resources.icon_bt, 0, 0, 16, 16); break;
            }

            g.DrawImage(Properties.Resources.icon_ds4, 16 + 4, 0, 16, 16);

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

    public class DS4ControllerFactory : IControllerFactory
    {
        public IController[] GetControllers()
        {
            List<HidDevice> _devices = HidDevices.Enumerate(DS4Controller.VendorId, DS4Controller.ProductIdDongle, DS4Controller.ProductIdWired, DS4Controller.ProductIdWiredV2).ToList();
            List<DS4Controller> ControllerList = new List<DS4Controller>();
            string bt_hid_id = @"00001124-0000-1000-8000-00805f9b34fb";

            for (int i = 0; i < _devices.Count; i++)
            {
                if (_devices[i] != null)
                {
                    HidDevice _device = _devices[i];
                    string devicePath = _device.DevicePath.ToString();

                    EConnectionType ConType = EConnectionType.Unknown;
                    switch (_device.Attributes.ProductId)
                    {
                        case DS4Controller.ProductIdWired:
                        case DS4Controller.ProductIdWiredV2:
                            if (devicePath.Contains(bt_hid_id))
                            {
                                ConType = EConnectionType.Bluetooth;
                            }
                            else
                            {
                                ConType = EConnectionType.USB;
                            }
                            break;
                        case DS4Controller.ProductIdDongle:
                            ConType = EConnectionType.Dongle;
                            break;
                    }

                    DS4Controller ctrl = new DS4Controller(_device, ConType);
                    ctrl.HalfInitalize();
                    ControllerList.Add(ctrl);
                }
            }

            return ControllerList.OrderByDescending(dr => dr.ConnectionType).ThenBy(dr => dr.GetName()).ToArray();
        }
    }
}
