#if UNITY_ANDROID || UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Nextension.TextureLoader
{
    internal class AndroidProcessor : AbsProcessor
    {
        private AndroidJavaObject _AndroidJavaObject;
        
        public AndroidProcessor() : base() 
        {
            _AndroidJavaObject = new AndroidJavaObject("next.nextension.androidnative.NTextureProcess");
        }
        public override Task process(byte[] inData)
        {
            var callback = new AndroidTextureProcessCallback(_setting, Operation);
            Task.Run(() =>
            {
                try
                {
                    AndroidJNI.AttachCurrentThread();
                    if (_setting.maxDimension.x > 0 && _setting.maxDimension.y > 0)
                    {
                        _AndroidJavaObject.CallStatic("process", NConverter.convert<sbyte>(inData), _setting.maxDimension.x, _setting.maxDimension.y, callback);
                    }
                    else
                    {
                        _AndroidJavaObject.CallStatic("process", NConverter.convert<sbyte>(inData), _setting.inSampleSize, callback);
                    }
                }
                catch (Exception e)
                {
                    Operation.setError(e);
                }
            });
            return Task.CompletedTask;
        }
        public override async Task process(Uri uri)
        {
            await process(await NUtils.getBinaryFrom(uri));
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
        internal AndroidTextureProcessCallback(TextureSetting textureSetting, Texture2DLoaderOperation operation) : base("next.nextension.androidnative.AndroidTextureProcessCallback")
        {
            _operation = operation;
            _setting = textureSetting;
        }

        private TextureSetting _setting;
        private Texture2DLoaderOperation _operation;

        /// <summary>
        /// Called from native
        /// </summary>
        private void onComplete(AndroidJavaObject result)
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

                    NRunOnMainThread.run(async () =>
                    {
                        try
                        {
                            var tex = await getTexture(rawDataBuffer, outWidth, outHeight);
                            _operation.setResult(tex, inWidth, inHeight);
                        }
                        catch (Exception e)
                        {
                            result.Dispose();
                            _operation.setError(e);
                        }
                    });
                }
                else
                {
                    result.Dispose();
                    _operation.setError(new TextureProcessException(errorMsg));
                }
            }
            catch (Exception e)
            {
                result.Dispose();
                _operation.setError(e);
            }
        }
        private async Task<Texture2D> getTexture(sbyte[] rawDataBuffer, int w, int h)
        {
            var tex = _setting.createTexture(w, h);
            if (_setting.forceNoAlpha)
            {
                var nativeBytes = new NativeArray<sbyte>(rawDataBuffer, Allocator.TempJob);
                var nativeBinary24 = await NTextureUtils.convertT32ToT24(nativeBytes);
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
            await _setting.apply(tex);
            return tex;
        }
    }
}
#endif