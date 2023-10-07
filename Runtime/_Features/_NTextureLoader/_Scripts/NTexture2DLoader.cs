using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Nextension.TextureLoader
{
    public static class NTexture2DLoader
    {
        internal static Func<byte[], TextureSetting, Task<Texture2D>> _loaderFallback;
        private static ITexture2DLoader _loader;
        private static NScheduler<AbsProcessTask> _scheduler;

        private static ITexture2DLoader Loader => _loader ??= TextureLoaderFactory.getTexture2DLoader();

        private static NScheduler<AbsProcessTask> Scheduler => _scheduler ??= new();
        public static int MaximumNumberOfSchedulerTasksAtOnce
        {
            get => Scheduler.MaxSchedulableAtOnce;
            set => Scheduler.MaxSchedulableAtOnce = value;
        }

        internal static AbsProcessTask createProcessTask(byte[] imageData, TextureSetting setting)
        {
            return Loader.createProcessTask(imageData, setting);
        }
        internal static AbsProcessTask createProcessTask(Uri uri, TextureSetting setting)
        {
            return Loader.createProcessTask(uri, setting);
        }
        internal static AbsProcessTask createProcessTask(string url, TextureSetting setting)
        {
            return Loader.createProcessTask(url, setting);
        }

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
            var task = createProcessTask(imageData, setting);
            task.startProcess();
            return task.getOperation();
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
            var task = createProcessTask(url, setting);
            task.startProcess();
            return task.getOperation();
        }


        public static Texture2DLoaderOperation schedule(byte[] imageData, TextureSetting setting, int priority = 0)
        {
            var task = createProcessTask(imageData, setting);
            task.priority = priority;
            Scheduler.schedule(task);
            return task.getOperation();
        }
        public static Texture2DLoaderOperation schedule(byte[] imageData, int maxWidth, int maxHeight, int priority = 0)
        {
            TextureSetting setting = new TextureSetting();
            setting.maxDimension = new Vector2Int(maxWidth, maxHeight);
            return schedule(imageData, setting, priority);
        }
        public static Texture2DLoaderOperation schedule(byte[] imageData, int inSampleSize = 1, int priority = 0)
        {
            TextureSetting setting = new TextureSetting();
            setting.inSampleSize = inSampleSize;
            return schedule(imageData, setting, priority);
        }
        public static Texture2DLoaderOperation schedule(string url, TextureSetting setting, int priority = 0)
        {
            var task = createProcessTask(url, setting);
            task.priority = priority;
            Scheduler.schedule(task);
            return task.getOperation();
        }
        public static Texture2DLoaderOperation schedule(string url, int maxWidth, int maxHeight, int priority = 0)
        {
            TextureSetting setting = new TextureSetting();
            setting.maxDimension = new Vector2Int(maxWidth, maxHeight);
            return schedule(url, setting, priority);
        }
        public static Texture2DLoaderOperation schedule(string url, int inSampleSize = 1, int priority = 0)
        {
            TextureSetting setting = new TextureSetting();
            setting.inSampleSize = inSampleSize;
            return schedule(url, setting, priority);
        }
    }
}
