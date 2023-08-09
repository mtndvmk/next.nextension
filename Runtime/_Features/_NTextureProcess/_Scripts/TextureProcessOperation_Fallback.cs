using System;
using UnityEngine;

namespace Nextension.TextureProcess
{
    public partial class TextureProcessOperation
    {
        internal class Fallback : AbsProcessOperation
        {
            internal Fallback(string url, TextureSetting setting) : base(setting)
            {
                loadBinary(url);
            }
            internal Fallback(byte[] imgData, TextureSetting setting) : base(setting)
            {
                loadTexture(imgData);
            }
            private async void loadBinary(string url)
            {
                try
                {
                    var bin = await NTextureProcessUtils.getBinary(url);
                    loadTexture(bin);
                }
                catch (Exception e)
                {
                    Operation.innerFinalize(e);
                }
            }
            private async void loadTexture(byte[] imgData)
            {
                try
                {
                    Texture2D tex;

                    if (NTextureProcess._ProcessFallback != null)
                    {
                        tex = await NTextureProcess._ProcessFallback.Invoke(imgData, _setting);
                    }
                    else
                    {
                        var tex1 = _setting.createTexture(4, 4);
                        if (!tex1.LoadImage(imgData, _setting.isReadable))
                        {
                            throw new Exception("ImgData can't be loaded");
                        }

                        float inSample = 0;
                        int maxDimension;
                        if (_setting.maxDimension.x > 0 && _setting.maxDimension.y > 0)
                        {
                            maxDimension = Mathf.Min(_setting.maxDimension.x, _setting.maxDimension.y);
                            inSample = Mathf.Min(tex1.width, tex1.height) * 1f / maxDimension;
                        }
                        else
                        {
                            inSample = _setting.inSampleSize;
                            maxDimension = Mathf.Min(tex1.width, tex1.height) / _setting.inSampleSize;

                        }

                        if (inSample > 1)
                        {
                            var rawColor = tex1.GetRawTextureData<byte>();
                            var (colors, outW, outH) = await NTextureProcessUtils.resize(rawColor, tex1.width, tex1.height, maxDimension);

                            if (!Application.isPlaying)
                            {
                                rawColor.Dispose();
                                colors.Dispose();
                                NUtils.destroy(tex1);
                                return;
                            }

                            var tex2 = _setting.createTexture(outW, outH, tex1.format);
                            tex2.LoadRawTextureData(colors);
                            tex2.Apply();
                            tex = tex2;

                            rawColor.Dispose();
                            colors.Dispose();
                            NUtils.destroy(tex1);
                        }
                        else
                        {
                            tex = tex1;
                        }
                    }

                    _setting.compress(tex);

                    Operation.Texture = tex;
                    Operation.OriginHeight = tex.height;
                    Operation.OriginWidth = tex.width;
                    Operation.innerFinalize();
                }
                catch (Exception e)
                {
                    Operation.innerFinalize(e);
                }
            }
        }
    }
}
