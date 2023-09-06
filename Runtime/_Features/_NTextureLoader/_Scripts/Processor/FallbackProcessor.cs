using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Nextension.TextureLoader
{
    internal class FallbackProcessor : AbsProcessor
    {
        public override async Task process(byte[] inData)
        {
            await loadTexture(inData);
        }
        public override async Task process(Uri uri)
        {
            await loadTexture(await NUtils.getBinaryFrom(uri));
        }
        private async Task loadTexture(byte[] inData)
        {
            try
            {
                Texture2D tex;

                if (NTexture2DLoader._loaderFallback != null)
                {
                    tex = await NTexture2DLoader._loaderFallback.Invoke(inData, _setting);
                }
                else
                {
                    if (_setting.forceNoAlpha)
                    {
                        Debug.LogWarning("Not support ForceNoAlpha in this platform");
                    }
                    var tex1 = _setting.createTexture(4, 4);
                    if (!tex1.LoadImage(inData, _setting.isReadable))
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
                        var (colors, outW, outH) = await NTextureUtils.resize(rawColor, tex1.width, tex1.height, maxDimension);

                        if (!Application.isPlaying)
                        {
                            rawColor.Dispose();
                            colors.Dispose();
                            NUtils.destroy(tex1);
                            return;
                        }

                        tex = _setting.createTexture(outW, outH, tex1.format);
                        tex.LoadRawTextureData(colors);

                        rawColor.Dispose();
                        colors.Dispose();
                        NUtils.destroy(tex1);
                    }
                    else
                    {
                        tex = tex1;
                    }
                }

                await _setting.apply(tex);
                Operation.setResult(tex, tex.width, tex.height);
            }
            catch (Exception e)
            {
                Operation.setError(e);
            }
        }
    }
}
