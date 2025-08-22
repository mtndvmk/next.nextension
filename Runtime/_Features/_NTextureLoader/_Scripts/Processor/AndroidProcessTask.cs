#if UNITY_ANDROID || UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Nextension.TextureLoader
{
    internal class AndroidProcessTask : AbsProcessTask
    {
        private AndroidJavaObject _AndroidJavaObject;

        public AndroidProcessTask() : base()
        {
            _AndroidJavaObject = new AndroidJavaObject("next.nextension.androidnative.NTextureProcess");
        }
        protected override Task exeProcess(byte[] imageData)
        {
            var extension = NTextureUtils.getImageExtension(imageData);
            setImageExtension(extension);
            if (!(extension == ImageExtension.JPG || extension == ImageExtension.PNG))
            {
                setError(new Exception($"Image extension is not supported: {extension}"));
                return Task.CompletedTask;
            }

            var callback = new AndroidTextureProcessCallback(_setting, this);
            Task.Run(() =>
            {
                try
                {
                    AndroidJNI.AttachCurrentThread();
                    if (_setting.maxDimension.x > 0 && _setting.maxDimension.y > 0)
                    {
                        _AndroidJavaObject.CallStatic("process", NConverter.convert<sbyte>(imageData), _setting.maxDimension.x, _setting.maxDimension.y, callback);
                    }
                    else
                    {
                        _AndroidJavaObject.CallStatic("process", NConverter.convert<sbyte>(imageData), _setting.inSampleSize, callback);
                    }
                }
                catch (Exception e)
                {
                    setError(e);
                }
            });
            return Task.CompletedTask;
        }
        protected override async Task exeProcess(Uri uri)
        {
            await exeProcess(await NUtils.getBinaryFrom(uri));
        }
        public void dispose()
        {
            if (_AndroidJavaObject != null)
            {
                _AndroidJavaObject.Dispose();
            }
        }
    }
    internal class AndroidTextureProcessCallback : AndroidJavaProxy
    {
        internal AndroidTextureProcessCallback(TextureSetting textureSetting, AbsProcessTask processor) : base("next.nextension.androidnative.AndroidTextureProcessCallback")
        {
            _processor = processor;
            _setting = textureSetting;
        }

        private TextureSetting _setting;
        private AbsProcessTask _processor;

        /// <summary>
        /// Called from native
        /// </summary>
        private async void onComplete(AndroidJavaObject result)
        {
            try
            {
                string errorMsg = result.Get<string>("error");
                if (string.IsNullOrEmpty(errorMsg))
                {
                    sbyte[] rawDataBuffer = result.Get<sbyte[]>("rawTextureData");
                    int outWidth = result.Get<int>("outWidth");
                    int outHeight = result.Get<int>("outHeight");
                    int inWidth = result.Get<int>("originWidth");
                    int inHeight = result.Get<int>("originHeight");
                    result.Dispose();

                    var tex = await getTexture(rawDataBuffer, outWidth, outHeight);
                    _processor.setResult(tex, inWidth, inHeight);
                }
                else
                {
                    result.Dispose();
                    _processor.setError(new TextureProcessException(errorMsg));
                }
            }
            catch (Exception e)
            {
                result.Dispose();
                _processor.setError(e);
            }
        }
        private async Task<Texture2D> getTexture(sbyte[] rawDataBuffer, int w, int h)
        {
            await new NWaitMainThread();
            var tex = _setting.createTexture(w, h);
            if (_setting.forceNoAlpha)
            {
                var nativeBytes = new NativeArray<sbyte>(rawDataBuffer, Allocator.TempJob);
                var nativeBinary24 = await NTextureUtils.asyncConvertT32ToT24(nativeBytes);
                tex.LoadRawTextureData(nativeBinary24);
                nativeBytes.Dispose();
                nativeBinary24.Dispose();
            }
            else
            {
                GCHandle handle = GCHandle.Alloc(rawDataBuffer, GCHandleType.Pinned);
                tex.LoadRawTextureData(handle.AddrOfPinnedObject(), rawDataBuffer.Length);
                handle.Free();
            }
            _setting.apply(tex);
            return tex;
        }
    }
}
#endif