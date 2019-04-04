using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSCView
{
    public class UI
    {
        public int Height { get; private set; }
        public int Width { get; private set; }

        private UI_ImageCache cache;
        private ControllerData data;
        private List<UI_Item> Items;

        public UI(ControllerData data, string themePath, string json)
        {
            JObject themeData = JObject.Parse(json);
            Initalize(data, themePath, themeData);
        }

        public UI(ControllerData data, string themePath, JObject themeData)
        {
            Initalize(data, themePath, themeData);
        }

        private void Initalize(ControllerData data, string themePath, JObject themeData)
        {
            this.data = data;
            cache = new UI_ImageCache(themePath);
            Items = new List<UI_Item>();

            Height = themeData["height"]?.Value<int>() ?? 100;
            Width = themeData["width"]?.Value<int>() ?? 100;

            themeData["children"]?.ToList().ForEach(child =>
            {
                string uiType = child["type"]?.Value<string>() ?? "";

                switch (uiType)
                {
                    case "":
                        Items.Add(new UI_Item(data, cache, themePath, (JObject)child));
                        break;
                    case "image":
                        Items.Add(new UI_GraphicalItem(data, cache, themePath, (JObject)child));
                        break;
                    case "showhide":
                        Items.Add(new UL_ShowHide(data, cache, themePath, (JObject)child));
                        break;
                    case "slider":
                        Items.Add(new UL_Slider(data, cache, themePath, (JObject)child));
                        break;
                    case "trailpad":
                        Items.Add(new UL_TrailPad(data, cache, themePath, (JObject)child));
                        break;
                    case "pbar":
                        Items.Add(new UL_PBar(data, cache, themePath, (JObject)child));
                        break;
                    case "basic3d1":
                        Items.Add(new UL_Basic3D1(data, cache, themePath, (JObject)child));
                        break;
                    default:
                        throw new Exception("Unknown UI Widget Type");
                }
            });
        }

        public void Paint(Graphics graphics)
        {
            foreach(UI_Item item in Items)
            {
                item.Paint(graphics);
            }
        }
    }

    public class ControllerData
    {
        SteamController ActiveController;

        public bool GetBasicControl(string inputName)
        {
            if (ActiveController == null) return false;

            inputName = inputName.ToLowerInvariant();

            SteamController.SteamControllerState state = ActiveController.GetState();

            switch (inputName)
            {
                case "a":
                    return state.Buttons.A;
                case "b":
                    return state.Buttons.B;
                case "x":
                    return state.Buttons.X;
                case "y":
                    return state.Buttons.Y;

                case "leftbumper":
                case "lb":
                    return state.Buttons.LeftBumper;
                case "lefttrigger":
                case "lt":
                    return state.Buttons.LeftTrigger;

                case "rightbumper":
                case "rb":
                    return state.Buttons.RightBumper;
                case "righttrigger":
                case "rt":
                    return state.Buttons.RightTrigger;

                case "leftgrip":
                case "lg":
                    return state.Buttons.LeftGrip;
                case "rightgrip":
                case "rg":
                    return state.Buttons.RightGrip;

                case "start":
                    return state.Buttons.Start;
                case "steam":
                    return state.Buttons.Steam;
                case "select":
                    return state.Buttons.Select;

                case "down":
                    return state.Buttons.Down;
                case "left":
                    return state.Buttons.Left;
                case "right":
                    return state.Buttons.Right;
                case "up":
                    return state.Buttons.Up;

                case "stickclick":
                case "sc":
                    return state.Buttons.StickClick;
                case "leftpadtouch":
                case "lpt":
                    return state.Buttons.LeftPadTouch;
                case "leftpadclick":
                case "lpc":
                    return state.Buttons.LeftPadClick;
                case "rightpadtouch":
                case "rpt":
                    return state.Buttons.RightPadTouch;
                case "rightpadclick":
                case "rpc":
                    return state.Buttons.RightPadClick;
                default:
                    return false;
            }
        }

        public Int32 GetAnalogControl(string inputName)
        {
            if (ActiveController == null) return 0;

            inputName = inputName.ToLowerInvariant();

            SteamController.SteamControllerState state = ActiveController.GetState();

            switch (inputName)
            {
                case "leftpadx": return state.LeftPadX;
                case "leftpady": return state.LeftPadY;

                case "rightpadx": return state.RightPadX;
                case "rightpady": return state.RightPadY;

                case "leftstickx": return state.LeftStickX;
                case "leftsticky": return state.LeftStickY;

                case "lefttrigger": return state.LeftTrigger;
                case "righttrigger": return state.RightTrigger;

                case "angularvelocityx": return state.AngularVelocityX;
                case "angularvelocityy": return state.AngularVelocityY;
                case "angularvelocityz": return state.AngularVelocityZ;

                // consider orientation data here, but it's a quaternion so that's complicated

                default: return 0;
            }
        }

        public SteamController.SteamControllerState GetState()
        {
            if (ActiveController == null) return null;
            return ActiveController.GetState();
        }

        public void SetController(SteamController ActiveController)
        {
            this.ActiveController = ActiveController;
        }
    }

    public class UI_ImageCache : IDisposable
    {
        private string themePath;
        private Dictionary<string, Image> cache;

        public UI_ImageCache(string themePath)
        {
            this.themePath = themePath;
            this.cache = new Dictionary<string, Image>();
        }

        private bool disposed = false;
        ~UI_ImageCache()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                foreach(var key in cache.Keys)
                {
                    cache[key].Dispose();
                    cache[key] = null;
                }
            }

            // Free any unmanaged objects here.
            //
            cache = null;
            disposed = true;
        }

        public Image GetImage(string Key, Func<Image> ImageLoader)
        {
            lock(cache)
            {
                if (cache.ContainsKey(Key)) return cache[Key];
                cache[Key] = ImageLoader();
                return cache[Key];
            }
        }

        public Image LoadImage(string name)
        {
            // load the image for the active theme
            string ImagePath = Path.Combine("themes", themePath, name);

            // this will throw an exception if the file or path doesn't exist
            return Image.FromFile(ImagePath);
        }

        public static Image SetImageOpacity(Image image, float opacity)
        {
            try
            {
                //create a Bitmap the size of the image provided
                Bitmap bmp = new Bitmap(image.Width, image.Height);

                //create a graphics object from the image
                using (Graphics gfx = Graphics.FromImage(bmp))
                {

                    //create a color matrix object
                    ColorMatrix matrix = new ColorMatrix();

                    //set the opacity
                    matrix.Matrix33 = opacity;

                    //create image attributes
                    ImageAttributes attributes = new ImageAttributes();

                    //set the color(opacity) of the image
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    //now draw the image
                    gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);

                    bmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                }
                return bmp;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }

    public class UI_Item
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Rot { get; private set; }

        private UI_ImageCache cache;
        private List<UI_Item> Items;

        //public UI_Item(UI_ImageCache cache, string themePath, string json)
        //{
        //    JObject themeData = JObject.Parse(json);
        //    Initalize(cache, themePath, themeData);
        //}

        public UI_Item(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            Initalize(data, cache, themePath, themeData);
        }

        protected virtual void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            this.cache = cache;
            Items = new List<UI_Item>();

            X = themeData["x"]?.Value<float>() ?? 0;
            Y = themeData["y"]?.Value<float>() ?? 0;
            Rot = themeData["rot"]?.Value<float>() ?? 0;

            themeData["children"]?.ToList().ForEach(child =>
            {
                string uiType = child["type"]?.Value<string>() ?? "";

                switch (uiType)
                {
                    case "":
                        Items.Add(new UI_Item(data, cache, themePath, (JObject)child));
                        break;
                    case "image":
                        Items.Add(new UI_GraphicalItem(data, cache, themePath, (JObject)child));
                        break;
                    case "showhide":
                        Items.Add(new UL_ShowHide(data, cache, themePath, (JObject)child));
                        break;
                    case "slider":
                        Items.Add(new UL_Slider(data, cache, themePath, (JObject)child));
                        break;
                    case "trailpad":
                        Items.Add(new UL_TrailPad(data, cache, themePath, (JObject)child));
                        break;
                    case "pbar":
                        Items.Add(new UL_PBar(data, cache, themePath, (JObject)child));
                        break;
                    case "basic3d1":
                        Items.Add(new UL_Basic3D1(data, cache, themePath, (JObject)child));
                        break;
                    default:
                        throw new Exception("Unknown UI Widget Type");
                }
            });
        }

        public virtual void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;
            graphics.TranslateTransform(X, Y);
            graphics.RotateTransform(Rot);

            foreach (UI_Item item in Items)
            {
                item.Paint(graphics);
            }

            graphics.Transform = preserve;
        }
    }

    public class UI_GraphicalItem : UI_Item
    {
        public UI_GraphicalItem(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData) : base(data, cache, themePath, themeData)
        {
        }

        public float Height { get; private set; }
        public float Width { get; private set; }
        public bool DrawFromCenter { get; private set; }

        protected Image DisplayImage;

        protected override void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            base.Initalize(data, cache, themePath, themeData);

            Height = themeData["height"]?.Value<float>() ?? 0;
            Width = themeData["width"]?.Value<float>() ?? 0;
            DrawFromCenter = themeData["center"]?.Value<bool>() ?? false;

            string imageName = themeData["image"]?.Value<string>();

            if (!string.IsNullOrWhiteSpace(imageName))
            {
                DisplayImage = cache.LoadImage(imageName);
            }
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;
            if (DrawFromCenter)
                graphics.TranslateTransform(-Width / 2, -Height / 2);

            if (DisplayImage != null)
            {
                if(DisplayImage != null)
                    graphics.DrawImage(DisplayImage, X, Y, Width, Height);
            }

            graphics.Transform = preserve;

            //if (DrawFromCenter)
            //    graphics.TranslateTransform(X, Y);

            base.Paint(graphics);

            //graphics.Transform = preserve;
        }
    }

    public class UL_ShowHide : UI_Item
    {
        public UL_ShowHide(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData) : base(data, cache, themePath, themeData)
        {
        }

        private ControllerData data;
        private string InputName;

        private bool output;

        protected override void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            base.Initalize(data, cache, themePath, themeData);
            this.data = data;

            output = true;

            InputName = themeData["inputName"]?.Value<string>();
            output = !(themeData["invert"]?.Value<bool>() ?? false);
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;

            if (string.IsNullOrWhiteSpace(InputName))
            {
                if (output) base.Paint(graphics);
            }
            else if (data.GetBasicControl(InputName))
            {
                if (output) base.Paint(graphics);
            }
            else
            {
                if (!output) base.Paint(graphics);
            }
        }
    }

    public class UL_Slider : UI_Item
    {
        public UL_Slider(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData) : base(data, cache, themePath, themeData)
        {
        }

        private ControllerData data;
        protected string AxisNameX;
        protected string AxisNameY;
        protected float ScaleFactorX;
        protected float ScaleFactorY;

        protected override void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            base.Initalize(data, cache, themePath, themeData);
            this.data = data;

            AxisNameX = themeData["axisNameX"]?.Value<string>();
            AxisNameY = themeData["axisNameY"]?.Value<string>();

            ScaleFactorX = themeData["scaleFactorX"]?.Value<float>() ?? 0;
            ScaleFactorY = themeData["scaleFactorY"]?.Value<float>() ?? 0;
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;

            float AnalogX = string.IsNullOrWhiteSpace(AxisNameX) ? 0 : (data.GetAnalogControl(AxisNameX) * ScaleFactorX);
            float AnalogY = string.IsNullOrWhiteSpace(AxisNameY) ? 0 : (data.GetAnalogControl(AxisNameY) * ScaleFactorY);

            graphics.TranslateTransform(AnalogX, -AnalogY);

            base.Paint(graphics);

            graphics.Transform = preserve;
        }
    }

    public class UL_TrailPad : UL_Slider
    {
        public UL_TrailPad(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData) : base(data, cache, themePath, themeData)
        {
        }

        private ControllerData data;
        private Image[] ImagePadDecay;
        private List<PointF?> PadPosHistory;
        private string InputName;

        protected override void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            base.Initalize(data, cache, themePath, themeData);
            this.data = data;

            PadPosHistory = new List<PointF?>();

            InputName = themeData["inputName"]?.Value<string>();
            string imageName = themeData["image"]?.Value<string>();
            int TrailLength = themeData["length"]?.Value<int>()??0;

            if (!string.IsNullOrWhiteSpace(imageName) && TrailLength > 0)
            {
                Image ImagePadDecayBase = cache.LoadImage(imageName);
                ImagePadDecay = new Image[TrailLength];
                for (int x = 0; x < ImagePadDecay.Length; x++)
                {
                    float percent = ((x + 1) * 1.0f / ImagePadDecay.Length);

                    ImagePadDecay[x] = cache.GetImage($"{imageName}:{percent}", () => { return UI_ImageCache.SetImageOpacity(ImagePadDecayBase, percent * 0.15f); });
                }
            }
            else
            {
                ImagePadDecay = new Image[0];
            }
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;
            graphics.TranslateTransform(X, Y);

            bool ControlHot = data.GetBasicControl(InputName);

            PointF? prevCord = null;
            for (int pointfade = 0; pointfade < PadPosHistory.Count && pointfade < ImagePadDecay.Length; pointfade++)
            {
                PointF? cord = PadPosHistory[pointfade];
                if (cord.HasValue)
                {
                    float SubPointWidth = ImagePadDecay[pointfade].Width;
                    float SubPointHeight = ImagePadDecay[pointfade].Height;

                    float ratio = (pointfade * 1.0f / ImagePadDecay.Length);
                    ratio = ratio * 0.5f + 0.5f;
                    SubPointWidth *= ratio;
                    SubPointHeight *= ratio;

                    graphics.DrawImage(ImagePadDecay[pointfade], cord.Value.X - (SubPointWidth / 2.0f), cord.Value.Y - (SubPointHeight / 2.0f), SubPointWidth, SubPointHeight);
                    if (prevCord.HasValue)
                    {
                        // draw extra dots between points we've seen
                        double xVector = cord.Value.X - prevCord.Value.X;
                        double yVector = cord.Value.Y - prevCord.Value.Y;

                        double distance = Math.Sqrt(xVector * xVector + yVector * yVector);
                        double subdist = distance * 0.5f;

                        if ((int)subdist > 0)
                        {
                            double distBetweenDots = distance / subdist;
                            float SubPoint2Width = ImagePadDecay[pointfade - 1].Width;
                            float SubPoint2Height = ImagePadDecay[pointfade - 1].Height;

                            //float ratio = (pointfade * 1.0f / ImagePadDecay.Length);
                            SubPoint2Width *= ratio;
                            SubPoint2Height *= ratio;

                            for (int subDistPlot = 0; subDistPlot < subdist; subDistPlot++)
                            {
                                PointF betweenPoint = new PointF(
                                    (float)(prevCord.Value.X + xVector * subDistPlot / subdist),
                                    (float)(prevCord.Value.Y + yVector * subDistPlot / subdist));

                                //Matrix preserve2 = graphics.Transform;
                                //graphics.TranslateTransform(X, Y);
                                graphics.DrawImage(ImagePadDecay[pointfade - 1], betweenPoint.X - (SubPoint2Width / 2.0f), betweenPoint.Y - (SubPoint2Height / 2.0f), SubPoint2Width, SubPoint2Height);
                                //graphics.Transform = preserve2;
                            }
                        }
                    }
                }
                prevCord = cord;
            }


            if (PadPosHistory.Count >= ImagePadDecay.Length && PadPosHistory.Count > 0) PadPosHistory.RemoveAt(0);

            float AnalogX = string.IsNullOrWhiteSpace(AxisNameX) ? 0 : (data.GetAnalogControl(AxisNameX) * ScaleFactorX);
            float AnalogY = string.IsNullOrWhiteSpace(AxisNameY) ? 0 : (data.GetAnalogControl(AxisNameY) * ScaleFactorY);

            if (string.IsNullOrWhiteSpace(InputName) || data.GetBasicControl(InputName))
            {
                PointF cord = new PointF(AnalogX,-AnalogY);
                PadPosHistory.Add(cord);
            }
            else
            {
                PadPosHistory.Add(null);
            }

            graphics.Transform = preserve;

            base.Paint(graphics);
        }
    }

    public class UL_PBar : UI_Item
    {
        public UL_PBar(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData) : base(data, cache, themePath, themeData)
        {
        }

        private ControllerData data;
        protected string AxisName;
        protected string Direction;
        protected int Min;
        protected int Max;
        protected float Width;
        protected float Height;
        protected Color Foreground;
        protected Color Background;

        protected override void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            base.Initalize(data, cache, themePath, themeData);
            this.data = data;

            Background = Color.White;
            Foreground = Color.Black ;

            AxisName = themeData["axisName"]?.Value<string>();
            Direction = themeData["direction"]?.Value<string>();

            Min = themeData["min"]?.Value<int>() ?? 0;
            Max = themeData["max"]?.Value<int>() ?? 0;
            Width = themeData["width"]?.Value<float>() ?? 0;
            Height = themeData["height"]?.Value<float>() ?? 0;

            string ForegroundCode = themeData["foreground"]?.Value<string>();
            string BackgroundCode = themeData["background"]?.Value<string>();

            try
            {
                Foreground = Color.FromArgb(int.Parse(ForegroundCode, System.Globalization.NumberStyles.HexNumber));
            }
            catch { }
            try
            {
                Background = Color.FromArgb(int.Parse(BackgroundCode, System.Globalization.NumberStyles.HexNumber));
            }
            catch { }
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;

            float Analog = string.IsNullOrWhiteSpace(AxisName) ? 0 : (data.GetAnalogControl(AxisName));
            Analog = Math.Max(Math.Min((Analog - Min) / (Max - Min), 1.0f), 0.0f);

            graphics.TranslateTransform(X, Y);
            graphics.TranslateTransform(-Width / 2, -Height / 2);

            switch (Direction)
            {
                case "up":
                    graphics.FillRectangle(new SolidBrush(Background), 0, Height - (Height * Analog), Width, Height * Analog);
                    break;
                case "down":
                    graphics.FillRectangle(new SolidBrush(Background), 0, 0, Width, Height * Analog);
                    break;
                case "left":
                    graphics.FillRectangle(new SolidBrush(Background), Width - (Width * Analog), 0, Width * Analog, Height);
                    break;
                default:
                    graphics.FillRectangle(new SolidBrush(Background), 0, 0, Width * Analog, Height);
                    break;
            }

            graphics.DrawRectangle(new Pen(Foreground, 2), 0, 0, Width, Height);

            graphics.Transform = preserve;

            base.Paint(graphics);
        }
    }

    public class UL_Basic3D1 : UI_Item
    {
        public UL_Basic3D1(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData) : base(data, cache, themePath, themeData)
        {
        }

        private ControllerData data;
        private UI_ImageCache cache;
        protected string DisplayType;
        protected Image DisplayImage;
        protected Image ShadowL;
        protected Image ShadowR;
        protected Image ShadowU;
        protected Image ShadowD;
        protected float Width;
        protected float Height;

        float TiltTranslateX;
        float TiltTranslateY;

        string ShadowLName;
        string ShadowRName;
        string ShadowUName;
        string ShadowDName;

        protected override void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            base.Initalize(data, cache, themePath, themeData);
            this.data = data;
            this.cache = cache;

            DisplayType = themeData["mode"]?.Value<string>();
            string ImageName = themeData["image"]?.Value<string>();
            ShadowLName = themeData["shadowl"]?.Value<string>();
            ShadowRName = themeData["shadowr"]?.Value<string>();
            ShadowUName = themeData["shadowu"]?.Value<string>();
            ShadowDName = themeData["shadowd"]?.Value<string>();

            if (!string.IsNullOrWhiteSpace(ImageName))
                DisplayImage = cache.LoadImage(ImageName);

            if (!string.IsNullOrWhiteSpace(ShadowLName))
                ShadowL = cache.LoadImage(ShadowLName);

            if (!string.IsNullOrWhiteSpace(ShadowRName))
                ShadowR = cache.LoadImage(ShadowRName);

            if (!string.IsNullOrWhiteSpace(ShadowUName))
                ShadowU = cache.LoadImage(ShadowUName);

            if (!string.IsNullOrWhiteSpace(ShadowDName))
                ShadowD = cache.LoadImage(ShadowDName);

            Width = themeData["width"]?.Value<float>() ?? 0;
            Height = themeData["height"]?.Value<float>() ?? 0;

            TiltTranslateX = themeData["tilttranslatex"]?.Value<float>() ?? 0;
            TiltTranslateY = themeData["tilttranslatey"]?.Value<float>() ?? 0;
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;

            graphics.TranslateTransform(X, Y);
            //graphics.TranslateTransform(-Width / 2, -Height / 2);

            SteamController.SteamControllerState State = data.GetState();
            if (State != null)
            {
                int AccelMagX = State.AngularVelocityX, AccelMagY = State.AngularVelocityY, AccelMagZ = State.AngularVelocityZ;
                int GyroW = State.OrientationW, GyroX = State.OrientationX, GyroY = State.OrientationY, GyroZ = State.OrientationZ;

                float AccelTiltFactorX = AccelMagX * 0.0001f;
                float AccelTiltFactorY = AccelMagY * 0.0001f;
                float AccelRotationAngle = AccelMagZ * 0.0001f * -90;
                double GyroNormX = GyroX * 1.0f / 32768;
                double GyroNormY = GyroY * 1.0f / 32768;
                double GyroNormZ = GyroZ * 1.0f / 32768;
                double GyroNormW = GyroW * 1.0f / 32768;

                // use MahonyFilter integration with some sane coefficients (8ms. sampling @ 125hz)
                MahonyFilter gyroFilter = new MahonyFilter((1000 / 125.0f / 100), 1f, 0f);
                float[] _fIMU = gyroFilter.UpdateIMU((float)GyroNormX, (float)GyroNormY, (float)GyroNormZ, AccelMagX, AccelMagY, AccelMagZ);

                switch (DisplayType)
                {
                    case "accel":
                        {
                            float AccelTransformX = 1.0f - Math.Abs(AccelTiltFactorY * 0.5f);
                            float AccelTransformY = 1.0f - Math.Abs(AccelTiltFactorX * 0.5f);

                            Draw3dAs3d(
                                cache, graphics, DisplayImage, ShadowLName, ShadowL, ShadowRName, ShadowR, ShadowUName, ShadowU, ShadowDName, ShadowD,
                                AccelTransformX, AccelTransformY, AccelRotationAngle, AccelTiltFactorX, AccelTiltFactorY,
                                Width, Height, TiltTranslateX, TiltTranslateY
                            );

                            graphics.ResetTransform();
                        }
                        break;
                    case "gyro":
                        {
                            // flip input axis around to correct 2d->3d rendered orientation
                            double[] eulerAngles = ToEulerAngles(GyroNormW, GyroNormY, GyroNormZ, GyroNormX);

                            double Yaw = eulerAngles[0] * 2.0f / Math.PI;
                            double Pitch = eulerAngles[1] * 2.0f / Math.PI;
                            double Roll = -(eulerAngles[2] * 2.0f / Math.PI);
                            if (double.IsNaN(Yaw)) Yaw = 0;
                            if (double.IsNaN(Pitch)) Pitch = 0;
                            if (double.IsNaN(Roll)) Roll = 0;

                            int SignY = -Math.Sign((2 * mod(Math.Floor((Roll - 1) * 0.5f) + 1, 2)) - 1);
                            float GyroTiltFactorX = (float)((2 * Math.Abs(mod((Pitch - 1) * 0.5f, 2) - 1)) - 1);
                            float GyroTiltFactorY = (float)((2 * Math.Abs(mod((Roll - 1) * 0.5f, 2) - 1)) - 1);
                            float GyroRotationAngle = (float)(Yaw * -90.0f);
                            float GyroTransformX = SignY * Math.Max(1.0f - Math.Abs(GyroTiltFactorY), 0.15f);
                            float GyroTransformY = Math.Max(1.0f - Math.Abs(GyroTiltFactorX), 0.15f);

                            //Console.WriteLine($"{TiltFactorY}\t{Roll}\t{(2 * mod(Math.Floor((Roll - 1) * 0.5f) + 1, 2)) - 1}");

                            Draw3dAs3d(
                                cache, graphics, DisplayImage, ShadowLName, ShadowL, ShadowRName, ShadowR, ShadowUName, ShadowU, ShadowDName, ShadowD,
                                GyroTransformX, GyroTransformY, GyroRotationAngle, GyroTiltFactorX, GyroTiltFactorY,
                                Width, Height, TiltTranslateX, TiltTranslateY
                            );

                            graphics.ResetTransform();
                        }
                        break;
                    default:
                        break;
                }
            }

            graphics.Transform = preserve;

            base.Paint(graphics);
        }

        public static double[] ToEulerAngles(double QuaternionW, double QuaternionX, double QuaternionY, double QuaternionZ)
        {
            double sqw = QuaternionW * QuaternionW;
            double sqx = QuaternionX * QuaternionX;
            double sqy = QuaternionY * QuaternionY;
            double sqz = QuaternionZ * QuaternionZ;
            double Yaw, Pitch, Roll;

            // If quaternion is normalised the unit is one, otherwise it is the correction factor
            double unit = sqx + sqy + sqz + sqw;
            double test = QuaternionX * QuaternionY + QuaternionZ * QuaternionW;

            if (test > 0.4999f * unit)                              // 0.4999f OR 0.5f - EPSILON
            {
                // Singularity at north pole
                Yaw = 2f * Math.Atan2(QuaternionX, QuaternionW);  // Yaw
                Pitch = Math.PI * 0.5f;                         // Pitch
                Roll = 0f;                                // Roll
            }
            else if (test < -0.4999f * unit)                        // -0.4999f OR -0.5f + EPSILON
            {
                // Singularity at south pole
                Yaw = -2f * Math.Atan2(QuaternionX, QuaternionW); // Yaw
                Pitch = -Math.PI * 0.5f;                        // Pitch
                Roll = 0f;                                // Roll
            }
            else
            {
                Yaw = Math.Atan2(2f * QuaternionY * QuaternionW - 2f * QuaternionX * QuaternionZ, sqx - sqy - sqz + sqw);       // Yaw
                Pitch = Math.Asin(2f * test / unit);                                             // Pitch
                Roll = Math.Atan2(2f * QuaternionX * QuaternionW - 2f * QuaternionY * QuaternionZ, -sqx + sqy - sqz + sqw);      // Roll
            }
            return new double[] { Yaw, Pitch, Roll };
        }

        double mod(double x, double m)
        {
            return (x % m + m) % m;
        }

        private void Draw3dAs3d(
            UI_ImageCache cache, Graphics g,
            Image image, string ImageGyroLName, Image ImageGyroL, string ImageGyroRName, Image ImageGyroR, string ImageGyroUName, Image ImageGyroU, string ImageGyroDName, Image ImageGyroD,
            float transformX, float transformY,
            float rotationAngle,
            float TiltFactorX, float TiltFactorY,
            float Width, float Height,
            float TiltTranslateX, float TiltTranslateY)
        {
            // fix flip
            if (TiltFactorX < -1) TiltFactorX = 2.0f + TiltFactorX;
            if (TiltFactorX > 1) TiltFactorX = 2.0f - TiltFactorX;
            if (TiltFactorY < -1) TiltFactorY = 2.0f + TiltFactorY;
            if (TiltFactorY > 1) TiltFactorY = 2.0f - TiltFactorY;

            PointF location = new PointF((TiltFactorY * -TiltTranslateX) - (Width / 2), (TiltFactorX * TiltTranslateY) - (Height / 2));

            g.RotateTransform(rotationAngle);
            g.ScaleTransform(transformX, transformY);

            g.DrawImage(image, location.X, location.Y, Width, Height);

            if (Math.Abs(TiltFactorX) > 0)
            {
                if (Math.Sign(TiltFactorX) > 0) {
                    float percent = TiltFactorX * 2;// * 0.15f;
                    percent = (float)Math.Round(percent * 25f) / 25f;
                    percent = (float)Math.Min(percent, 1.0f);
                    g.DrawImage(cache.GetImage($"{ImageGyroDName}:{percent}", () => { return UI_ImageCache.SetImageOpacity(ImageGyroD, percent); }), location.X, location.Y, Width, Height);
                }
                if (Math.Sign(TiltFactorX) < 0) {
                    float percent = -TiltFactorX * 2;// * 0.15f;
                    percent = (float)Math.Round(percent * 25f) / 25f;
                    percent = (float)Math.Min(percent, 1.0f);
                    g.DrawImage(cache.GetImage($"{ImageGyroUName}:{percent}", () => { return UI_ImageCache.SetImageOpacity(ImageGyroU, percent); }), location.X, location.Y, Width, Height);
                }
            }

            if (Math.Abs(TiltFactorY) > 0)
            {
                if (Math.Sign(TiltFactorY) > 0) {
                    float percent = TiltFactorY * 2;// * 0.15f;
                    percent = (float)Math.Round(percent * 25f) / 25f;
                    percent = (float)Math.Min(percent, 1.0f);
                    g.DrawImage(cache.GetImage($"{ImageGyroLName}:{percent}", () => { return UI_ImageCache.SetImageOpacity(ImageGyroL, percent); }), location.X, location.Y, Width, Height);
                }
                if (Math.Sign(TiltFactorY) < 0) {
                    float percent = -TiltFactorY * 2;// * 0.15f;
                    percent = (float)Math.Round(percent * 25f) / 25f;
                    percent = (float)Math.Min(percent, 1.0f);
                    g.DrawImage(cache.GetImage($"{ImageGyroRName}:{percent}", () => { return UI_ImageCache.SetImageOpacity(ImageGyroR, percent); }), location.X, location.Y, Width, Height);
                }
            }
        }

    }
}
