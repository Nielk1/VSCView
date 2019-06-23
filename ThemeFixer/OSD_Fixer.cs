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
                children?.ForEach(dr => dr.Update());
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
        public int? width { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? height { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? center { get; set; }
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

            if (type == "showhide")
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
                calc = string.Join("&&", calc
                    .Split(new string[] { "&&" }, StringSplitOptions.None)
                    .Select(raw =>
                    {
                        string trimed = raw.Trim();
                        bool invert = trimed.StartsWith("!");
                        trimed = trimed.TrimStart('!');
                        return (invert ? "!" : string.Empty) + FixDigitalInputName(trimed);
                    }));
            }

            if (!string.IsNullOrWhiteSpace(axisName))
            {
                axisName = FixAnalogInputName(axisName);
            }
            if (!string.IsNullOrWhiteSpace(axisNameX))
            {
                axisNameX = FixAnalogInputName(axisNameX);
            }
            if (!string.IsNullOrWhiteSpace(axisNameY))
            {
                axisNameY = FixAnalogInputName(axisNameY);
            }

            children?.ForEach(dr => dr.Update());
        }

        private string FixDigitalInputName(string inputName)
        {
            switch (inputName.ToLowerInvariant())
            {
                case "y":            return "quad_right:0";
                case "b":            return "quad_right:1";
                case "a":            return "quad_right:2";
                case "x":            return "quad_right:3";
                case "leftbumper":
                case "lb":           return "bumpers:0";
                case "rightbumper":
                case "rb":           return "bumpers:1";
                case "leftgrip":
                case "lg":           return "grip:0";
                case "rightgrip":
                case "rg":           return "grip:1";
                case "select":       return "menu:0";
                case "start":        return "menu:1";
                case "lefttrigger":
                case "lt":           return "triggers:stage2_0";
                case "righttrigger":
                case "rt":           return "triggers:stage2_1";
                case "steam":        return "home";
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
            }
            return inputName;
        }
    }
}
