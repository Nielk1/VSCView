using Flee.PublicTypes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public void InitalizeController()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].InitalizeController();
            }
        }

        public void Paint(Graphics graphics)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].CalculateValues();
                Items[i].Paint(graphics);
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
                cache.Dispose();
            disposed = true;
        }

        ~UI()
        {
            Dispose(false);
        }
    }

    public class ControllerData
    {
        public IController ActiveController;

        internal Type GetControlType(string inputName)
        {
            if (ActiveController == null) return default;

            ControllerState state = ActiveController.GetState();

            string[] parts = inputName.Split(new char[] { ':' }, 2);
            string subkey = (parts.Length > 1) ? parts[1] : string.Empty;
            IControl ctrl = state.Controls[parts[0]];
            if (ctrl == null) return typeof(bool);
            return ctrl.Type(subkey);
        }
        public T GetControlValue<T>(string inputName)
        {
            if (ActiveController == null) return default;

            ControllerState state = ActiveController.GetState();

            string[] parts = inputName.Split(new char[] { ':' }, 2);
            string subkey = (parts.Length > 1) ? parts[1] : string.Empty;
            IControl ctrl = state.Controls[parts[0]];
            if (ctrl == null) return default;
            return ctrl.Value<T>(subkey);
        }

        public ControllerState GetState()
        {
            if (ActiveController == null) return null;
            return ActiveController.GetState();
        }

        public void SetController(IController ActiveController)
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
                cache.Clear();
            disposed = true;
        }

        public Image GetImage(string Key, Func<Image> ImageLoader)
        {
            lock (cache)
            {
                Image result;
                if (cache.TryGetValue(Key, out result)) return result;
                cache[Key] = ImageLoader();
                return cache[Key];
            }
        }

        public Image LoadImage(string name)
        {
            // load the image for the active theme
            string ImagePath = name.StartsWith("\\") ? Path.Combine("themes", name.Substring(1)) : Path.Combine(themePath, name);

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

        public SmoothingMode? SmoothingMode { get; private set; }
        public InterpolationMode? InterpolationMode { get; private set; }

        private UI_ImageCache cache;
        private ControllerData data;
        private List<UI_Item> Items;

        public UI_Item(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            Initalize(data, cache, themePath, themeData);
        }

        //protected ExpressionContext BooleanContext;
        protected ExpressionContext NumericContext;

        protected virtual void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            this.cache = cache;
            this.data = data;

            /*{
                BooleanContext = new ExpressionContext();
                BooleanContext.Options.ParseCulture = CultureInfo.InvariantCulture;
                VariableCollection variables = BooleanContext.Variables;

                // Hook up the required events
                variables.ResolveVariableType += new EventHandler<ResolveVariableTypeEventArgs>(variables_ResolveVariableType);
                variables.ResolveVariableValue += new EventHandler<ResolveVariableValueEventArgs>(variables_ResolveVariableValue);
            }*/

            {
                NumericContext = new ExpressionContext();
                NumericContext.Options.ParseCulture = CultureInfo.InvariantCulture;
                VariableCollection variables = NumericContext.Variables;

                NumericContext.Imports.AddType(typeof(CustomFleeFunctions));

                // Hook up the required events
                variables.ResolveVariableType += new EventHandler<ResolveVariableTypeEventArgs>(variables_ResolveVariableTypeNumeric);
                variables.ResolveVariableValue += new EventHandler<ResolveVariableValueEventArgs>(variables_ResolveVariableValue);
            }

            Items = new List<UI_Item>();

            X = themeData["x"]?.Value<float>() ?? 0;
            Y = themeData["y"]?.Value<float>() ?? 0;
            Rot = themeData["rot"]?.Value<float>() ?? 0;

            try
            {
                SmoothingMode = (SmoothingMode)Enum.Parse(typeof(SmoothingMode), themeData["winform.smoothing"].Value<string>());
            }
            catch { }
            try
            {
                InterpolationMode = (InterpolationMode)Enum.Parse(typeof(InterpolationMode), themeData["winform.interpolation"].Value<string>());
            }
            catch { }

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

        protected void variables_ResolveVariableValue(object sender, ResolveVariableValueEventArgs e)
        {
            MethodInfo method = data.GetType().GetMethod("GetControlValue").MakeGenericMethod(new Type[] { e.VariableType });
            object retVal = method.Invoke(data, new object[] { e.VariableName.Replace("__colon__", ":") });
            e.VariableValue = Convert.ChangeType(retVal, e.VariableType);
        }

        /*protected void variables_ResolveVariableType(object sender, ResolveVariableTypeEventArgs e)
        {
            e.VariableType = data.GetControlType(e.VariableName.Replace("__colon__", ":"));
        }*/

        protected void variables_ResolveVariableTypeNumeric(object sender, ResolveVariableTypeEventArgs e)
        {
            e.VariableType = data.GetControlType(e.VariableName.Replace("__colon__", ":"));
            if (e.VariableType == typeof(bool))
                e.VariableType = typeof(int);
        }

        public virtual void InitalizeController()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].InitalizeController();
            }
        }

        public virtual void CalculateValues()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].CalculateValues();
            }
        }

        public virtual void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;

            graphics.TranslateTransform(X, Y);
            graphics.RotateTransform(Rot);

            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Paint(graphics);
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
                graphics.SmoothingMode = SmoothingMode ?? System.Drawing.Drawing2D.SmoothingMode.Default;
                graphics.InterpolationMode = InterpolationMode ?? System.Drawing.Drawing2D.InterpolationMode.Default;

                graphics.DrawImage(DisplayImage, X, Y, Width, Height);

                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
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
        //private string InputName;

        //private bool output;

        //private List<Tuple<bool, string>> calcFunc;
        private string Calc;
        private IDynamicExpression calcFunc;

        //private ExpressionContext BooleanContext;

        protected bool Digital;

        protected override void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            base.Initalize(data, cache, themePath, themeData);
            this.data = data;

            //output = true;

            //InputName = themeData["inputName"]?.Value<string>();
            //output = !(themeData["invert"]?.Value<bool>() ?? false);

            // super simplistic parsing for now, only supports single prefix ! and &&
            /*string Calc = themeData["calc"]?.Value<string>();
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
            }*/

            Calc = themeData["input"]?.Value<string>();
            /*if (!string.IsNullOrWhiteSpace(Calc))
            {
                BooleanContext = new ExpressionContext();
                BooleanContext.Options.ParseCulture = CultureInfo.InvariantCulture;
                VariableCollection variables = BooleanContext.Variables;

                // Hook up the required events
                variables.ResolveVariableType += new EventHandler<ResolveVariableTypeEventArgs>(variables_ResolveVariableType);
                variables.ResolveVariableValue += new EventHandler<ResolveVariableValueEventArgs>(variables_ResolveVariableValue);
            }*/

            InitalizeController();
        }

        public override void InitalizeController()
        {
            if (!string.IsNullOrWhiteSpace(Calc))
            {
                try
                {
                    //calcFunc = BooleanContext.CompileDynamic(Calc.Replace(":", "__colon__"));
                    calcFunc = NumericContext.CompileDynamic(Calc.Replace(":", "__colon__"));
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to compile dynamic formula \"{Calc}\"\r\n{ex}");
                }
            }

            base.InitalizeController();
        }

        public override void CalculateValues()
        {
            Digital = false;
            if (calcFunc != null)
                Digital = (bool)Convert.ChangeType(calcFunc?.Evaluate(), typeof(bool));

            base.CalculateValues();
        }

        /*private void variables_ResolveVariableValue(object sender, ResolveVariableValueEventArgs e)
        {
            MethodInfo method = data.GetType().GetMethod("GetControlValue").MakeGenericMethod(new Type[] { e.VariableType });
            object retVal = method.Invoke(data, new object[] { e.VariableName.Replace("__colon__", ":") });
            e.VariableValue = Convert.ChangeType(retVal, e.VariableType);
        }

        private void variables_ResolveVariableType(object sender, ResolveVariableTypeEventArgs e)
        {
            e.VariableType = data.GetControlType(e.VariableName.Replace("__colon__", ":"));
        }*/

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;

            //if (calcFunc != null && (bool)Convert.ChangeType(calcFunc?.Evaluate(), typeof(bool)))
            if (Digital)
            {
                base.Paint(graphics);
            }
        }
    }

    public class UL_Slider : UI_Item
    {
        public UL_Slider(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData) : base(data, cache, themePath, themeData)
        {
        }

        private ControllerData data;
        //protected string AxisNameX;
        //protected string AxisNameY;
        //protected float ScaleFactorX;
        //protected float ScaleFactorY;

        private string CalcX;
        protected IDynamicExpression calcXFunc;
        private string CalcY;
        protected IDynamicExpression calcYFunc;
        private string CalcR;
        protected IDynamicExpression calcRFunc;

        //private ExpressionContext NumericContext;

        // cache anlog values
        protected float AnalogX = 0;
        protected float AnalogY = 0;
        protected float AnalogR = 0;

        protected override void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            base.Initalize(data, cache, themePath, themeData);
            this.data = data;

            //AxisNameX = themeData["axisNameX"]?.Value<string>();
            //AxisNameY = themeData["axisNameY"]?.Value<string>();

            //ScaleFactorX = themeData["scaleFactorX"]?.Value<float>() ?? 0;
            //ScaleFactorY = themeData["scaleFactorY"]?.Value<float>() ?? 0;

            CalcX = themeData["inputX"]?.Value<string>();
            CalcY = themeData["inputY"]?.Value<string>();
            CalcR = themeData["inputR"]?.Value<string>();

            /*if (!string.IsNullOrWhiteSpace(CalcX)
             || !string.IsNullOrWhiteSpace(CalcY)
             || !string.IsNullOrWhiteSpace(CalcR))
            {
                NumericContext = new ExpressionContext();
                NumericContext.Options.ParseCulture = CultureInfo.InvariantCulture;
                VariableCollection variables = NumericContext.Variables;

                // Hook up the required events
                variables.ResolveVariableType += new EventHandler<ResolveVariableTypeEventArgs>(variables_ResolveVariableTypeNumeric);
                variables.ResolveVariableValue += new EventHandler<ResolveVariableValueEventArgs>(variables_ResolveVariableValue);
            }*/

            InitalizeController();
        }

        public override void InitalizeController()
        {
            if (!string.IsNullOrWhiteSpace(CalcX))
            {
                try
                {
                    calcXFunc = NumericContext.CompileDynamic(CalcX.Replace(":", "__colon__"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to compile dynamic formula \"{CalcX}\"\r\n{ex}");
                }
            }

            if (!string.IsNullOrWhiteSpace(CalcY))
            {
                try
                {
                    calcYFunc = NumericContext.CompileDynamic(CalcY.Replace(":", "__colon__"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to compile dynamic formula \"{CalcY}\"\r\n{ex}");
                }
            }

            if (!string.IsNullOrWhiteSpace(CalcR))
            {
                try
                {
                    calcRFunc = NumericContext.CompileDynamic(CalcR.Replace(":", "__colon__"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to compile dynamic formula \"{CalcR}\"\r\n{ex}");
                }
            }

            base.InitalizeController();
        }

        public override void CalculateValues()
        {
            AnalogX = 0;
            if (calcXFunc != null)
                 AnalogX = (float)Convert.ChangeType(calcXFunc?.Evaluate(), typeof(float));

            AnalogY = 0;
            if (calcYFunc != null)
                AnalogY = (float)Convert.ChangeType(calcYFunc?.Evaluate(), typeof(float));

            AnalogR = 0;
            if (calcRFunc != null)
                AnalogR = (float)Convert.ChangeType(calcRFunc?.Evaluate(), typeof(float));

            base.CalculateValues();
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;

            graphics.TranslateTransform(AnalogX, AnalogY);
            graphics.RotateTransform(AnalogR);

            base.Paint(graphics);

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
        //private string InputName;
        private string Calc;
        private IDynamicExpression calcFunc;

        //private ExpressionContext BooleanContext;
        protected bool Digital;

        protected override void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            base.Initalize(data, cache, themePath, themeData);
            this.data = data;

            PadPosHistory = new List<PointF?>();

            Calc = themeData["input"]?.Value<string>();
            /*if (!string.IsNullOrWhiteSpace(Calc))
            {
                BooleanContext = new ExpressionContext();
                BooleanContext.Options.ParseCulture = CultureInfo.InvariantCulture;
                VariableCollection variables = BooleanContext.Variables;

                // Hook up the required events
                variables.ResolveVariableType += new EventHandler<ResolveVariableTypeEventArgs>(variables_ResolveVariableType);
                variables.ResolveVariableValue += new EventHandler<ResolveVariableValueEventArgs>(variables_ResolveVariableValue);
            }*/

            InitalizeController();

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
                //int scaledTrailLength = (int)(TrailLength * (MainForm.fpsLimit / 60.0f));
                ImagePadDecay = new Image[TrailLength];
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

        public override void InitalizeController()
        {
            if (!string.IsNullOrWhiteSpace(Calc))
            {
                try
                {
                    //calcFunc = BooleanContext.CompileDynamic(Calc.Replace(":", "__colon__"));
                    calcFunc = NumericContext.CompileDynamic(Calc.Replace(":", "__colon__"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to compile dynamic formula \"{Calc}\"\r\n{ex}");
                }
            }

            base.InitalizeController();
        }

        public override void CalculateValues()
        {
            Digital = false;
            if (calcFunc != null)
                Digital = (bool)Convert.ChangeType(calcFunc?.Evaluate(), typeof(bool));

            base.CalculateValues();
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;
            graphics.TranslateTransform(X, Y);

            graphics.SmoothingMode = SmoothingMode ?? System.Drawing.Drawing2D.SmoothingMode.Default;
            graphics.InterpolationMode = InterpolationMode ?? System.Drawing.Drawing2D.InterpolationMode.Default;

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

            //if (calcFunc != null && (bool)Convert.ChangeType(calcFunc?.Evaluate(), typeof(bool)))
            if (Digital)
            {
                PointF cord = new PointF(AnalogX,AnalogY);
                PadPosHistory.Add(cord);
            }
            else
            {
                PadPosHistory.Add(null);
            }

            graphics.Transform = preserve;

            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;

            base.Paint(graphics);
        }
    }

    public class UL_PBar : UI_Item
    {
        public UL_PBar(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData) : base(data, cache, themePath, themeData)
        {
        }

        private ControllerData data;
        //protected string AxisName;
        protected string Direction;
        protected float Min;
        protected float Max;
        protected float Width;
        protected float Height;
        protected Color Foreground;
        protected Color Background;

        private string Calc;
        private IDynamicExpression calcFunc;

        protected float Analog = 0;

        protected override void Initalize(ControllerData data, UI_ImageCache cache, string themePath, JObject themeData)
        {
            base.Initalize(data, cache, themePath, themeData);
            this.data = data;

            Background = Color.White;
            Foreground = Color.Black;

            //AxisName = themeData["axisName"]?.Value<string>();
            Direction = themeData["direction"]?.Value<string>();

            Min = themeData["min"]?.Value<float>() ?? 0;
            Max = themeData["max"]?.Value<float>() ?? 0;
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

            Calc = themeData["input"]?.Value<string>();

            InitalizeController();
        }

        public override void InitalizeController()
        {
            if (!string.IsNullOrWhiteSpace(Calc))
            {
                try
                {
                    calcFunc = NumericContext.CompileDynamic(Calc.Replace(":", "__colon__"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to compile dynamic formula \"{Calc}\"\r\n{ex}");
                }
            }

            base.InitalizeController();
        }

        public override void CalculateValues()
        {
            Analog = 0;
            if (calcFunc != null)
            {
                Analog = (float)Convert.ChangeType(calcFunc?.Evaluate(), typeof(float));
                Analog = Math.Max(Math.Min((Analog - Min) / (Max - Min), 1.0f), 0.0f);
            }

            base.CalculateValues();
        }

        public override void Paint(Graphics graphics)
        {
            Matrix preserve = graphics.Transform;

            //float Analog = string.IsNullOrWhiteSpace(AxisName) ? 0 : (data.GetAnalogControl(AxisName));
            //Analog = Math.Max(Math.Min((Analog - Min) / (Max - Min), 1.0f), 0.0f);

            graphics.TranslateTransform(X, Y);
            graphics.TranslateTransform(-Width / 2, -Height / 2);

            graphics.SmoothingMode = SmoothingMode ?? System.Drawing.Drawing2D.SmoothingMode.Default;
            graphics.InterpolationMode = InterpolationMode ?? System.Drawing.Drawing2D.InterpolationMode.Default;

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
                case "right":
                default:
                    graphics.FillRectangle(new SolidBrush(Background), 0, 0, Width * Analog, Height);
                    break;
            }

            graphics.DrawRectangle(new Pen(Foreground, 2), 0, 0, Width, Height);

            graphics.Transform = preserve;

            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;

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

            var sensorData = MainForm.sensorData.Data; // use the cache!

            graphics.SmoothingMode = SmoothingMode ?? System.Drawing.Drawing2D.SmoothingMode.Default;
            graphics.InterpolationMode = InterpolationMode ?? System.Drawing.Drawing2D.InterpolationMode.Default;

            if (sensorData != null)
            {
                switch (DisplayType)
                {
                    case "accel":
                        {
                            float transformX = 1.0f - Math.Abs(sensorData.GyroTiltFactorY * 0.5f);
                            float transformY = 1.0f - Math.Abs(sensorData.GyroTiltFactorX * 0.5f);

                            Draw2dAs3d(
                                cache, graphics, DisplayImage, ShadowLName, ShadowL, ShadowRName, ShadowR, ShadowUName, ShadowU, ShadowDName, ShadowD,
                                transformX, transformY, sensorData.GyroTiltFactorZ, sensorData.GyroTiltFactorX, sensorData.GyroTiltFactorY,
                                Width, Height, TiltTranslateX, TiltTranslateY
                            );

                            graphics.ResetTransform();
                        }
                        break;
                    case "gyro":
                        {
                            int SignY = -Math.Sign((2 * SensorCollector.Mod(Math.Floor((sensorData.Roll - 1) * 0.5f) + 1, 2)) - 1);
                            float transformX = SignY * Math.Max(1.0f - Math.Abs(sensorData.QuatTiltFactorY), 0.15f);
                            float transformY = Math.Max(1.0f - Math.Abs(sensorData.QuatTiltFactorX), 0.15f);

#if DEBUG
                            //Debug.WriteLine($"{TiltFactorY}\t{Roll}\t{(2 * mod(Math.Floor((Roll - 1) * 0.5f) + 1, 2)) - 1}");
                            //Debug.WriteLine($"qW={qw},{qx},{qy},{qz}");
                            //Debug.WriteLine($"gX={_gx},{_gy},{_gz}\taX={_ax},{_ay},{_az}");
                            //Debug.WriteLine($"{sensorData.Yaw},{sensorData.Pitch},{sensorData.Roll}\r\n");
#endif

                            Draw2dAs3d(
                                cache, graphics, DisplayImage, ShadowLName, ShadowL, ShadowRName, ShadowR, ShadowUName, ShadowU, ShadowDName, ShadowD,
                                transformX, transformY, sensorData.QuatTiltFactorZ, sensorData.QuatTiltFactorX, sensorData.QuatTiltFactorY,
                                Width, Height, TiltTranslateX, TiltTranslateY
                            );

                            graphics.ResetTransform();
                        }
                        break;
                    default:
                        break;
                }
            }

            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;

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
