using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThemeFixer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            if (!File.Exists(args[0])) return;

            UI ui = JsonConvert.DeserializeObject<UI>(File.ReadAllText(args[0]));
            ui.Update();
            string raw = JsonConvert.SerializeObject(ui, Formatting.Indented);
            File.Move(args[0], args[0] + ".bak");
            File.WriteAllText(args[0], raw);
        }
    }
}
