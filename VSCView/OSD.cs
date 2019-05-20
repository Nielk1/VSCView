﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;

namespace VSCView
{
    public class UI : IDisposable
    {
        public int Height { get; private set; }
        public int Width { get; private set; }

        private UI_ImageCache cache;
        private ControllerData data;
        private List<UI_Item> Items;

        bool disposed = false;

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

        public void Update()
        {
            foreach (UI_Item item in Items)
            {
                item.Update();
            }
        }

        public bool IsDirty()
        {
            foreach (UI_Item item in Items)
            {
                if (item.IsDirty()) return true;
            }
            return false;
        }

        public void Paint(Graphics graphics)
        {
            foreach(UI_Item item in Items)
            {
                item.Paint(graphics);
            }
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
                cache.Dispose();
                disposed = true;
            }
        }

        ~UI()
        {
            Dispose(false);
        }
    }

    public class ControllerData
    {
        public SteamController ActiveController;

        public bool GetBasicControl(string inputName)
        {
            if (ActiveController == null) return false;

            inputName = inputName.ToLowerInvariant();

            SteamController.SteamControllerState state = ActiveController.GetState();
            if (state.Buttons == null) return false;

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
                foreach(var key in cache.Keys)
                {
                    cache[key].Dispose();
                    cache[key] = null;
                }
            }

            cache = null;
            disposed = true;
        }

        public Image GetImage(string Key, Func<Image> ImageLoader)
        {
            lock (cache)
            {
                if (cache.ContainsKey(Key)) return cache[Key];
                cache[Key] = ImageLoader();
                return cache[Key];
            }
        }

        public Image LoadImage(string name)
        {
            // load the image for the active theme
            string ImagePath = Path.Combine(themePath, name);

            // this will throw an exception if the file or path doesn't exist
            return Image.FromFile(ImagePath);
        }

        public static Image SetImageOpacity(Image image, float opacity)
        {
            try
            {
                //create a Bitmap the size of the image provided
                Bitmap bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppPArgb);

                //create a graphics object from the image  
                using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    // tweaked rendering settings for better performance rendering sprites
                    gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
                    gfx.SmoothingMode = SmoothingMode.None;
                    gfx.PixelOffsetMode = PixelOffsetMode.Half;
                    gfx.CompositingMode = CompositingMode.SourceOver;
                    gfx.CompositingQuality = CompositingQuality.HighSpeed;
                    gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;

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

        public virtual void Update()
        {
            foreach (UI_Item item in Items)
            {
                item.Update();
            }
        }

        public virtual bool IsDirty()
        {
            foreach (UI_Item item in Items)
            {
                if (item.IsDirty())
                    return true;
            }

            return false;
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

        public override void Update()
        {
            base.Update();
        }

        public override bool IsDirty()
        {
            return base.IsDirty();
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;
            if (DrawFromCenter)
                graphics.TranslateTransform(-Width / 2, -Height / 2);

            if (DisplayImage != null)
            {
                graphics.DrawImage(DisplayImage, X, Y, Width, Height);
            }

            graphics.Transform = preserve;

            base.Paint(graphics);
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

        private bool val = false;
        private bool? lastVal = null;

        private List<Tuple<bool, string>> calcFunc;

        protected override void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            base.Initalize(data, cache, themePath, themeData);
            this.data = data;

            output = true;

            InputName = themeData["inputName"]?.Value<string>();
            output = !(themeData["invert"]?.Value<bool>() ?? false);

            // super simplistic parsing for now, only supports single prefix ! and &&
            string Calc = themeData["calc"]?.Value<string>();
            if (!string.IsNullOrWhiteSpace(Calc))
            {
                calcFunc = Calc
                    .Split(new string[] { "&&" }, StringSplitOptions.None)
                    .Select(raw =>
                    {
                        string trimed = raw.Trim();
                        bool invert = trimed.StartsWith("!");
                        trimed = trimed.TrimStart('!');
                        return new Tuple<bool, string>(invert, trimed);
                    })
                    .ToList();
            }
        }

        private bool Evaluate()
        {
            if (calcFunc != null)
            {
                bool chk = true;
                foreach (Tuple<bool, string> itm in calcFunc)
                {
                    if (itm.Item1)
                    {
                        chk &= !data.GetBasicControl(itm.Item2);
                    }
                    else
                    {
                        chk &= data.GetBasicControl(itm.Item2);
                    }
                }
                if (chk) return output;
            }
            else if (string.IsNullOrWhiteSpace(InputName))
            {
                return output;
            }
            else if (data.GetBasicControl(InputName))
            {
                return output;
            }
            return !output;
        }

        public override void Update()
        {
            val = Evaluate();

            base.Update();
        }

        public override bool IsDirty()
        {
            if (lastVal != val)
                return true;
            return base.IsDirty();
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;

            if (val)
                base.Paint(graphics);

            lastVal = val;
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

        float? LastAnalogX = null;
        float? LastAnalogY = null;
        float AnalogX = 0;
        float AnalogY = 0;

        protected override void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            base.Initalize(data, cache, themePath, themeData);
            this.data = data;

            AxisNameX = themeData["axisNameX"]?.Value<string>();
            AxisNameY = themeData["axisNameY"]?.Value<string>();

            ScaleFactorX = themeData["scaleFactorX"]?.Value<float>() ?? 0;
            ScaleFactorY = themeData["scaleFactorY"]?.Value<float>() ?? 0;
        }

        public override void Update()
        {
            AnalogX = string.IsNullOrWhiteSpace(AxisNameX) ? 0 : (data.GetAnalogControl(AxisNameX) * ScaleFactorX);
            AnalogY = string.IsNullOrWhiteSpace(AxisNameY) ? 0 : (data.GetAnalogControl(AxisNameY) * ScaleFactorY);

            base.Update();
        }

        public override bool IsDirty()
        {
            if (LastAnalogX != AnalogX) return true;
            if (LastAnalogY != AnalogY) return true;

            return base.IsDirty();
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;

            graphics.TranslateTransform(AnalogX, -AnalogY);

            base.Paint(graphics);

            LastAnalogX = AnalogX;
            LastAnalogY = AnalogY;

            graphics.Transform = preserve;
        }
    }

    public class UL_TrailPad : UL_Slider
    {
        //protected float Width;
        //protected float Height;

        public UL_TrailPad(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData) : base(data, cache, themePath, themeData)
        {
        }

        private ControllerData data;
        private Image[] ImagePadDecay;
        private List<PointF?> PadPosHistory;
        private string InputName;

        private bool lastFrameHadItems = false;

        protected override void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            base.Initalize(data, cache, themePath, themeData);
            this.data = data;

            PadPosHistory = new List<PointF?>();

            InputName = themeData["inputName"]?.Value<string>();
            string imageName = themeData["image"]?.Value<string>();
            int TrailLength = themeData["length"]?.Value<int>()??0;

            float Width = themeData["width"]?.Value<float>() ?? 0;
            float Height = themeData["height"]?.Value<float>() ?? 0;

            if (!string.IsNullOrWhiteSpace(imageName) && TrailLength > 0)
            {
                Image ImagePadDecayBase = cache.LoadImage(imageName);
                string SizeSuffix = string.Empty;
                if (Width > 0 && Height > 0)
                    ImagePadDecayBase = new Bitmap(ImagePadDecayBase, (int)Width, (int)Height);
                int scaledTrailLength = MainForm.fpsLimit == 60 ? TrailLength : TrailLength / 2;
                ImagePadDecay = new Image[scaledTrailLength];
                for (int x = 0; x < ImagePadDecay.Length; x++)
                {
                    float percent = ((x + 1) * 1.0f / ImagePadDecay.Length);

                    ImagePadDecay[x] = cache.GetImage($"{imageName}:{Width}:{Height}:{percent}", () => { return UI_ImageCache.SetImageOpacity(ImagePadDecayBase, percent * 0.15f); });
                }
            }
            else
            {
                ImagePadDecay = new Image[0];
            }
        }

        public override void Update()
        {
            base.Update();
        }

        public override bool IsDirty()
        {
            if (lastFrameHadItems) return true;
            lock (PadPosHistory)
            {
                foreach (var itm in PadPosHistory)
                {
                    if (itm != null) return true;
                }
            }
            return base.IsDirty();
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;
            graphics.TranslateTransform(X, Y);

            lock (PadPosHistory)
            {
                //bool ControlHot = data.GetBasicControl(InputName);

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

                            double distance = SensorFusion.EMACalc.ApproxSqrt((float)(xVector * xVector + yVector * yVector));
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
                    PointF cord = new PointF(AnalogX, -AnalogY);
                    PadPosHistory.Add(cord);
                }
                else
                {
                    PadPosHistory.Add(null);
                }
                lastFrameHadItems = PadPosHistory.Any(dr => dr != null);
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

        private float Value = 0;
        private float? LastVal = null;

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

        public override void Update()
        {
            float Analog = string.IsNullOrWhiteSpace(AxisName) ? 0 : (data.GetAnalogControl(AxisName));
            Value = Math.Max(Math.Min((Analog - Min) / (Max - Min), 1.0f), 0.0f);

            base.Update();
        }

        public override bool IsDirty()
        {
            if (LastVal != Value)
                return true;
            return base.IsDirty();
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;

            graphics.TranslateTransform(X, Y);
            graphics.TranslateTransform(-Width / 2, -Height / 2);

            switch (Direction)
            {
                case "up":
                    graphics.FillRectangle(new SolidBrush(Background), 0, Height - (Height * Value), Width, Height * Value);
                    break;
                case "down":
                    graphics.FillRectangle(new SolidBrush(Background), 0, 0, Width, Height * Value);
                    break;
                case "left":
                    graphics.FillRectangle(new SolidBrush(Background), Width - (Width * Value), 0, Width * Value, Height);
                    break;
                default:
                    graphics.FillRectangle(new SolidBrush(Background), 0, 0, Width * Value, Height);
                    break;
            }

            graphics.DrawRectangle(new Pen(Foreground, 2), 0, 0, Width, Height);

            LastVal = Value;

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

        float TransformX = 0;
        float TransformY = 0;
        float RotationAngle = 0;
        float TiltFactorX = 0;
        float TiltFactorY = 0;

        float? LastTransformX = null;
        float? LastTransformY = null;
        float? LastRotationAngle = null;
        float? LastTiltFactorX = null;
        float? LastTiltFactorY = null;

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

        public override void Update()
        {
            var sensorData = MainForm.sensorData.Data; // use the cache!

            if (sensorData != null)
            {
                switch (DisplayType)
                {
                    case "accel":
                        {
                            TransformX = 1.0f - Math.Abs(sensorData.GyroTiltFactorY * 0.5f);
                            TransformY = 1.0f - Math.Abs(sensorData.GyroTiltFactorX * 0.5f);
                            RotationAngle = sensorData.GyroTiltFactorZ;
                            TiltFactorX = sensorData.GyroTiltFactorX;
                            TiltFactorY = sensorData.GyroTiltFactorY;
                        }
                        break;
                    case "gyro":
                        {
                            int SignY = -Math.Sign((2 * SensorCollector.Mod(Math.Floor((sensorData.Roll - 1) * 0.5f) + 1, 2)) - 1);
                            TransformX = SignY * Math.Max(1.0f - Math.Abs(sensorData.QuatTiltFactorY), 0.15f);
                            TransformY = Math.Max(1.0f - Math.Abs(sensorData.QuatTiltFactorX), 0.15f);

#if DEBUG
                            //Debug.WriteLine($"{TiltFactorY}\t{Roll}\t{(2 * mod(Math.Floor((Roll - 1) * 0.5f) + 1, 2)) - 1}");
                            //Debug.WriteLine($"qW={qw},{qx},{qy},{qz}");
                            //Debug.WriteLine($"gX={_gx},{_gy},{_gz}\taX={_ax},{_ay},{_az}");
                            //Debug.WriteLine($"{sensorData.Yaw},{sensorData.Pitch},{sensorData.Roll}\r\n");
#endif

                            RotationAngle = sensorData.QuatTiltFactorZ;
                            TiltFactorX = sensorData.QuatTiltFactorX;
                            TiltFactorY = sensorData.QuatTiltFactorY;
                        }
                        break;
                    default:
                        break;
                }
            }

            base.Update();
        }

        public override bool IsDirty()
        {
            if ((LastTransformX != TransformX)
             || (LastTransformY != TransformY)
             || (LastRotationAngle != RotationAngle)
             || (LastTiltFactorX != TiltFactorX)
             || (LastTiltFactorY != TiltFactorY))
                return true;
            return base.IsDirty();
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;

            graphics.TranslateTransform(X, Y);

            var sensorData = MainForm.sensorData.Data; // use the cache!

            if (sensorData != null)
            {
                switch (DisplayType)
                {
                    case "accel":
                    case "gyro":
                        {
                            Draw2dAs3d(
                                cache, graphics, DisplayImage, ShadowLName, ShadowL, ShadowRName, ShadowR, ShadowUName, ShadowU, ShadowDName, ShadowD,
                                TransformX, TransformY, RotationAngle, TiltFactorX, TiltFactorY,
                                Width, Height, TiltTranslateX, TiltTranslateY
                            );

                            graphics.ResetTransform();
                        }
                        break;
                    default:
                        break;
                }
            }

            LastTransformX = TransformX;
            LastTransformY = TransformY;
            LastRotationAngle = RotationAngle;
            LastTiltFactorX = TiltFactorX;
            LastTiltFactorY = TiltFactorY;

            graphics.Transform = preserve;
        }

        private void Draw2dAs3d(
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
