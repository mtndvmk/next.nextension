using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Nextension.TextureLoader
{
    public static class NTexture2DLoader
    {
        internal static Func<byte[], TextureSetting, Task<Texture2D>> _loaderFallback;
        private static ITexture2DLoader _loader = TextureLoaderFactory.getTexture2DLoader();

        public static void setLoaderFallback(Func<byte[], TextureSetting, Task<Texture2D>> fallbackFunc)
        {
            _loaderFallback = fallbackFunc;
        }
        public static void forceUseFallback(bool isForceUseFallback)
        {
            _loader = TextureLoaderFactory.getTexture2DLoader(isForceUseFallback);
        }
        public static Texture2DLoaderOperation startLoad(byte[] imageData, int maxWidth, int maxHeight)
        {
            TextureSetting setting = new TextureSetting();
            setting.maxDimension = new Vector2Int(maxWidth, maxHeight);
            return startLoad(imageData, setting);
        }
        public static Texture2DLoaderOperation startLoad(byte[] imageData, int inSampleSize = 1)
        {
            TextureSetting setting = new TextureSetting();
            setting.inSampleSize = inSampleSize;
            return startLoad(imageData, setting);
        }
        public static Texture2DLoaderOperation startLoad(byte[] imageData, TextureSetting setting)
        {
            return _loader.startLoad(imageData, setting);
        }
        public static Texture2DLoaderOperation startLoad(string url, int maxWidth, int maxHeight)
        {
            TextureSetting setting = new TextureSetting();
            setting.maxDimension = new Vector2Int(maxWidth, maxHeight);
            return startLoad(url, setting);
        }
        public static Texture2DLoaderOperation startLoad(string url, int inSampleSize = 1)
        {
            TextureSetting setting = new TextureSetting();
            setting.inSampleSize = inSampleSize;
            return startLoad(url, setting);
        }
        public static Texture2DLoaderOperation startLoad(string url, TextureSetting setting)
        {
            return _loader.startLoad(new Uri(url), setting);
        }
    }
}
