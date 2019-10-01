using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
                children?.ForEach(dr => dr.Update());
                version = 1;
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
        public float? min { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float? max { get; set; }
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
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<UI_Item> children { get; set; }
        public void Update()
        {
            if (scaleFactorX.HasValue) scaleFactorX *= Int16.MaxValue;
            if (scaleFactorY.HasValue) scaleFactorY *= Int16.MaxValue;
            if (min.HasValue) min /= byte.MaxValue;
            if (max.HasValue) max /= byte.MaxValue;

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

            if (type == "showhide" || type == "trailpad")
            {
                if (!string.IsNullOrWhiteSpace(inputName))
                {
                    if (string.IsNullOrWhiteSpace(calc))
                    {
                        if (invert.HasValue && invert.Value)
                        {
                            calc = @"!" + inputName;
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
                    inputX += " * " + scaleFactorX.Value;
                axisNameX = null;
                scaleFactorX = null;
            }
            if (!string.IsNullOrWhiteSpace(axisNameY))
            {
                axisNameY = FixAnalogInputName(axisNameY);
                inputY = axisNameY;
                if (scaleFactorY.HasValue)
                    inputY += " * " + scaleFactorY.Value;
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

            children?.ForEach(dr => dr.Update());
        }

        private string FixDigitalInputName(string inputName)
        {
            switch (inputName.ToLowerInvariant())
            {
                case "y":             return "quad_right:0";
                case "b":             return "quad_right:1";
                case "a":             return "quad_right:2";
                case "x":             return "quad_right:3";
                case "leftbumper":
                case "lb":            return "bumpers:0";
                case "rightbumper":
                case "rb":            return "bumpers:1";
                case "leftgrip":
                case "lg":            return "grip:0";
                case "rightgrip":
                case "rg":            return "grip:1";
                case "select":        return "menu:0";
                case "start":         return "menu:1";
                case "lefttrigger":
                case "lt":            return "triggers:stage2_0";
                case "righttrigger":
                case "rt":            return "triggers:stage2_1";
                case "steam":         return "home";
                case "stickclick":
                case "sc":            return "stick_left:click";
                case "leftpadtouch":  return "touch_left:touch0";
                case "leftpadclick":  return "touch_left:click";
                case "rightpadtouch": return "touch_right:touch0";
                case "rightpadclick": return "touch_right:click";
                case "touch0":
                case "touchnw":       return "quad_center:0";
                case "touch1":
                case "touchne":       return "quad_center:1";
                case "touch2":
                case "touchsw":       return "quad_center:3";
                case "touch3":
                case "touchse":       return "quad_center:2";

            }
            return inputName;
        }

        private string FixAnalogInputName(string inputName)
        {
            switch (inputName.ToLowerInvariant())
            {
                case "lefttrigger":  return "triggers:analog0";
                case "righttrigger": return "triggers:analog1";
                case "leftstickx":   return "stick_left:x";
                case "leftsticky":   return "stick_left:y";
                case "leftpadx":     return "touch_left:x0";
                case "leftpady":     return "touch_left:y0";
                case "rightpadx":    return "touch_right:x0";
                case "rightpady":    return "touch_right:y0";
            }
            return inputName;
        }
    }
}
