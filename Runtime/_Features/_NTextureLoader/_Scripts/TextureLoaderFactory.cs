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
                return new Texture2DLoader<AndroidProcessTask>();
#elif UNITY_WEBGL && !UNITY_EDITOR
                return new Texture2DLoader<WebGLProcessTask>();
#elif (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && NET_4_6
                return new Texture2DLoader<WinProcessTask>();
#endif
            }
            return new Texture2DLoader<FallbackProcessTask>();
        }
    }
    internal class Texture2DLoader<T> : ITexture2DLoader where T : AbsProcessTask, new()
    {
        public AbsProcessTask createProcessTask(byte[] imageData, TextureSetting setting)
        {
            var processor = new T();
            processor.initialize(imageData, setting);
            return processor;
        }

        public AbsProcessTask createProcessTask(Uri uri, TextureSetting setting)
        {
            var processor = new T();
            processor.initialize(uri, setting);
            return processor;
        }

        public AbsProcessTask createProcessTask(string url, TextureSetting setting)
        {
            var processor = new T();
            processor.initialize(url, setting);
            return processor;
        }
    }
}