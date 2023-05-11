using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PrviProjekat
{
    internal class Cache
    {
        static object locker = new object();
        static Dictionary<string, byte[]> ImageCache;

        public Cache()
        {
            ImageCache = new Dictionary<string, byte[]>();
        }

        public void AddImageToCache(string imageName, byte[] buf)
        {
            lock(locker)
            {
                if (ImageCache.ContainsKey(imageName))
                    return;

                ImageCache.Add(imageName, buf);
            }
        }

        public bool GetImageFromCache(string imageName, out byte[] buf)
        {
            bool hit = false;
            lock(locker)
            {
                hit=ImageCache.TryGetValue(imageName, out buf);
                  
            }
            return hit;
        }
    }
}
