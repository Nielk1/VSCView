using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace ThemeFixer
{
    public class UI
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string name { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? width { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? height { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? version { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<UI_Item> children { get; set; }
        public void Update()
        {
            if ((version ?? 0) == 0)
            {
                children?.ForEach(dr => dr.Update(version ?? 0));
                version = 1;
            }

            if ((version ?? 0) == 1)
            {
                children?.ForEach(dr => dr.Update(1));
                version = 2;
            }
        }
    }

    public class UI_Item
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string @type { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string image { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float? x { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float? y { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float? rot { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ang { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float? width { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float? height { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? center { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string input { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string inputX { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string inputY { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string inputR { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string inputName { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string calc { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? invert { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string axisNameX { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string axisNameY { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float? scaleFactorX { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float? scaleFactorY { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string axisName { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string direction { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string min { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string max { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? length { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string foreground { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string background { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string mode { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string shadowl { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string shadowr { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string shadowu { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string shadowd { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, PropertyName = "winform.smoothing")]
        public string winform_smoothing { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, PropertyName = "winform.interpolation")]
        public string winform_interpolation { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<UI_Item> children { get; set; }

        public void Update(int version)
        {
            switch (version)
            {
                case 0:
                    {
                        if (scaleFactorX.HasValue) scaleFactorX *= Int16.MaxValue;
                        if (scaleFactorY.HasValue) scaleFactorY *= Int16.MaxValue;
                        if (min != null)
                        {
                            try
                            {
                                min = (float.Parse(min) / byte.MaxValue).ToString(CultureInfo.InvariantCulture);
                            }
                            catch { }
                        }
                        if (max != null)
                        {
                            try
                            {
                                max = (float.Parse(max) / byte.MaxValue).ToString(CultureInfo.InvariantCulture);
                            }
                            catch { }
                        }

                        // remove rotation from trailpads as we unrotate the raw data now.
                        // also note that 30 deg was wrong, the correct value is actually 15 deg.

                        if (rot.HasValue)
                        {
                            bool AnyChildIsTrailpad = type == "trailpad";
                            if (!AnyChildIsTrailpad)
                            {
                                Queue<UI_Item> TrailpadDive = new Queue<UI_Item>();
                                children?.ForEach(dr => TrailpadDive.Enqueue(dr));
                                while (TrailpadDive.Count > 0)
                                {
                                    UI_Item tmp = TrailpadDive.Dequeue();
                                    if (tmp.type == "trailpad")
                                    {
                                        AnyChildIsTrailpad = true;
                                        break;
                                    }
                                    children?.ForEach(dr => TrailpadDive.Enqueue(dr));
                                }
                            }

                            if (AnyChildIsTrailpad)
                            {
                                if (rot.HasValue && rot.Value == 15)
                                    rot = null;
                                if (rot.HasValue && rot.Value == -15)
                                    rot = null;
                            }
                        }

                        if (type == "pbar")
                            center = true; // old behavior is the new centered behavior

                        if (type == "showhide" || type == "trailpad")
                        {
                            if (!string.IsNullOrWhiteSpace(inputName))
                            {
                                if (string.IsNullOrWhiteSpace(calc))
                                {
                                    if (invert.HasValue && invert.Value)
                                    {
                                        calc = @"not tobool(" + inputName + ")";
                                    }
                                    else
                                    {
                                        calc = inputName;
                                    }
                                }
                                inputName = null;
                                invert = null;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(inputName))
                        {
                            inputName = FixDigitalInputName(inputName);
                        }

                        if (!string.IsNullOrWhiteSpace(calc))
                        {
                            input = string.Join(" AND ", calc
                                .Split(new string[] { "&&" }, StringSplitOptions.None)
                                .Select(raw =>
                                {
                                    string trimed = raw.Trim();
                                    bool invert = trimed.StartsWith("!");
                                    trimed = trimed.TrimStart('!');
                                    return (invert ? "NOT " : string.Empty) + FixDigitalInputName(trimed);
                                }));
                            calc = null;
                        }

                        if (!string.IsNullOrWhiteSpace(axisName))
                        {
                            axisName = FixAnalogInputName(axisName);
                        }
                        if (!string.IsNullOrWhiteSpace(axisNameX))
                        {
                            axisNameX = FixAnalogInputName(axisNameX);
                            inputX = axisNameX;
                            if (scaleFactorX.HasValue)
                                inputX += " * " + scaleFactorX.Value.ToString(CultureInfo.InvariantCulture);
                            axisNameX = null;
                            scaleFactorX = null;
                        }
                        if (!string.IsNullOrWhiteSpace(axisNameY))
                        {
                            axisNameY = FixAnalogInputName(axisNameY);
                            inputY = axisNameY;
                            if (scaleFactorY.HasValue)
                                inputY += " * " + scaleFactorY.Value.ToString(CultureInfo.InvariantCulture);
                            axisNameY = null;
                            scaleFactorY = null;
                        }

                        if (!string.IsNullOrWhiteSpace(axisName))
                        {
                            axisName = FixAnalogInputName(axisName);
                            input = axisName;
                            //if (scaleFactor.HasValue)
                            //    input += " * " + scaleFactor.Value;
                            axisName = null;
                            //scaleFactor = null;
                        }

                        children?.ForEach(dr => dr.Update(version));
                    }
                    break;
                case 1:
                    {
                        if (!string.IsNullOrWhiteSpace(calc))
                        {
                            calc = calc.Replace("quad_right", "cluster_right");
                            calc = calc.Replace("quad_left", "cluster_left");
                        }

                        children?.ForEach(dr => dr.Update(version));
                    }
                    break;
            }
        }

        private string FixDigitalInputName(string inputName)
        {
            switch (inputName.ToLowerInvariant())
            {
                case "y":             return "quad_right:n";
                case "b":             return "quad_right:e";
                case "a":             return "quad_right:s";
                case "x":             return "quad_right:w";
                case "leftbumper":
                case "lb":            return "bumpers:l";
                case "rightbumper":
                case "rb":            return "bumpers:r";
                case "leftgrip":
                case "lg":            return "grip:l";
                case "rightgrip":
                case "rg":            return "grip:r";
                case "select":        return "menu:l";
                case "start":         return "menu:r";
                case "lefttrigger":
                case "lt":            return "triggers:l:stage2";
                case "righttrigger":
                case "rt":            return "triggers:r:stage2";
                case "steam":         return "home";
                case "stickclick":
                case "sc":            return "stick_left:click";
                case "leftpadtouch":  return "touch_left:0:touch";
                case "leftpadclick":  return "touch_left:click";
                case "rightpadtouch": return "touch_right:0:touch";
                case "rightpadclick": return "touch_right:click";
                case "touch0":
                case "touchnw":       return "grid_center:0:0";
                case "touch1":
                case "touchne":       return "grid_center:1:0";
                case "touch2":
                case "touchsw":       return "grid_center:0:1";
                case "touch3":
                case "touchse":       return "grid_center:1:1";

            }
            return inputName;
        }

        private string FixAnalogInputName(string inputName)
        {
            switch (inputName.ToLowerInvariant())
            {
                case "lefttrigger":  return "triggers:l:analog";
                case "righttrigger": return "triggers:r:analog";
                case "leftstickx":   return "stick_left:x";
                case "leftsticky":   return "stick_left:y";
                case "leftpadx":     return "touch_left:0:x";
                case "leftpady":     return "touch_left:0:y";
                case "rightpadx":    return "touch_right:0:x";
                case "rightpady":    return "touch_right:0:y";
            }
            return inputName;
        }
    }
}
