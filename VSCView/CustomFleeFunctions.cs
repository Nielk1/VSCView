using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSCView
{
    public static class CustomFleeFunctions
    {
        public static int Max(params int[] args)
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
        }
    }
}
