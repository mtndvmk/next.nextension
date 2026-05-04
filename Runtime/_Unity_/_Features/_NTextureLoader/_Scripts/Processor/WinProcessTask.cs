#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && NET_4_6
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Nextension.TextureLoader
{
    internal class WinProcessTask : AbsProcessTask
    {
        protected override NTask exeProcess(byte[] imageData)
        {
            try
            {
                var memoryStream = new MemoryStream(imageData);
                return execute(memoryStream);
            }
            catch (Exception ex)
            {
                setError(ex);
            }
            return NTask.CompletedTask;
        }
        protected override async NTask exeProcess(Uri uri)
        {
            try
            {
                if (uri.IsFile)
                {
                    var fileStream = new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read);
                    await execute(fileStream);
                }
                else
                {
                    await exeProcess(await NUtils.getBinaryFrom(uri));
                }
            }
            catch (Exception ex)
            {
                setError(ex);
            }
        }
        private async NTask execute(Stream stream)
        {
            var extension = NTextureUtils.getImageExtension(stream);
            setImageExtension(extension);
            if (!(extension == ImageExtension.JPG || extension == ImageExtension.PNG))
            {
                setError(new Exception($"Image extension is not supported: {extension}"));
                stream.Dispose();
                return;
            }

            byte[] outData = null;
            int resizeWidth = 0;
            int resizeHeight = 0;

            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    Bitmap originBitmap = new Bitmap(stream);
                    float inSample = 0;
                    int maxDimension;
                    int originWidth = originBitmap.Width;
                    int originHeight = originBitmap.Height;

                    if (_setting.maxDimension.x > 0 && _setting.maxDimension.y > 0)
                    {
                        maxDimension = Mathf.Min(_setting.maxDimension.x, _setting.maxDimension.y);
                        inSample = Mathf.Min(originWidth, originHeight) * 1f / maxDimension;
                    }
                    else
                    {
                        inSample = _setting.inSampleSize;
                        maxDimension = Mathf.Min(originWidth, originHeight) / _setting.inSampleSize;

                    }

                    Bitmap resizedBitmap;
                    if (inSample > 1)
                    {
                        var newWidth = (int)(originWidth / inSample);
                        var newHeight = (int)(originHeight / inSample);
                        resizedBitmap = new Bitmap(originBitmap, newWidth, newHeight);
                        originBitmap.Dispose();
                    }
                    else
                    {
                        resizedBitmap = originBitmap;
                    }

                    resizeWidth = resizedBitmap.Width;
                    resizeHeight = resizedBitmap.Height;

                    BitmapData bitmapData;
                    if (_setting.forceNoAlpha)
                    {
                        // because in unity color pixel is bottom to top but in bitmap is top to bottom
                        // and color in binary is rgb but bitmap is a bgr thus flip x and reverse array
                        resizedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        bitmapData = resizedBitmap.LockBits(new Rectangle(0, 0, resizeWidth, resizeHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                        outData = new byte[resizeWidth * resizedBitmap.Height * 3];
                        Marshal.Copy(bitmapData.Scan0, outData, 0, outData.Length);
                        Array.Reverse(outData);
                    }
                    else
                    {
                        // because in unity color pixel is bottom to top but in bitmap is top to bottom thus flip y
                        resizedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        bitmapData = resizedBitmap.LockBits(new Rectangle(0, 0, resizeWidth, resizeHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        outData = new byte[resizeWidth * resizedBitmap.Height * 4];
                        Marshal.Copy(bitmapData.Scan0, outData, 0, outData.Length);
                    }
                    resizedBitmap.UnlockBits(bitmapData);
                    resizedBitmap.Dispose();
                }
                catch (Exception ex)
                {
                    setError(ex);
                }
                finally
                {
                    stream.Dispose();
                    if (outData != null)
                    {
                        onLoadedRawData(outData, resizeWidth, resizeHeight).forget();
                    }
                    else
                    {
                        setError(new TextureProcessException("Output data is null"));
                    }
                }
            });
        }
        private async NTaskVoid onLoadedRawData(byte[] inData, int width, int height)
        {
            try
            {
                await new NWaitMainThread();
                Texture2D tex;
                if (_setting.forceNoAlpha)
                {
                    tex = _setting.createTexture(width, height, TextureFormat.RGB24);
                }
                else
                {
                    tex = _setting.createTexture(width, height, TextureFormat.BGRA32);
                }
                tex.LoadRawTextureData(inData);
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
#endif