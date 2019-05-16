using System.Collections.Generic;

namespace VSCView
{
    internal class ThemeDesc
    {
        public string name { get; set; }
        public List<ThemeDescAuthors> authors { get; set; }
    }
    internal class ThemeDescAuthors
    {
        public string name { get; set; }
        public string url { get; set; }
    }
    internal class ThemeSubDesc
    {
        public string name { get; set; }
    }
}