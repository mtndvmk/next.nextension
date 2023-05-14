namespace Nextension.TextureProcess
{
    internal interface INativeRequest
    {
        TextureProcessOperation decodeRGBA(byte[] imageData, TextureSetting setting);
        TextureProcessOperation decodeRGBA(string filePath, TextureSetting setting);
        TextureProcessOperation loadTextureAtNative(string filePath, TextureSetting setting);
    }
    internal partial class NativeRequestFactory
    {
        public static INativeRequest get(bool isForceUseFallback = false)
        {
            if (!isForceUseFallback)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return new Android();
#elif UNITY_WEBGL && !UNITY_EDITOR
                return new WebGL();
#elif (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && NET_4_6
                return new Win();
#endif
            }
            return new Fallback();
        }
    }
}
