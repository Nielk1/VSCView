using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSCView
{
    // Declare a class with some static functions
    public static class CustomFleeFunctions
    {
        // Declare a function that takes a variable number of arguments
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
    }
}
