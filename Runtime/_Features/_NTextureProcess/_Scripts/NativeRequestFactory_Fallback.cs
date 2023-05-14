using UnityEngine;

namespace Nextension.TextureProcess
{
    internal partial class NativeRequestFactory
    {
        internal class Fallback : INativeRequest
        {
            public TextureProcessOperation decodeRGBA(byte[] imageData, TextureSetting setting)
            {
                var operation = new TextureProcessOperation.Fallback(imageData, setting).Operation;
                return operation;
            }

            public TextureProcessOperation decodeRGBA(string url, TextureSetting setting)
            {
                var operation = new TextureProcessOperation.Fallback(url, setting).Operation;
                return operation;
            }

            public TextureProcessOperation loadTextureAtNative(string url, TextureSetting setting)
            {
                Debug.LogWarning($"Not support on {Application.platform}, fallbacked to decodeRGBA()");
                return decodeRGBA(url, setting);
            }
        }
    }
}
