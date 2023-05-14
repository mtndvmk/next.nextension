#if UNITY_WEBGL || UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Nextension.TextureProcess
{
    internal partial class NativeRequestFactory
    {
        internal class WebGL : INativeRequest
        {
            [DllImport("__Internal")]
            private static extern void decodeRGBA(string md5, string base64Str, string ext, int maxWidth, int maxHeight);
            [DllImport("__Internal")]
            private static extern void decodeRGBAWithSampleSize(string md5, string base64Str, string ext, int inSampleSize);
            [DllImport("__Internal")]
            private static extern void decodeRGBAUrlWithSampleSize(string md5, string url, int inSampleSize);
            [DllImport("__Internal")]
            private static extern void decodeRGBAUrl(string md5, string url, int maxWidth, int maxHeight);
            [DllImport("__Internal")]
            private static extern void loadTextureAtNative(IntPtr texPtr, string md5, string url, int inSampleSize);

            private string getExt(byte[] imageData)
            {
                return "png";
            }

            public TextureProcessOperation decodeRGBA(byte[] imageData, TextureSetting setting)
            {
                string md5 = Guid.NewGuid().ToString();
                var ext = getExt(imageData);
                TextureProcessOperation.WebGL webGLOperation = new TextureProcessOperation.WebGL(md5, setting);
                var base64Str = Convert.ToBase64String(imageData);

                if (setting.maxDimension.x > 0 && setting.maxDimension.y > 0)
                {
                    decodeRGBA(md5, base64Str, ext, setting.maxDimension.x, setting.maxDimension.y);
                }
                else
                {
                    decodeRGBAWithSampleSize(md5, base64Str, ext, setting.inSampleSize);
                }
                return webGLOperation.Operation;
            }

            /// <summary>
            /// Maybe error when url is local path
            /// </summary>
            public TextureProcessOperation decodeRGBA(string url, TextureSetting setting)
            {
                string md5 = Guid.NewGuid().ToString();
                TextureProcessOperation.WebGL webGLOperation = new TextureProcessOperation.WebGL(md5, setting);

                if (setting.maxDimension.x > 0 && setting.maxDimension.y > 0)
                {
                    decodeRGBAUrl(md5, url, setting.maxDimension.x, setting.maxDimension.y);
                }
                else
                {
                    decodeRGBAUrlWithSampleSize(md5, url, setting.inSampleSize);
                }
                return webGLOperation.Operation;
            }
            /// <summary>
            /// Maybe error when url is local path
            /// </summary>
            public TextureProcessOperation loadTextureAtNative(string url, TextureSetting setting)
            {
                string md5 = Guid.NewGuid().ToString();
                TextureProcessOperation.WebGL webGLOperation = new TextureProcessOperation.WebGL(md5, setting);
                var tex = new Texture2D(1, 1);
                loadTextureAtNative(tex.GetNativeTexturePtr(), md5, url, setting.inSampleSize);
                return webGLOperation.Operation;
            }
        }
    }
}
#endif