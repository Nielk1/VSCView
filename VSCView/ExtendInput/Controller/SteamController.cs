using ExtendInput.Providers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using VSCView;

namespace ExtendInput.Controller
{
    public class SteamController : IController
    {
        public const int VendorId = 0x28DE; // 10462
        public const int ProductIdDongle = 0x1142; // 4418
        public const int ProductIdWired = 0x1102; // 4354
        public const int ProductIdChell = 0x1101; // 4353
        public const int ProductIdBT = 0x1106; // 4358

        const byte k_nKeyboardReportNumber = 0x02;
        const byte k_nMouseReportNumber = 0x01;

        const byte k_nSegmentHasDataFlag = 0x80;
        const byte k_nLastSegmentFlag = 0x40;
        const byte k_nSegmentNumberMask = 0x07;

        const float PadAngle = 0.261799f; // 15 deg in radians


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

        private enum EBLEOptionDataChunksBitmask : UInt16
        {
            // First byte uppper nibble
            BLEButtonChunk1 = 0x10,
            BLEButtonChunk2 = 0x20,
            BLEButtonChunk3 = 0x40,
            BLELeftJoystickChunk = 0x80,

            // Second full byte
            BLELeftTrackpadChunk = 0x100,
            BLERightTrackpadChunk = 0x200,
            BLEIMUAccelChunk = 0x400,
            BLEIMUGyroChunk = 0x800,
            BLEIMUQuatChunk = 0x1000,
        };
        private enum EBLEPacketReportNums : byte
        {
            BLEReportState = 4,
            BLEReportStatus = 5,
        };
        public ControllerState GetState()
        {
            if (0 == Interlocked.Exchange(ref stateUsageLock, 1))
            {
                ControllerState newState = (ControllerState)State.Clone();

                State = newState;
                Interlocked.Exchange(ref stateUsageLock, 0);
            }
            return State;
        }
        #endregion

#if Serial
        public string Serial { get; private set; }
#endif

        ControllerState State = new ControllerState();
        ControllerState OldState = new ControllerState();

        struct RawSteamControllerState
        {
            public byte[] ulButtons;
            public byte sTriggerL;
            public byte sTriggerR;
            public byte[] ulButtons5; // what's this?

            public Int16 sLeftStickX;
            public Int16 sLeftStickY;

            public Int16 sLeftPadX;
            public Int16 sLeftPadY;

            public Int16 sRightPadX;
            public Int16 sRightPadY;

            public Int16 sAccelX { get; set; }
            public Int16 sAccelY { get; set; }
            public Int16 sAccelZ { get; set; }
            public Int16 sGyroX { get; set; }
            public Int16 sGyroY { get; set; }
            public Int16 sGyroZ { get; set; }
            public Int16 sGyroQuatW { get; set; }
            public Int16 sGyroQuatX { get; set; }
            public Int16 sGyroQuatY { get; set; }
            public Int16 sGyroQuatZ { get; set; }

            // We only need this bceause we add touch events rather than using the raw touch x/y
            public bool LeftTouchChange { get; set; }
        }

        RawSteamControllerState RawState = new RawSteamControllerState()
        {
            ulButtons = new byte[3],
            ulButtons5 = new byte[3]
        };

        int Initalized;

        public EConnectionType ConnectionType { get; private set; }
        public EControllerType ControllerType { get; private set; }

        private enum InternalConState
        {
            Unknown,
            Disconnected,
            Connected,
        }
        private InternalConState ConState;
        private DateTime ConnectedTime;

        public delegate void StateUpdatedEventHandler(object sender, ControllerState e);
        public event StateUpdatedEventHandler StateUpdated;
        protected virtual void OnStateUpdated(ControllerState e)
        {
            StateUpdated?.Invoke(this, e);
        }

        // TODO for now it is safe to assume the startup connection type is correct, however, in the future we will need to have connection events trigger a recheck of the type or something once the V2 controller is out (if ever)
        public SteamController(HidDevice device, EConnectionType connection = EConnectionType.Unknown, EControllerType type = EControllerType.Unknown)
        {
            State.Controls["quad_left"] = new ControlDPad(/*4*/);
            State.Controls["quad_right"] = new ControlButtonQuad();
            State.Controls["bumpers"] = new ControlButtonPair();
            State.Controls["triggers"] = new ControlTriggerPair(HasStage2: true);
            State.Controls["menu"] = new ControlButtonPair();
            State.Controls["grip"] = new ControlButtonPair();
            State.Controls["home"] = new ControlButton();
            State.Controls["stick_left"] = new ControlStick(HasClick: true);
            State.Controls["touch_left"] = new ControlTouch(TouchCount: 1, HasClick: true);
            State.Controls["touch_right"] = new ControlTouch(TouchCount: 1, HasClick: true);

            if (type == EControllerType.Chell)
            {
                State.Controls["grid_center"] = new ControlButtonGrid(2, 2);
            }

            State.Controls["motion"] = new ControlMotion();

            _device = device;
            ConnectionType = connection;
            ControllerType = type;

            Initalized = 0;

            ConnectedTime = DateTime.MinValue;
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
            //_device.OpenDevice(DeviceMode.Overlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead | ShareMode.ShareWrite);
            _device.OpenDevice(DeviceMode.Overlapped, DeviceMode.Overlapped, ShareMode.ShareRead | ShareMode.ShareWrite);

            //_device.Inserted += DeviceAttachedHandler;
            //_device.Removed += DeviceRemovedHandler;

            //_device.MonitorDeviceEvents = true;

            Initalized = 1;

#if Serial
            new Thread(() =>
            {
                try {
                    while (Initalized > 0)
                    {
                        UpdateMetadata();
                        Thread.Sleep(1000);
                    }
                } catch { }
            }).Start();
#endif
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
            byte[] reportData = new byte[65];
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

#if Serial
        public bool UpdateSerial()
        {
            // Avoiding trying to talk to the controller if it hasn't been around for 10 seconds.
            // This will allow steam time to talk to the controller as otherwise we steal Steam's feature requests.
            if (ConnectedTime.AddSeconds(10) > DateTime.UtcNow)
                return false;

            byte[] reportData = new byte[64];
            reportData[1] = 0xAE; // 0xAE = get serial
            reportData[2] = 0x15; // 0x15 = length of data to be written
            reportData[3] = 0x01;

            //Thread.Sleep(1000); // why do we need this? race condition?
            //var result = _device.WriteFeatureData(reportData);
            //if (!result) return false;
            //Thread.Sleep(1000); // why do we need this? race condition?
            var reply = GetFeatureReport(reportData);

            //byte[] reply;
            //bool success = _device.ReadFeatureData(out reply, 0);

            if (reply == null || reply[1] != 0xae || reply[4] == 0)
            {
                Serial = null;
                ControllerNameUpdated?.Invoke();
                return false;
            }
            //Serial = System.Text.Encoding.UTF8.GetString(reply.Skip(4).Take(10).ToArray());
            string NewSerial = System.Text.Encoding.UTF8.GetString(reply.Skip(4).Take(reply[4]).ToArray());
            NewSerial = NewSerial.Split('\0')[0];
            if (NewSerial.Length > 0) Serial = NewSerial;
            ControllerNameUpdated?.Invoke();
            return true;
        }
        private void ClearSerial()
        {
            if (Serial != null)
            {
                Serial = null;
                ControllerNameUpdated?.Invoke();
            }
        }

        private void UpdateMetadata()
        {
            //if (ConnectionType == EConnectionType.Bluetooth) return; // not sure how to do this on BT, might be normal?
            //if (ControllerType == EControllerType.Chell) return; // does Chell support this?

            switch (ConState)
            {
                case InternalConState.Unknown:
                case InternalConState.Connected:
                    if (Serial == null)
                        UpdateSerial();
                    break;
                case InternalConState.Disconnected:
                    ClearSerial();
                    break;
            }

        }
#endif

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
            string retVal = "Valve Steam Controller";
            switch(ControllerType)
            {
                case EControllerType.ReleaseV1: retVal += " 1001"; break;
                case EControllerType.ReleaseV2: retVal += " V2"; break;
                case EControllerType.Chell: retVal += " Chell"; break;
            }
            if (ConState == InternalConState.Disconnected)
            {
                retVal += $" <{ConState}>";
            }
            else
            {
                retVal += $" <{ConState}>";
#if Serial
                retVal += $" [{Serial ?? "No ID"}]";
#endif
            }
            return retVal;
        }

        private void OnReport(byte[] reportData, int reportID)
        {
            if (Initalized < 2) return;

            // If we happen to receive any keyboard or mouse reports just skip them and keep reading
            if (reportID == k_nKeyboardReportNumber || reportID == k_nMouseReportNumber)
                return;

            if (0 == Interlocked.Exchange(ref reportUsageLock, 1))
            {
                //OldState = State; // shouldn't this be a clone?
                                    //if (_attached == false) { return; }

                switch (ConnectionType)
                {
                    // report ID here is 3, do check what it is for the other connection types
                    case EConnectionType.Bluetooth:
                        {
                            //Console.WriteLine($"Unknown Packet {reportData.Length}\t{BitConverter.ToString(reportData)}");

                            byte ucHeader = reportData[0];
                            if ((ucHeader & k_nSegmentHasDataFlag) != k_nSegmentHasDataFlag)
                                return; // steam itself actually asserts in this case

                            if(!IsSegmentNumberValid(ucHeader))
                            {
                                // This likely means that we missed a packet.
                                //m_InputReportState.Reset();
                                ResetSegment();

                                // If the sequence number is zero then we can use it, otherwise we need to flush
                                // the remainder of the partial message.
                                if ((ucHeader & k_nSegmentNumberMask) > 0)
                                    return;
                            }

                            {
                                int nLength = k_nMaxSegmentSize - 1;
                                Array.Copy(reportData, 1, m_rgubBuffer, m_unCurrentMsgIndex, nLength);
                                m_unCurrentMsgIndex += nLength;
                                ++m_unNextSegmentNumber;
                            }

                            if ((ucHeader & k_nLastSegmentFlag) == k_nLastSegmentFlag)
                            {
                                // Last packet

                                EBLEPacketReportNums reportType = (EBLEPacketReportNums)(m_rgubBuffer[0] & 0x0f);
                                switch(reportType)
                                {
                                    case EBLEPacketReportNums.BLEReportState:
                                        {
                                            EBLEOptionDataChunksBitmask ucOptionDataMask = (EBLEOptionDataChunksBitmask)(BitConverter.ToUInt16(m_rgubBuffer, 0) & 0xfff0);
                                            int pBuf = 2;

                                            try
                                            {
                                                if (ucOptionDataMask.HasFlag(EBLEOptionDataChunksBitmask.BLEButtonChunk1))
                                                {
                                                    bool LeftTouchOld = (RawState.ulButtons[2] & 8) == 8;
                                                    bool RightTouchOld = (RawState.ulButtons[2] & 16) == 16;

                                                    Array.Copy(m_rgubBuffer, pBuf, RawState.ulButtons, 0, 3);

                                                    if (LeftTouchOld != ((RawState.ulButtons[2] & 8) == 8))
                                                        RawState.LeftTouchChange = true;

                                                    //if (RightTouchOld != ((RawState.ulButtons[2] & 16) == 16))
                                                    //    RawState.RightTouchChange = true;

                                                    //Console.WriteLine($"{Convert.ToString(RawState.ulButtons[0], 2).PadLeft(8, '0')} {Convert.ToString(RawState.ulButtons[1], 2).PadLeft(8, '0')} {Convert.ToString(RawState.ulButtons[2], 2).PadLeft(8, '0')}");

                                                    pBuf += 3;
                                                }
                                                if (ucOptionDataMask.HasFlag(EBLEOptionDataChunksBitmask.BLEButtonChunk2))
                                                {
                                                    RawState.sTriggerL = m_rgubBuffer[pBuf + 0];
                                                    RawState.sTriggerR = m_rgubBuffer[pBuf + 1];

                                                    pBuf += 2;
                                                }
                                                if (ucOptionDataMask.HasFlag(EBLEOptionDataChunksBitmask.BLEButtonChunk3))
                                                {
                                                    Array.Copy(m_rgubBuffer, pBuf, RawState.ulButtons5, 0, 3);

                                                    Console.WriteLine($"{Convert.ToString(RawState.ulButtons5[0], 2).PadLeft(8, '0')} {Convert.ToString(RawState.ulButtons5[1], 2).PadLeft(8, '0')} {Convert.ToString(RawState.ulButtons5[2], 2).PadLeft(8, '0')}");

                                                    pBuf++;
                                                    pBuf++;
                                                    pBuf++;
                                                }
                                                if (ucOptionDataMask.HasFlag(EBLEOptionDataChunksBitmask.BLELeftJoystickChunk))
                                                {
                                                    RawState.sLeftStickX = BitConverter.ToInt16(m_rgubBuffer, pBuf);
                                                    RawState.sLeftStickY = BitConverter.ToInt16(m_rgubBuffer, pBuf + sizeof(Int16));

                                                    pBuf += sizeof(Int16) + sizeof(Int16);
                                                }
                                                if (ucOptionDataMask.HasFlag(EBLEOptionDataChunksBitmask.BLELeftTrackpadChunk))
                                                {
                                                    //if ((RawState.ulButtons[2] & 8) == 8) // let's only set the X/Y with a touch to avoid jumps to 0
                                                    {
                                                        int X = BitConverter.ToInt16(m_rgubBuffer, pBuf);
                                                        int Y = BitConverter.ToInt16(m_rgubBuffer, pBuf + sizeof(Int16));

                                                        RotateXY(-PadAngle, ref X, ref Y);

                                                        RawState.sLeftPadX = (short)Math.Min(Math.Max(X, Int16.MinValue), Int16.MaxValue);
                                                        RawState.sLeftPadY = (short)Math.Min(Math.Max(Y, Int16.MinValue), Int16.MaxValue);

                                                        RawState.LeftTouchChange = true;
                                                    }

                                                    pBuf += sizeof(Int16) + sizeof(Int16);
                                                }
                                                if (ucOptionDataMask.HasFlag(EBLEOptionDataChunksBitmask.BLERightTrackpadChunk))
                                                {
                                                    //if ((RawState.ulButtons[2] & 16) == 16) // let's only set the X/Y with a touch to avoid jumps to 0
                                                    {
                                                        int X = BitConverter.ToInt16(m_rgubBuffer, pBuf);
                                                        int Y = BitConverter.ToInt16(m_rgubBuffer, pBuf + sizeof(Int16));

                                                        RotateXY(PadAngle, ref X, ref Y);

                                                        RawState.sRightPadX = (short)Math.Min(Math.Max(X, Int16.MinValue), Int16.MaxValue);
                                                        RawState.sRightPadY = (short)Math.Min(Math.Max(Y, Int16.MinValue), Int16.MaxValue);

                                                        //RawState.RightTouchChange = true;
                                                    }

                                                    pBuf += sizeof(Int16) + sizeof(Int16);
                                                }
                                                if (ucOptionDataMask.HasFlag(EBLEOptionDataChunksBitmask.BLEIMUAccelChunk))
                                                {
                                                    RawState.sAccelX = BitConverter.ToInt16(m_rgubBuffer, pBuf);
                                                    RawState.sAccelY = BitConverter.ToInt16(m_rgubBuffer, pBuf + sizeof(Int16));
                                                    RawState.sAccelZ = BitConverter.ToInt16(m_rgubBuffer, pBuf + sizeof(Int16) + sizeof(Int16));

                                                    pBuf += sizeof(Int16) + sizeof(Int16) + sizeof(Int16);
                                                }
                                                if (ucOptionDataMask.HasFlag(EBLEOptionDataChunksBitmask.BLEIMUGyroChunk))
                                                {
                                                    RawState.sGyroX = BitConverter.ToInt16(m_rgubBuffer, pBuf);
                                                    RawState.sGyroY = BitConverter.ToInt16(m_rgubBuffer, pBuf + sizeof(Int16));
                                                    RawState.sGyroZ = BitConverter.ToInt16(m_rgubBuffer, pBuf + sizeof(Int16) + sizeof(Int16));

                                                    pBuf += sizeof(Int16) + sizeof(Int16) + sizeof(Int16);
                                                }
                                                if (ucOptionDataMask.HasFlag(EBLEOptionDataChunksBitmask.BLEIMUQuatChunk))
                                                {
                                                    RawState.sGyroQuatW = BitConverter.ToInt16(m_rgubBuffer, pBuf);
                                                    RawState.sGyroQuatX = BitConverter.ToInt16(m_rgubBuffer, pBuf + sizeof(Int16));
                                                    RawState.sGyroQuatY = BitConverter.ToInt16(m_rgubBuffer, pBuf + sizeof(Int16) + sizeof(Int16));
                                                    RawState.sGyroQuatZ = BitConverter.ToInt16(m_rgubBuffer, pBuf + sizeof(Int16) + sizeof(Int16) + sizeof(Int16));

                                                    pBuf += sizeof(Int16) + sizeof(Int16) + sizeof(Int16) + sizeof(Int16);
                                                }

                                                //Console.WriteLine($"Good Packet {reportID}\t{BitConverter.ToString(m_rgubBuffer)}");
                                            }
                                            catch
                                            {
                                                //Console.WriteLine($"Error Packet {reportID}\t{BitConverter.ToString(m_rgubBuffer)}");
                                            }

                                            ProcessStateBytes();
                                        }
                                        break;
                                    case EBLEPacketReportNums.BLEReportStatus:
                                        break;
                                    default:
                                        Console.WriteLine($"Unknown Packet {reportID}\t{BitConverter.ToString(m_rgubBuffer)}");
                                        break;
                                }

                                ResetSegment();
                            }
                        }
                        break;
                    default:
                        {
                            byte Unknown1 = reportData[0]; // always 0x01?
                            byte Unknown2 = reportData[1]; // always 0x00?
                            VSCEventType EventType = (VSCEventType)reportData[2];
                            //reportData[3] // length

                            switch (EventType)
                            {
                                case 0: // not sure what this is but wired controllers do it
                                    break;
                                case VSCEventType.CONTROL_UPDATE:
                                    {

                                        UInt32 PacketIndex = BitConverter.ToUInt32(reportData, 4);

                                        Array.Copy(reportData, 8 + 0, RawState.ulButtons, 0, 3);

                                        bool LeftAnalogMultiplexMode = (RawState.ulButtons[2] & 128) == 128;
                                        bool LeftStickClick = (RawState.ulButtons[2] & 64) == 64;
                                        (State.Controls["stick_left"] as ControlStick).Click = LeftStickClick;
                                        bool Unknown = (RawState.ulButtons[2] & 32) == 32; // what is this?
                                        bool RightPadTouch = (RawState.ulButtons[2] & 16) == 16;
                                        bool LeftPadTouch = (RawState.ulButtons[2] & 8) == 8;
                                        (State.Controls["touch_right"] as ControlTouch).Click = (RawState.ulButtons[2] & 4) == 4;
                                        bool ThumbOrLeftPadPress = (RawState.ulButtons[2] & 2) == 2; // what is this even for?
                                        (State.Controls["grip"] as ControlButtonPair).Right = (RawState.ulButtons[2] & 1) == 1;

                                        RawState.sTriggerL = reportData[8 + 3];
                                        RawState.sTriggerR = reportData[8 + 4];

                                        if (LeftAnalogMultiplexMode)
                                        {
                                            if (LeftPadTouch)
                                            {
                                                int X = BitConverter.ToInt16(reportData, 8 + 8);
                                                int Y = BitConverter.ToInt16(reportData, 8 + 10);

                                                RotateXY(-PadAngle, ref X, ref Y);

                                                RawState.sLeftPadX = (short)Math.Min(Math.Max(X, Int16.MinValue), Int16.MaxValue);
                                                RawState.sLeftPadY = (short)Math.Min(Math.Max(Y, Int16.MinValue), Int16.MaxValue);

                                                RawState.LeftTouchChange = true;
                                            }
                                            else
                                            {
                                                RawState.sLeftStickX = BitConverter.ToInt16(reportData, 8 + 8);
                                                RawState.sLeftStickY = BitConverter.ToInt16(reportData, 8 + 10);
                                            }
                                        }
                                        else
                                        {
                                            if (LeftPadTouch)
                                            {
                                                int X = BitConverter.ToInt16(reportData, 8 + 8);
                                                int Y = BitConverter.ToInt16(reportData, 8 + 10);

                                                RotateXY(-PadAngle, ref X, ref Y);

                                                RawState.sLeftPadX = (short)Math.Min(Math.Max(X, Int16.MinValue), Int16.MaxValue);
                                                RawState.sLeftPadY = (short)Math.Min(Math.Max(Y, Int16.MinValue), Int16.MaxValue);
                                            }
                                            else
                                            {
                                                // // we're trying to fix the pad jumping to center by not sending new coords if the pad is not touched
                                                RawState.sLeftPadX = 0;
                                                RawState.sLeftPadY = 0;

                                                RawState.sLeftStickX = BitConverter.ToInt16(reportData, 8 + 8);
                                                RawState.sLeftStickY = BitConverter.ToInt16(reportData, 8 + 10);
                                            }

                                            RawState.LeftTouchChange = true;

                                            //(State.Controls["touch_left"] as ControlTouch).Click = ThumbOrLeftPadPress && !LeftStickClick;
                                        }

                                        //if (RightPadTouch) // we're trying to fix the pad jumping to center by not sending new coords if the pad is not touched
                                        {
                                            int X = BitConverter.ToInt16(reportData, 8 + 12);
                                            int Y = BitConverter.ToInt16(reportData, 8 + 14);

                                            RotateXY(PadAngle, ref X, ref Y);

                                            RawState.sRightPadX = (short)Math.Min(Math.Max(X, Int16.MinValue), Int16.MaxValue);
                                            RawState.sRightPadY = (short)Math.Min(Math.Max(Y, Int16.MinValue), Int16.MaxValue);
                                        }

                                        //RawState.RightTouchChange = true;

                                        RawState.sAccelX = BitConverter.ToInt16(reportData, 8 + 20);
                                        RawState.sAccelY = BitConverter.ToInt16(reportData, 8 + 22);
                                        RawState.sAccelZ = BitConverter.ToInt16(reportData, 8 + 24);
                                        RawState.sGyroX = BitConverter.ToInt16(reportData, 8 + 26);
                                        RawState.sGyroY = BitConverter.ToInt16(reportData, 8 + 28);
                                        RawState.sGyroZ = BitConverter.ToInt16(reportData, 8 + 30);
                                        RawState.sGyroQuatW = BitConverter.ToInt16(reportData, 8 + 32);
                                        RawState.sGyroQuatX = BitConverter.ToInt16(reportData, 8 + 34);
                                        RawState.sGyroQuatY = BitConverter.ToInt16(reportData, 8 + 36);
                                        RawState.sGyroQuatZ = BitConverter.ToInt16(reportData, 8 + 38);

                                        ProcessStateBytes();

                                        ConState = InternalConState.Connected;
                                        ConnectedTime = DateTime.UtcNow;
                                        ControllerNameUpdated?.Invoke();
                                    }
                                    break;

                                case VSCEventType.CONNECTION_DETAIL:
                                    {
                                        //reportData[3] // 0x01?

                                        // Connection detail. 0x01 for disconnect, 0x02 for connect, 0x03 for pairing request.
                                        ConnectionState ConnectionStateV = (ConnectionState)reportData[4];

                                        switch(ConnectionStateV)
                                        {
                                            case ConnectionState.CONNECT:
                                                ConState = InternalConState.Connected;
                                                ConnectedTime = DateTime.UtcNow;
                                                ControllerNameUpdated?.Invoke();
                                                break;
                                            case ConnectionState.DISCONNECT:
                                                ConState = InternalConState.Disconnected;
                                                ControllerNameUpdated?.Invoke();
                                                break;
                                        }
                                    }
                                    break;

                                case VSCEventType.BATTERY_UPDATE:
                                    {
                                        //reportData[3] // 0x0B?

                                        UInt32 PacketIndex = BitConverter.ToUInt32(reportData, 4);

                                        // only works if controller is configured to send this data

                                        // millivolts
                                        UInt16 BatteryVoltage = BitConverter.ToUInt16(reportData, 8);
                                        //BitConverter.ToUInt16(reportData, 10); // UNKNOWN, stuck at 100
                                    }
                                    break;

                                default:
                                    {
                                        Console.WriteLine($"Unknown Packet Type {(int)EventType:D3} of length {reportData.Length}\t{BitConverter.ToString(reportData)}");
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

        private void RotateXY(float padAngle, ref int x, ref int y, int cx = 0, int cy = 0)
        {
            int xrot = (int)(Math.Cos(padAngle) * (x - cx) - Math.Sin(padAngle) * (y - cy) + cx);
            int yrot = (int)(Math.Sin(padAngle) * (x - cx) + Math.Cos(padAngle) * (y - cy) + cy);

            x = xrot;
            y = yrot;
        }

        const int k_nMaxSegmentSize = 19;
        const int k_nMaxSegmentPayloadSize = k_nMaxSegmentSize - 1;
        const int k_nMaxNumberOfSegments = 8;
        const int k_nMaxMessageSize = k_nMaxSegmentPayloadSize * k_nMaxNumberOfSegments;
        byte[] m_rgubBuffer = new byte[k_nMaxMessageSize];

        byte m_unNextSegmentNumber = 0x00;
        int m_unCurrentMsgIndex = 0x00;

        private void ResetSegment()
        {
            m_unNextSegmentNumber = 0;
            m_unCurrentMsgIndex = 0;
            Array.Clear(m_rgubBuffer, 0, m_rgubBuffer.Length);
        }

        private bool IsSegmentNumberValid(byte ucHeader)
        {
            return ((ucHeader & k_nSegmentNumberMask) == m_unNextSegmentNumber);
        }

        private void ProcessStateBytes()
        {
            //OldState = State; // shouldn't this be a clone?
            OldState = (ControllerState)State.Clone(); // shouldn't this be a clone?

            (State.Controls["quad_right"] as ControlButtonQuad).ButtonS = (RawState.ulButtons[0] & 128) == 128; // A - S SE
            (State.Controls["quad_right"] as ControlButtonQuad).ButtonW = (RawState.ulButtons[0] & 64) == 64;   // X - W SW
            (State.Controls["quad_right"] as ControlButtonQuad).ButtonE = (RawState.ulButtons[0] & 32) == 32;   // B - E NE
            (State.Controls["quad_right"] as ControlButtonQuad).ButtonN = (RawState.ulButtons[0] & 16) == 16;   // Y - N NW
            (State.Controls["bumpers"] as ControlButtonPair).Left = (RawState.ulButtons[0] & 8) == 8;
            (State.Controls["bumpers"] as ControlButtonPair).Right = (RawState.ulButtons[0] & 4) == 4;
            (State.Controls["triggers"] as ControlTriggerPair).L_Stage2 = (RawState.ulButtons[0] & 2) == 2;
            (State.Controls["triggers"] as ControlTriggerPair).R_Stage2 = (RawState.ulButtons[0] & 1) == 1;

            (State.Controls["grip"] as ControlButtonPair).Left = (RawState.ulButtons[1] & 128) == 128;
            (State.Controls["menu"] as ControlButtonPair).Right = (RawState.ulButtons[1] & 64) == 64;
            (State.Controls["home"] as ControlButton).Button0 = (RawState.ulButtons[1] & 32) == 32;
            (State.Controls["menu"] as ControlButtonPair).Left = (RawState.ulButtons[1] & 16) == 16;

            if (ControllerType == EControllerType.Chell)
            {
                // for the Chell controller, these are the 4 face buttons
                (State.Controls["grid_center"] as ControlButtonGrid).Button[0, 0] = (RawState.ulButtons[1] & 0x01) == 0x01; // State.ButtonsOld.Touch0 = (RawState.ulButtons[1] & 0x01) == 0x01; // NW
                (State.Controls["grid_center"] as ControlButtonGrid).Button[1, 0] = (RawState.ulButtons[1] & 0x02) == 0x02; // State.ButtonsOld.Touch1 = (RawState.ulButtons[1] & 0x02) == 0x02; // NE
                (State.Controls["grid_center"] as ControlButtonGrid).Button[0, 1] = (RawState.ulButtons[1] & 0x04) == 0x04; // State.ButtonsOld.Touch2 = (RawState.ulButtons[1] & 0x04) == 0x04; // SW
                (State.Controls["grid_center"] as ControlButtonGrid).Button[1, 1] = (RawState.ulButtons[1] & 0x08) == 0x08; // State.ButtonsOld.Touch3 = (RawState.ulButtons[1] & 0x08) == 0x08; // SE
            }
            else
            {
                // these are mutually exclusive in the raw data, so let's act like they are in the code too, even though they use 4 bits
                if ((RawState.ulButtons[1] & 1) == 1)
                {
                    (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.North;
                }
                else if ((RawState.ulButtons[1] & 2) == 2)
                {
                    (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.East;
                }
                else if ((RawState.ulButtons[1] & 8) == 8)
                {
                    (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.South;
                }
                else if ((RawState.ulButtons[1] & 4) == 4)
                {
                    (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.West;
                }
                else
                {
                    (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.None;
                }
            }
            //bool LeftAnalogMultiplexMode = (RawState.ulButtons[2] & 128) == 128;
            bool LeftStickClick = (RawState.ulButtons[2] & 64) == 64;
            (State.Controls["stick_left"] as ControlStick).Click = LeftStickClick;
            //bool Unknown = (RawState.ulButtons[2] & 32) == 32; // what is this?
            bool RightPadTouch = (RawState.ulButtons[2] & 16) == 16;
            bool LeftPadTouch = (RawState.ulButtons[2] & 8) == 8;
            (State.Controls["touch_right"] as ControlTouch).Click = (RawState.ulButtons[2] & 4) == 4;
            bool ThumbOrLeftPadPress = (RawState.ulButtons[2] & 2) == 2; // what is this even for?
            (State.Controls["grip"] as ControlButtonPair).Right = (RawState.ulButtons[2] & 1) == 1;

            (State.Controls["triggers"] as ControlTriggerPair).L_Analog = (float)RawState.sTriggerL / byte.MaxValue;
            (State.Controls["triggers"] as ControlTriggerPair).R_Analog = (float)RawState.sTriggerR / byte.MaxValue;

            (State.Controls["stick_left"] as ControlStick).X = (float)RawState.sLeftStickX / Int16.MaxValue;
            (State.Controls["stick_left"] as ControlStick).Y = (float)-RawState.sLeftStickY / Int16.MaxValue;
            if (RawState.LeftTouchChange)
            {
                float LeftPadX = LeftPadTouch ? (float)RawState.sLeftPadX / Int16.MaxValue : 0f;
                float LeftPadY = LeftPadTouch ? (float)-RawState.sLeftPadY / Int16.MaxValue : 0f;
                (State.Controls["touch_left"] as ControlTouch).AddTouch(0, LeftPadTouch, LeftPadX, LeftPadY, 0);
            }

            float RightPadX = (float)RawState.sRightPadX / Int16.MaxValue;
            float RightPadY = (float)-RawState.sRightPadY / Int16.MaxValue;

            (State.Controls["touch_right"] as ControlTouch).AddTouch(0, RightPadTouch, RightPadX, RightPadY, 0);
            (State.Controls["touch_left"] as ControlTouch).Click = ThumbOrLeftPadPress && !LeftStickClick;

            /*
            State.DataStuck = CheckSensorDataStuck();
            if (!SensorsEnabled || DataStuck) { EnableGyroSensors(); }
            */

            (State.Controls["motion"] as ControlMotion).AccelerometerX = RawState.sAccelX;
            (State.Controls["motion"] as ControlMotion).AccelerometerY = RawState.sAccelY;
            (State.Controls["motion"] as ControlMotion).AccelerometerZ = RawState.sAccelZ;
            (State.Controls["motion"] as ControlMotion).AngularVelocityX = RawState.sGyroX;
            (State.Controls["motion"] as ControlMotion).AngularVelocityY = RawState.sGyroY;
            (State.Controls["motion"] as ControlMotion).AngularVelocityZ = RawState.sGyroZ;
            (State.Controls["motion"] as ControlMotion).OrientationW = RawState.sGyroQuatW;
            (State.Controls["motion"] as ControlMotion).OrientationX = RawState.sGyroQuatX;
            (State.Controls["motion"] as ControlMotion).OrientationY = RawState.sGyroQuatY;
            (State.Controls["motion"] as ControlMotion).OrientationZ = RawState.sGyroQuatZ;

            RawState.LeftTouchChange = false;
        }

        public Image GetIcon()
        {
            Image Icon = new Bitmap(32 + 4, 16);
            Graphics g = Graphics.FromImage(Icon);

            switch (ConnectionType)
            {
                case EConnectionType.Dongle: g.DrawImage(VSCView.Properties.Resources.icon_wireless, 0, 0, 16, 16); break;
                case EConnectionType.USB: g.DrawImage(VSCView.Properties.Resources.icon_usb, 0, 0, 16, 16); break;
                case EConnectionType.Bluetooth: g.DrawImage(VSCView.Properties.Resources.icon_bt, 0, 0, 16, 16); break;
            }

            switch(ControllerType)
            {
                case EControllerType.ReleaseV1: g.DrawImage(VSCView.Properties.Resources.icon_sc, 16 + 4, 0, 16, 16); break;
                case EControllerType.ReleaseV2: g.DrawImage(VSCView.Properties.Resources.icon_sc, 16 + 4, 0, 16, 16); break;
                case EControllerType.Chell: g.DrawImage(VSCView.Properties.Resources.icon_chell, 16 + 4, 0, 16, 16); break;
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
}
