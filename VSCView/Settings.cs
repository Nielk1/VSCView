using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSCView
{
    public class Settings
    {
        public string Theme { get; set; }
        public string Background { get; set; }
        public int PreviousPid { get; set; }
        public bool CustomSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public bool AutoSelectOnlyController { get; set; }



        public bool HackScMotionOn { get; set; }
    }
}
