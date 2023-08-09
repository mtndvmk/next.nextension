#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && NET_4_6
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Nextension.TextureProcess
{
    public partial class TextureProcessOperation
    {
        internal class Win
        {
            private TextureSetting _setting;
            public TextureProcessOperation Operation { get; }
            internal Win(TextureSetting textureSetting)
            {
                Operation = new TextureProcessOperation();
                _setting = textureSetting;
            }

            internal void startDecode(byte[] inData)
            {
                var memoryStream = new MemoryStream(inData);
                startDecode(memoryStream);

            }
            internal void startDecode(string localPath)
            {
                var fileStream = new FileStream(localPath, FileMode.Open);
                startDecode(fileStream);
            }

            private async void startDecode(Stream stream)
            {
                byte[] outData = null;
                int resizeWidth = 0;
                int resizeHeight = 0;

                await Task.Run(() =>
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
                    if (_setting.forceNotAlpha)
                    {
                        // because in unity color pixel is bottom to top but in bitmap is top to bottom
                        // and color in binary is rgb but bitmap is a bgr thus flip x and reverse array
                        resizedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        bitmapData = resizedBitmap.LockBits(new Rectangle(0, 0, resizeWidth, resizeHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                        outData = new byte[resizeWidth * resizedBitmap.Height * 3];
                        Marshal.Copy(bitmapData.Scan0, outData, 0, outData.Length);
                        outData = outData.Reverse().ToArray();
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
                    stream.Dispose();
                });

                if (outData != null)
                {
                    onLoadedPixelData(outData, resizeWidth, resizeHeight);
                }
                else
                {
                    Operation.innerFinalize(new TextureProcessException("Output data is null"));
                }
            }

            private void onLoadedPixelData(byte[] inData, int width, int height)
            {
                Texture2D tex;
                if (_setting.forceNotAlpha)
                {
                    tex = _setting.createTexture(width, height, TextureFormat.RGB24);
                }
                else
                {
                    tex = _setting.createTexture(width, height, TextureFormat.BGRA32);
                }
                tex.LoadRawTextureData(inData);
                tex.Apply();

                _setting.compress(tex);

                Operation.Texture = tex;
                Operation.OriginHeight = tex.height;
                Operation.OriginWidth = tex.width;
                Operation.innerFinalize();
            }
        }
    }
}
#endif