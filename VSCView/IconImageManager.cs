using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VSCView
{
    public class IconImageManager : IDisposable
    {
        private Dictionary<string, Image> cache;
        public IconImageManager()
        {
            this.cache = new Dictionary<string, Image>();
        }

        private bool disposed = false;
        ~IconImageManager()
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
            {
                foreach(var item in cache)
                {
                    item.Value.Dispose();
                }
                cache.Clear();
            }
            disposed = true;
        }

        public Image GetImage(string Key)
        {
            try
            {
                lock (cache)
                {
                    Image result;
                    if (cache.TryGetValue(Key, out result)) return result;
                    string ImagePath = Path.Combine("icons", Key + ".png");
                    if (!File.Exists(ImagePath))
                        return null;
                    cache[Key] = Image.FromFile(ImagePath);
                    return cache[Key];
                }
            }
            catch
            {
                return null;
            }
        }
    }
}