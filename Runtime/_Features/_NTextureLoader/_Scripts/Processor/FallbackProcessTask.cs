using System;
using UnityEngine;

namespace Nextension.TextureLoader
{
    internal class FallbackProcessTask : AbsProcessTask
    {
        protected override async NTask exeProcess(byte[] imageData)
        {
            await loadTexture(imageData);
        }
        protected override async NTask exeProcess(Uri uri)
        {
            await loadTexture(await NUtils.getBinaryFrom(uri));
        }

        private async NTask loadTexture(byte[] inData)
        {
            try
            {
                var extension = NTextureUtils.getImageExtension(inData);
                setImageExtension(extension);
                if (!(extension == ImageExtension.JPG || extension == ImageExtension.PNG))
                {
                    setError(new Exception($"Image extension is not supported: {extension}"));
                    return;
                }

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
                        var (colors, outW, outH) = await NTextureUtils.asyncResizeBinaryColor(rawColor, tex1.width, tex1.height, maxDimension);

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

                _setting.apply(tex);
                setResult(tex, tex.width, tex.height);
            }
            catch (Exception e)
            {
                setError(e);
            }
        }
    }
}
