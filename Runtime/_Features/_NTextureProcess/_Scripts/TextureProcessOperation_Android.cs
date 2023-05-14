#if UNITY_ANDROID || UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Nextension.TextureProcess
{
    public partial class TextureProcessOperation
    {
        internal class Android : AndroidJavaProxy
        {
            internal Android(TextureSetting textureSetting) : base("next.nextension.ntextureprocess.ITextureProcessCallback")
            {
                Operation = new TextureProcessOperation();
                _setting = textureSetting;
            }

            private TextureSetting _setting;

            public TextureProcessOperation Operation { get; protected set; }
            private void onComplete(AndroidJavaObject rawTextureData)
            {
                try
                {
                    string errorMsg = rawTextureData.Get<string>("error");
                    if (string.IsNullOrEmpty(errorMsg))
                    {
                        sbyte[] rawDataBuffer = rawTextureData.Get<sbyte[]>("rawTextureData");
                        int outWidth = rawTextureData.Get<int>("outWidth");
                        int outHeight = rawTextureData.Get<int>("outHeight");
                        int inWidth = rawTextureData.Get<int>("originWidth");
                        int inHeight = rawTextureData.Get<int>("originHeight");
                        Operation.OriginWidth = inWidth;
                        Operation.OriginHeight = inHeight;

                        NRunOnMainThread.run(() =>
                        {
                            try
                            {
                                loadTexture(rawDataBuffer, outWidth, outHeight);
                                rawTextureData.Dispose();
                                Operation.innerSetComplete();
                            }
                            catch (Exception e)
                            {
                                rawTextureData.Dispose();
                                Operation.innerSetComplete(e.Message);
                            }
                        });

                    }
                    else
                    {
                        rawTextureData.Dispose();
                        Operation.innerSetComplete(errorMsg);
                    }
                }
                catch (Exception e)
                {
                    rawTextureData.Dispose();
                    Operation.innerSetComplete(e.Message);
                }
            }

            internal void setError(string errorMsg)
            {
                Operation.innerSetComplete(errorMsg);
            }

            private void loadTexture(sbyte[] rawDataBuffer, int w, int h)
            {
                if (_setting == null)
                {
                    _setting = new TextureSetting();
                }

                var tex = _setting.createTexture(w, h);
                var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(rawDataBuffer, 0);

                tex.LoadRawTextureData(ptr, rawDataBuffer.Length);
                tex.Apply(false, !_setting.isReadable);

                Operation.Texture = tex;
            }
        }
    }
}
#endif