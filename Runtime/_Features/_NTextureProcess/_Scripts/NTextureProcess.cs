using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Nextension.TextureProcess
{
    public static class NTextureProcess
    {
        internal static Func<byte[], TextureSetting, Task<Texture>> _ProcessFallback;
        private static INativeRequest _nativeRequest = NativeRequestFactory.get();

        public static void setImageProcessFallback(Func<byte[], TextureSetting, Task<Texture>> fallbackFunc)
        {
            _ProcessFallback = fallbackFunc;
        }
        public static void forceUseFallback(bool isForceUseFallback)
        {
            _nativeRequest = NativeRequestFactory.get(isForceUseFallback);
        }

        public static TextureProcessOperation decodeRGBA(byte[] imageData, int maxWidth, int maxHeight)
        {
            TextureSetting setting = new TextureSetting();
            setting.maxDimension = new Vector2Int(maxWidth, maxHeight);
            return decodeRGBA(imageData, setting);
        }
        public static TextureProcessOperation decodeRGBA(byte[] imageData, int inSampleSize = 1)
        {
            TextureSetting setting = new TextureSetting();
            setting.inSampleSize = inSampleSize;
            return decodeRGBA(imageData, setting);
        }
        public static TextureProcessOperation decodeRGBA(byte[] imageData, TextureSetting setting)
        {
            return _nativeRequest.decodeRGBA(imageData, setting);
        }

        public static TextureProcessOperation decodeRGBA(string url, int maxWidth, int maxHeight)
        {
            TextureSetting setting = new TextureSetting();
            setting.maxDimension = new Vector2Int(maxWidth, maxHeight);
            return decodeRGBA(url, setting);
        }
        public static TextureProcessOperation decodeRGBA(string url, int inSampleSize = 1)
        {
            TextureSetting setting = new TextureSetting();
            setting.inSampleSize = inSampleSize;
            return decodeRGBA(url, setting);
        }
        public static TextureProcessOperation decodeRGBA(string url, TextureSetting setting)
        {
            return _nativeRequest.decodeRGBA(url, setting);
        }

        public static TextureProcessOperation loadTextureAtNative(string url, TextureSetting setting)
        {
            return _nativeRequest.loadTextureAtNative(url, setting);
        }
    }
}
