using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSCView
{
    public class SteamControllerButtons : ICloneable
    {
        //public bool A { get; set; }
        //public bool B { get; set; }
        //public bool X { get; set; }
        //public bool Y { get; set; }

        //public bool LeftBumper { get; set; }
        //public bool LeftTrigger { get; set; }

        //public bool RightBumper { get; set; }
        //public bool RightTrigger { get; set; }

        //public bool LeftGrip { get; set; }
        //public bool RightGrip { get; set; }

        //public bool Start { get; set; }
        //public bool Home { get; set; }
        //public bool Select { get; set; }
        //public bool DS4PadClick { get; set; }

        //public bool Down { get; set; }
        //public bool Left { get; set; }
        //public bool Right { get; set; }
        //public bool Up { get; set; }

        //public bool LeftStickClick { get; set; }
        //public bool LeftPadTouch { get; set; }
        //public bool LeftPadClick { get; set; }
        //public bool RightStickClick { get; set; }
        //public bool RightPadTouch { get; set; }
        //public bool RightPadClick { get; set; }

        /// <summary>
        /// touchnw
        /// </summary>
        [Obsolete]
        public bool Touch0 { get; set; }
        /// <summary>
        /// touchne
        /// </summary>
        [Obsolete]
        public bool Touch1 { get; set; }
        /// <summary>
        /// touchsw
        /// </summary>
        [Obsolete]
        public bool Touch2 { get; set; }
        /// <summary>
        /// touchse
        /// </summary>
        [Obsolete]
        public bool Touch3 { get; set; }

        public virtual object Clone()
        {
            SteamControllerButtons buttons = (SteamControllerButtons)base.MemberwiseClone();

            //buttons.A = A;
            //buttons.B = B;
            //buttons.X = X;
            //buttons.Y = Y;

            //buttons.LeftBumper = LeftBumper;
            //buttons.LeftTrigger = LeftTrigger;

            //buttons.RightBumper = RightBumper;
            //buttons.RightTrigger = RightTrigger;

            //buttons.LeftGrip = LeftGrip;
            //buttons.RightGrip = RightGrip;

            //buttons.Start = Start;
            //buttons.Home = Home;
            //buttons.Select = Select;

            //buttons.Down = Down;
            //buttons.Left = Left;
            //buttons.Right = Right;
            //buttons.Up = Up;

            //buttons.LeftStickClick = LeftStickClick;
            //buttons.LeftPadTouch = LeftPadTouch;
            //buttons.LeftPadClick = LeftPadClick;
            //buttons.RightStickClick = RightStickClick;
            //buttons.RightPadTouch = RightPadTouch;
            //buttons.RightPadClick = RightPadClick;

            buttons.Touch0 = Touch0;
            buttons.Touch1 = Touch1;
            buttons.Touch2 = Touch2;
            buttons.Touch3 = Touch3;

            //buttons.DS4PadClick = DS4PadClick;

            return buttons;
        }
    }
    public interface IControl : ICloneable
    {
        T Value<T>(string key);
        Type Type(string key);
    }
    public class ControlCollection /*: ICloneable*/// where T : IControl
    {
        private Dictionary<string, IControl> Data = new Dictionary<string, IControl>();

        /*public string[] Keys
        {
            get
            {
                return Data.Keys.ToArray();
            }
        }*/
        public IControl this[string key]
        {
            get
            {
                if (Data.ContainsKey(key))
                    return Data[key];
                return default;
            }
            set
            {
                Data[key] = value;
            }
        }

        public object Clone()
        {
            ControlCollection newData = new ControlCollection();

            foreach (var key in Data.Keys)
            {
                newData[key] = (IControl)Data[key].Clone();
            }

            return newData;
        }
    }

    public enum EDPadDirection
    {
        None,
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest,
    }
    public class ControlTriggerPair : IControl
    {
        public bool HasStage2 { get; private set; }
        public float Analog0 { get; set; }
        public float Analog1 { get; set; }
        public bool Stage2_0 { get; set; }
        public bool Stage2_1 { get; set; }

        public ControlTriggerPair(bool HasStage2)
        {
            this.HasStage2 = HasStage2;
        }
        public T Value<T>(string key)
        {
            switch (key)
            {
                case "analog0":
                    return (T)Convert.ChangeType(Analog0, typeof(T));
                case "analog1":
                    return (T)Convert.ChangeType(Analog1, typeof(T));
                case "stage2_0":
                    return (T)Convert.ChangeType(Stage2_0, typeof(T));
                case "stage2_1":
                    return (T)Convert.ChangeType(Stage2_1, typeof(T));
                default:
                    return default;
            }
        }
        public Type Type(string key)
        {
            switch (key)
            {
                case "analog0":
                    return typeof(float);
                case "analog1":
                    return typeof(float);
                case "stage2_0":
                    return typeof(bool);
                case "stage2_1":
                    return typeof(bool);
                default:
                    return default;
            }
        }
        public object Clone()
        {
            ControlTriggerPair newData = new ControlTriggerPair(this.HasStage2);

            newData.Analog0 = this.Analog0;
            newData.Analog1 = this.Analog1;

            newData.Stage2_0 = this.Stage2_0;
            newData.Stage2_1 = this.Stage2_1;

            return newData;
        }
    }
    public class ControlTrigger : IControl
    {
        public T Value<T>(string key)
        {
            return default;
        }

        public object Clone()
        {
            ControlTrigger newData = new ControlTrigger();

            return newData;
        }

        public Type Type(string key)
        {
            return default;
        }
    }
    public class ControlDPad : IControl
    {
        //public int StateCount { get; private set; }
        public EDPadDirection Direction { get; set; }
        public ControlDPad(/*int StateCount*/)
        {
            //this.StateCount = StateCount;
        }

        public T Value<T>(string key)
        {
            switch (key)
            {
                case "n":
                    if (Direction == EDPadDirection.North) return (T)Convert.ChangeType(1, typeof(T));
                    if (Direction == EDPadDirection.NorthEast) return (T)Convert.ChangeType(1, typeof(T));
                    if (Direction == EDPadDirection.NorthWest) return (T)Convert.ChangeType(1, typeof(T));
                    return default;
                case "e":
                    if (Direction == EDPadDirection.East) return (T)Convert.ChangeType(1, typeof(T));
                    if (Direction == EDPadDirection.NorthEast) return (T)Convert.ChangeType(1, typeof(T));
                    if (Direction == EDPadDirection.SouthEast) return (T)Convert.ChangeType(1, typeof(T));
                    return default;
                case "s":
                    if (Direction == EDPadDirection.South) return (T)Convert.ChangeType(1, typeof(T));
                    if (Direction == EDPadDirection.SouthEast) return (T)Convert.ChangeType(1, typeof(T));
                    if (Direction == EDPadDirection.SouthWest) return (T)Convert.ChangeType(1, typeof(T));
                    return default;
                case "w":
                    if (Direction == EDPadDirection.West) return (T)Convert.ChangeType(1, typeof(T));
                    if (Direction == EDPadDirection.NorthWest) return (T)Convert.ChangeType(1, typeof(T));
                    if (Direction == EDPadDirection.SouthWest) return (T)Convert.ChangeType(1, typeof(T));
                    return default;
                default:
                    return default;
            }
        }
        public Type Type(string key)
        {
            return typeof(bool);
        }

        public object Clone()
        {
            ControlDPad newData = new ControlDPad();

            newData.Direction = this.Direction;

            return newData;
        }
    }
    public class ControlButtonQuad : IControl
    {
        public bool Button0 { get; set; }
        public bool Button1 { get; set; }
        public bool Button2 { get; set; }
        public bool Button3 { get; set; }

        public ControlButtonQuad()
        {
        }

        public T Value<T>(string key)
        {
            switch (key)
            {
                case "n":
                    return (T)Convert.ChangeType(Button0, typeof(T));
                case "e":
                    return (T)Convert.ChangeType(Button1, typeof(T));
                case "s":
                    return (T)Convert.ChangeType(Button2, typeof(T));
                case "w":
                    return (T)Convert.ChangeType(Button3, typeof(T));
                default:
                    return default;
            }
        }
        public Type Type(string key)
        {
            return typeof(bool);
        }

        public object Clone()
        {
            ControlButtonQuad newData = new ControlButtonQuad();

            newData.Button0 = this.Button0;
            newData.Button1 = this.Button1;
            newData.Button2 = this.Button2;
            newData.Button3 = this.Button3;

            return newData;
        }
    }
    public class ControlButtonGrid : IControl
    {
        public bool[,] Button { get; set; }
        private int Width;
        private int Height;

        public ControlButtonGrid(int width, int height)
        {
            Width = width;
            Height = height;
            Button = new bool[Width, Height];
        }

        public T Value<T>(string key)
        {
            switch (key)
            {
                case "width":
                    return (T)Convert.ChangeType(Width, typeof(T));
                case "height":
                    return (T)Convert.ChangeType(Height, typeof(T));
                default:
                try
                {
                    string[] parts = key.Split(':');
                    return (T)Convert.ChangeType(Button[int.Parse(parts[0]), int.Parse(parts[1])], typeof(T));
                }
                catch
                {
                    return default;
                }
            }
        }
        public Type Type(string key)
        {
            return typeof(bool);
        }

        public object Clone()
        {
            ControlButtonGrid newData = new ControlButtonGrid(Width, Height);

            for (int w = 0; w < Width; w++)
                for (int h = 0; h < Height; h++)
                    newData.Button[w, h] = this.Button[w, h];

            return newData;
        }
    }
    public class ControlButtonPair : IControl
    {
        public bool Button0 { get; set; }
        public bool Button1 { get; set; }

        public ControlButtonPair()
        {
        }

        public T Value<T>(string key)
        {
            switch (key)
            {
                case "l":
                    return (T)Convert.ChangeType(Button0, typeof(T));
                case "r":
                    return (T)Convert.ChangeType(Button1, typeof(T));
                default:
                    return default;
            }
        }
        public Type Type(string key)
        {
            return typeof(bool);
        }

        public object Clone()
        {
            ControlButtonPair newData = new ControlButtonPair();

            newData.Button0 = this.Button0;
            newData.Button1 = this.Button1;

            return newData;
        }
    }
    public class ControlButton : IControl
    {
        public bool Button0 { get; set; }
        public T Value<T>(string key)
        {
            return (T)Convert.ChangeType(Button0, typeof(T));
            //return default;
        }
        public Type Type(string key)
        {
            return typeof(bool);
        }

        public object Clone()
        {
            ControlButton newData = new ControlButton();

            newData.Button0 = this.Button0;

            return newData;
        }
    }
    public class ControlStick : IControl
    {
        public bool HasClick { get; private set; }
        public float X { get; set; }
        public float Y { get; set; }
        public bool Click { get; internal set; }

        public ControlStick(bool HasClick)
        {
            this.HasClick = HasClick;
        }

        public T Value<T>(string key)
        {
            switch (key)
            {
                case "x":
                    return (T)Convert.ChangeType(X, typeof(T));
                case "y":
                    return (T)Convert.ChangeType(Y, typeof(T));
                case "click":
                    return (T)Convert.ChangeType(Click, typeof(T));
                default:
                    return default;
            }
        }
        public Type Type(string key)
        {
            switch (key)
            {
                case "x":
                    return typeof(float);
                case "y":
                    return typeof(float);
                case "click":
                    return typeof(bool);
                default:
                    return default;
            }
        }

        public object Clone()
        {
            ControlStick newData = new ControlStick(this.HasClick);

            newData.X = this.X;
            newData.Y = this.Y;
            newData.Click = this.Click;

            return newData;
        }
    }
    public class ControlTouch : IControl
    {
        public bool HasClick { get; private set; }
        public int TouchCount { get; private set; }
        public float[] X { get; private set; }
        public float[] Y { get; private set; }
        public bool[] Touch { get; private set; }
        public bool Click { get; set; }

        public ControlTouch(int TouchCount, bool HasClick)
        {
            this.TouchCount = TouchCount;
            this.HasClick = HasClick;

            this.X = new float[TouchCount];
            this.Y = new float[TouchCount];
            this.Touch = new bool[TouchCount];
            this.Click = false;
        }

        public T Value<T>(string key)
        {
            if(key == "click")
                return (T)Convert.ChangeType(Click, typeof(T));

            for (int i = 0; i < TouchCount; i++)
            {
                if (key == $"x{i}")
                    return (T)Convert.ChangeType(X[i], typeof(T));

                if (key == $"y{i}")
                    return (T)Convert.ChangeType(Y[i], typeof(T));

                if (key == $"touch{i}")
                    return (T)Convert.ChangeType(Touch[i], typeof(T));
            }

            return default;
        }
        public Type Type(string key)
        {
            if (key == "click")
                return typeof(bool);

            for (int i = 0; i < TouchCount; i++)
            {
                if (key == $"x{i}")
                    return typeof(float);

                if (key == $"y{i}")
                    return typeof(float);

                if (key == $"touch{i}")
                    return typeof(bool);
            }

            return default;
        }

        public object Clone()
        {
            ControlTouch newData = new ControlTouch(this.TouchCount, this.HasClick);

            newData.Click = this.Click;

            for (int i = 0; i < this.TouchCount; i++)
            {
                // taking advantage of the fact it's an array, so the private setter doesn't stop us
                newData.Touch[i] = this.Touch[i];
                newData.X[i] = this.X[i];
                newData.Y[i] = this.Y[i];
            }

            return newData;
        }

        public void AddTouch(int idx, bool touch, float x, float y, byte timedeltams)
        {
            //Console.WriteLine($"{idx}\t{touch}\t{x}\t{y}\t{timedelta}");

            Touch[idx] = touch;
            X[idx] = x;
            Y[idx] = y;
        }
    }


    /// <summary>
    /// This motion data will change drasticly in later versions
    /// </summary>
    public class ControlMotion : IControl
    {
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
        

        public ControlMotion()
        {
        }

        public T Value<T>(string key)
        {
            if (key == "accelerometer:x")
                return (T)Convert.ChangeType(AccelerometerX, typeof(T));
            if (key == "accelerometer:y")
                return (T)Convert.ChangeType(AccelerometerY, typeof(T));
            if (key == "accelerometer:z")
                return (T)Convert.ChangeType(AccelerometerZ, typeof(T));
            if (key == "angularVelocity:x")
                return (T)Convert.ChangeType(AngularVelocityX, typeof(T));
            if (key == "angularVelocity:y")
                return (T)Convert.ChangeType(AngularVelocityY, typeof(T));
            if (key == "angularVelocity:z")
                return (T)Convert.ChangeType(AngularVelocityZ, typeof(T));
            if (key == "orientation:w")
                return (T)Convert.ChangeType(OrientationW, typeof(T));
            if (key == "orientation:x")
                return (T)Convert.ChangeType(OrientationX, typeof(T));
            if (key == "orientation:y")
                return (T)Convert.ChangeType(OrientationY, typeof(T));
            if (key == "orientation:z")
                return (T)Convert.ChangeType(OrientationZ, typeof(T));

            return default;
        }
        public Type Type(string key)
        {
            switch (key)
            {
                case "accelerometer:x":
                case "accelerometer:y":
                case "accelerometer:z":
                case "angularVelocity:x":
                case "angularVelocity:y":
                case "angularVelocity:z":
                case "orientation:w":
                case "orientation:x":
                case "orientation:y":
                case "orientation:z":
                    return typeof(Int16);
            }

            return default;
        }

        public object Clone()
        {
            ControlMotion newData = new ControlMotion();

            newData.AccelerometerX = this.AccelerometerX;
            newData.AccelerometerY = this.AccelerometerY;
            newData.AccelerometerZ = this.AccelerometerZ;
            newData.AngularVelocityX = this.AngularVelocityX;
            newData.AngularVelocityY = this.AngularVelocityY;
            newData.AngularVelocityZ = this.AngularVelocityZ;
            newData.OrientationW = this.OrientationW;
            newData.OrientationX = this.OrientationX;
            newData.OrientationY = this.OrientationY;
            newData.OrientationZ = this.OrientationZ;

            return newData;
        }
    }

    public class ControllerState : ICloneable
    {
        public ControlCollection Controls { get; private set; }

        public ControllerState()
        {
            Controls = new ControlCollection();
        }
        
        public object Clone()
        {
            ControllerState newState = new ControllerState();

            newState.Controls = (ControlCollection)this.Controls.Clone();

            return newState;
        }
    }
}
