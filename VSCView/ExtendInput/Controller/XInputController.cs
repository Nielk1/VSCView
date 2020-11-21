using ExtendInput.Providers;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using VSCView;

namespace ExtendInput.Controller
{
    public class XInputController : IController
    {
        public EConnectionType ConnectionType => EConnectionType.Unknown;

        public IDevice DeviceHackRef => _device;
        private XInputDevice _device;
        int stateUsageLock = 0, reportUsageLock = 0;

        public event ControllerNameUpdateEvent ControllerNameUpdated;

        ControllerState State = new ControllerState();
        ControllerState OldState = new ControllerState();

        public bool HasMotion => false;

        int Initalized;
        public XInputController(XInputDevice device)
        {
            State.Controls["quad_left"] = new ControlDPad();
            State.Controls["quad_right"] = new ControlButtonQuad();
            State.Controls["bumpers"] = new ControlButtonPair();
            State.Controls["triggers"] = new ControlTriggerPair(HasStage2: false);
            State.Controls["menu"] = new ControlButtonPair();
            State.Controls["home"] = new ControlButton();
            State.Controls["stick_left"] = new ControlStick(HasClick: true);
            State.Controls["stick_right"] = new ControlStick(HasClick: true);

            _device = device;
            Initalized = 0;

            _device.ControllerNameUpdated += OnReport;
        }

        public Image GetIcon()
        {
            Image Icon = new Bitmap(32 + 4, 16);
            Graphics g = Graphics.FromImage(Icon);

            return Icon;
        }

        public string GetName()
        {
            return _device.DevicePath;
        }

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

        private void OnReport(byte[] reportData, int reportID)
        {
            if (Initalized < 1) return;

            if (0 == Interlocked.Exchange(ref reportUsageLock, 1))
            {
                {
                    (State.Controls["stick_left"] as ControlStick).X = BitConverter.ToInt16(reportData, 4) * 1.0f / Int16.MaxValue;
                    (State.Controls["stick_left"] as ControlStick).Y = BitConverter.ToInt16(reportData, 6) * -1.0f / Int16.MaxValue;
                    (State.Controls["stick_right"] as ControlStick).X = BitConverter.ToInt16(reportData, 8) * 1.0f / Int16.MaxValue;
                    (State.Controls["stick_right"] as ControlStick).Y = BitConverter.ToInt16(reportData, 10) * -1.0f / Int16.MaxValue;

                    UInt16 buttons = BitConverter.ToUInt16(reportData, 0);

                    (State.Controls["quad_right"] as ControlButtonQuad).ButtonN = (buttons & 0x8000) == 0x8000;
                    (State.Controls["quad_right"] as ControlButtonQuad).ButtonE = (buttons & 0x2000) == 0x2000;
                    (State.Controls["quad_right"] as ControlButtonQuad).ButtonS = (buttons & 0x1000) == 0x1000;
                    (State.Controls["quad_right"] as ControlButtonQuad).ButtonW = (buttons & 0x4000) == 0x4000;

                    bool DPadUp = (buttons & 0x0001) == 0x0001;
                    bool DPadDown = (buttons & 0x0002) == 0x0002;
                    bool DPadLeft = (buttons & 0x0004) == 0x0004;
                    bool DPadRight = (buttons & 0x0008) == 0x0008;

                    if (DPadUp && DPadDown)
                        DPadUp = DPadDown = false;

                    if (DPadLeft && DPadRight)
                        DPadLeft = DPadRight = false;

                    if (DPadUp)
                    {
                        if (DPadRight)
                        {
                            (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.NorthEast;
                        }
                        else if (DPadLeft)
                        {
                            (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.NorthWest;
                        }
                        else
                        {
                            (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.North;
                        }
                    }
                    else if (DPadDown)
                    {
                        if (DPadRight)
                        {
                            (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.SouthEast;
                        }
                        else if (DPadLeft)
                        {
                            (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.SouthWest;
                        }
                        else
                        {
                            (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.South;
                        }
                    }
                    else
                    {
                        if (DPadRight)
                        {
                            (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.East;
                        }
                        else if (DPadLeft)
                        {
                            (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.West;
                        }
                        else
                        {
                            (State.Controls["quad_left"] as ControlDPad).Direction = EDPadDirection.None;
                        }
                    }


                    (State.Controls["stick_right"] as ControlStick).Click = (buttons & 0x0080) == 0x0080;
                    (State.Controls["stick_left"] as ControlStick).Click = (buttons & 0x0040) == 0x0040;
                    (State.Controls["menu"] as ControlButtonPair).Right = (buttons & 0x0010) == 0x0010;
                    (State.Controls["menu"] as ControlButtonPair).Left = (buttons & 0x0020) == 0x0020;
                    (State.Controls["bumpers"] as ControlButtonPair).Right = (buttons & 0x0200) == 0x0200;
                    (State.Controls["bumpers"] as ControlButtonPair).Left = (buttons & 0x0100) == 0x0100;

                    //(State.Controls["home"] as ControlButton).Button0 = (buttons & 0x1) == 0x1;
                    (State.Controls["triggers"] as ControlTriggerPair).L_Analog = (float)reportData[2] / byte.MaxValue;
                    (State.Controls["triggers"] as ControlTriggerPair).R_Analog = (float)reportData[3] / byte.MaxValue;

                    ControllerState NewState = GetState();
                    //OnStateUpdated(NewState);
                }
                Interlocked.Exchange(ref reportUsageLock, 0);
            }
        }

        public void Identify()
        {
            
        }

        public void Initalize()
        {
            if (Initalized > 1) return;

            HalfInitalize();

            Initalized = 2;
            _device.StartReading();
        }

        public void HalfInitalize()
        {
            if (Initalized > 0) return;

            Initalized = 1;

            if (ConnectionType == EConnectionType.Dongle)
            {
                _device.StartReading();
            }
        }

        public void DeInitalize()
        {
            if (Initalized == 0) return;

            _device.StopReading();

            Initalized = 0;
            //_device.CloseDevice();
        }
    }
}