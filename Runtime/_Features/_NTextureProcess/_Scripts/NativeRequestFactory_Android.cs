#if UNITY_ANDROID || UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Nextension.TextureProcess
{
    internal partial class NativeRequestFactory
    {
        internal class Android : INativeRequest
        {
            private AndroidJavaObject _AndroidJavaObject;
            internal Android()
            {
                _AndroidJavaObject = new AndroidJavaObject("next.nextension.ntextureprocess.NTextureProcess");
            }

            public TextureProcessOperation decodeRGBA(byte[] imageData, TextureSetting setting)
            {
                if (setting == null)
                {
                    setting = new TextureSetting();
                }
                TextureProcessOperation.Android operation = new TextureProcessOperation.Android(setting);
                processImage(imageData, setting, operation);
                return operation.Operation;
            }

            public TextureProcessOperation decodeRGBA(string url, TextureSetting setting)
            {
                if (setting == null)
                {
                    setting = new TextureSetting();
                }
                TextureProcessOperation.Android operation = new TextureProcessOperation.Android(setting);
                loadBinary(url, setting, operation);
                return operation.Operation;
            }

            private async void loadBinary(string url, TextureSetting setting, TextureProcessOperation.Android operation)
            {
                try
                {
                    var bin = await NTextureProcessUtils.getBinary(url);
                    processImage(bin, setting, operation);
                }
                catch (Exception e)
                {
                    operation.setError(e.Message);
                }
            }

            private void processImage(byte[] imageData, TextureSetting setting, TextureProcessOperation.Android operation)
            {
                sbyte[] signedBytes = (sbyte[])(Array)imageData;
                if (setting.maxDimension.x > 0 && setting.maxDimension.y > 0)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            AndroidJNI.AttachCurrentThread();
                            _AndroidJavaObject.CallStatic("decodeRGBA", signedBytes, setting.maxDimension.x, setting.maxDimension.y, operation);
                        }
                        catch (Exception e)
                        {
                            operation.setError(e.Message);
                        }
                    });
                }
                else
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            AndroidJNI.AttachCurrentThread();
                            _AndroidJavaObject.CallStatic("decodeRGBA", signedBytes, setting.inSampleSize, operation);
                        }
                        catch (Exception e)
                        {
                            operation.setError(e.Message);
                        }
                    });
                }
            }

            public void dispose()
            {
                if (_AndroidJavaObject != null)
                {
                    _AndroidJavaObject.Dispose();
                }
            }

            public TextureProcessOperation loadTextureAtNative(string url, TextureSetting setting)
            {
                Debug.LogWarning($"Not support on {Application.platform}, fallbacked to decodeRGBA()");
                return decodeRGBA(url, setting);
            }
        }
    }
}
#endif