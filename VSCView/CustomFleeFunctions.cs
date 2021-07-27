using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSCView
{
    public static class CustomFleeFunctions
    {
        /*public static int Max(params int[] args)
        {
            if (args.Length == 0)
                return 0;

            int max = args[0];

            foreach (int i in args)
            {
                max = Math.Max(max, i);
            }

            return max;
        }
        public static int Min(params int[] args)
        {
            if (args.Length == 0)
                return 0;

            int min = args[0];

            foreach (int i in args)
            {
                min = Math.Min(min, i);
            }

            return min;
        }*/
        public static float Max(params float[] args)
        {
            if (args.Length == 0)
                return 0;

            float max = args[0];

            foreach (float i in args)
            {
                max = Math.Max(max, i);
            }

            return max;
        }
        public static float Min(params float[] args)
        {
            if (args.Length == 0)
                return 0;

            float min = args[0];

            foreach (float i in args)
            {
                min = Math.Min(min, i);
            }

            return min;
        }

        public static bool ToBool(object v)
        {
            if (v != null)
                return (bool)Convert.ChangeType(v, typeof(bool));
            return default(bool);
        }

        public static object If(bool v, object a, object b)
        {
            if (v)
                return a;
            return b;
        }

        public static long UnixTimeMs()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /*public static T IIf<T>(bool condition, T t, T f) { if (condition) return t; return f; }
        public static int IIf(bool condition, int t, int f) { if (condition) return t; return f; }
        public static float IIf(bool condition, float t, float f) { if (condition) return t; return f; }

        public static int IsNull(object unknown, int def)
        {
            if (unknown != null)
                return (int)Convert.ChangeType(unknown, typeof(int));
            return def;
        }

        public static float IsNull(object unknown, float def)
        {
            if (unknown != null)
                return (float)Convert.ChangeType(unknown, typeof(float));
            return def;
        }

        public static bool IsNull(object unknown, bool def)
        {
            if (unknown != null)
                return (bool)Convert.ChangeType(unknown, typeof(bool));
            return def;
        }*/
    }
}
