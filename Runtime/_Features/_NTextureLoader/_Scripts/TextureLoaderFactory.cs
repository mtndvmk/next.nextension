using System;

namespace Nextension.TextureLoader
{
    internal static class TextureLoaderFactory
    {
        public static ITexture2DLoader getTexture2DLoader(bool isForceUseFallback = false)
        {
            if (!isForceUseFallback)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return new Texture2DLoader<AndroidProcessor>();
#elif UNITY_WEBGL && !UNITY_EDITOR
                return new Texture2DLoader<WebGLProcessor>();
#elif (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && NET_4_6
                return new Texture2DLoader<WinProcessor>();
#endif
            }
            return new Texture2DLoader<FallbackProcessor>();
        }
    }
    internal class Texture2DLoader<T> : ITexture2DLoader where T : AbsProcessor, new()
    {
        public Texture2DLoaderOperation startLoad(byte[] inData, TextureSetting setting)
        {
            var processor = new T();
            processor.initialize(setting);
            try
            {
                processor.process(inData);
            }
            catch (Exception e)
            {
                processor.Operation.setError(e);
            }
            return processor.Operation;
        }

        public Texture2DLoaderOperation startLoad(Uri uri, TextureSetting setting)
        {
            var processor = new T();
            processor.initialize(setting);
            try
            {
                processor.process(uri);
            }
            catch (Exception e)
            {
                processor.Operation.setError(e);
            }
            return processor.Operation;
        }
    }
}
