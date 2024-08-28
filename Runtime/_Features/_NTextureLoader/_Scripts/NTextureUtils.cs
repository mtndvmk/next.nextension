using System;
using System.IO;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Nextension.TextureLoader
{
    public enum ImageExtension
    {
        Unknown,
        JPG,
        BMP,
        GIF,
        PNG
    }
    public static class NTextureUtils
    {
        public static ImageExtension getImageExtension(Stream stream)
        {
            Span<byte> bytes = stackalloc byte[8];
            stream.Read(bytes);
            return getImageExtension(bytes);
        }
        public static ImageExtension getImageExtension(Span<byte> bytes)
        {
            ulong firstNum = NConverter.fromBytes<ulong>(bytes);

            var num2 = firstNum & 0xffff;
            if (num2 == 0xd8ff)
            {
                return ImageExtension.JPG;
            }
            if (num2 == 0x4d42)
            {
                return ImageExtension.BMP;
            }

            if ((firstNum & 0xffffff) == 0x464947)
            {
                return ImageExtension.GIF;
            }

            if (firstNum == 0x0a1a0a0d474e5089)
            {
                return ImageExtension.PNG;
            }

            return ImageExtension.Unknown;
        }
        /// <summary>
        /// A `T` item is a color
        /// </summary>
        public static async Task<(NativeArray<T>, int, int)> asyncResizeColor<T>(NativeArray<T> src, int srcWidth, int srcHeight, int maxDimension) where T : unmanaged
        {
            float higher = Mathf.Max(srcWidth, srcHeight);

            int outWidth;
            int outHeight;

            if (higher <= maxDimension)
            {
                outWidth = srcWidth;
                outHeight = srcHeight;
                return (src, outWidth, outHeight);
            }

            var ratio = higher / maxDimension;
            outWidth = (int)(srcWidth / ratio);
            outHeight = (int)(srcHeight / ratio);
            var targetSize = outWidth * outHeight;

            var dst = new NativeArray<T>(targetSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var job = new Job_ResizeTexture_PixelData<T>()
            {
                src = src,
                dst = dst,
                srcWidth = srcWidth,
                srcHeight = srcHeight,
                outWidth = outWidth,
                outHeight = outHeight,
                ratio = ratio,
            };
            var jobHandle = job.ScheduleByRef(targetSize, 64);
            await jobHandle;
            return (dst, outWidth, outHeight);
        }
        /// <summary>
        /// sizePerPixel bytes is binary of a color
        /// </summary>
        public static async Task<(NativeArray<byte>, int, int)> asyncResizeBinaryColor(NativeArray<byte> src, int srcWidth, int srcHeight, int maxDimension)
        {
            float higher = Mathf.Max(srcWidth, srcHeight);

            int outWidth;
            int outHeight;

            if (higher <= maxDimension)
            {
                outWidth = srcWidth;
                outHeight = srcHeight;
                return (src, outWidth, outHeight);
            }

            var ratio = higher / maxDimension;
            outWidth = (int)(srcWidth / ratio);
            outHeight = (int)(srcHeight / ratio);
            var targetSize = outWidth * outHeight;

            int sizePerPixel = src.Length / srcWidth / srcHeight;

            var nativeDst = new NativeArray<byte>(targetSize * sizePerPixel, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var job = new Job_ResizeTexture_Binary()
            {
                src = src,
                dst = nativeDst,
                srcWidth = srcWidth,
                srcHeight = srcHeight,
                outWidth = outWidth,
                outHeight = outHeight,
                ratio = ratio,
                sizePerPixel = sizePerPixel,
            };
            var jobHandle = job.ScheduleByRef(targetSize, 64);
            await jobHandle;

            return (nativeDst, outWidth, outHeight);
        }
        public static async Task<Texture2D> asyncResizeReadableTexture(Texture2D src, int maxDimension)
        {
            if (src == null) return null;
            var srcWidth = src.width;
            var srcHeight = src.height;
            float higher = Mathf.Max(srcWidth, srcHeight);
            if (higher <= maxDimension)
            {
                return src;
            }

            var originColorNativeArr = new NativeArray<Color32>(src.GetPixels32(), Allocator.TempJob);
            (var resizedColorNativeArr, var outWidth, var outHeight) = await asyncResizeColor(originColorNativeArr, srcWidth, srcHeight, maxDimension);
            var setting = new TextureSetting();
            Texture2D result = setting.createTexture(outWidth, outHeight);
            originColorNativeArr.Dispose();
            result.LoadRawTextureData(resizedColorNativeArr);
            resizedColorNativeArr.Dispose();
            await setting.apply(result);
            return result;
        }
        public static async Task<Texture2D> asyncResizeTextureUseBlit(Texture2D src, int maxDimension)
        {
            if (src == null) return null;
            var srcWidth = src.width;
            var srcHeight = src.height;
            float higher = Mathf.Max(srcWidth, srcHeight);
            if (higher <= maxDimension)
            {
                return src;
            }

            var ratio = higher / maxDimension;
            var outWidth = (int)(srcWidth / ratio);
            var outHeight = (int)(srcHeight / ratio);

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture tempRenderTexture = RenderTexture.GetTemporary(outWidth, outHeight);
            RenderTexture.active = tempRenderTexture;
            Graphics.Blit(src, tempRenderTexture);

            Texture2D result = new Texture2D(outWidth, outHeight, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, result.width, result.height), 0, 0);
            result.Apply(false, false);
            await new NWaitFrame(1);
            result.Compress(false);

            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(tempRenderTexture);
            return result;
        }

        /// <summary>
        /// Convert binary of RGBA color array (32bit) to Color array
        /// </summary>
        public static async Task<NativeArray<Color>> asyncConvertBinary32ToColor(NativeArray<byte> src)
        {
            var nativeDst = new NativeArray<Color>(src.Length / 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var job = new Job_ConvertBinary32ToColor()
            {
                src = src,
                dst = nativeDst,
            };
            var jobHandle = job.ScheduleByRef(nativeDst.Length, 64);
            await jobHandle;
            return nativeDst;
        }
        /// <summary>
        /// Convert binary of RGB color array (24bit) to Color array
        /// </summary>
        public static async Task<NativeArray<Color>> asyncConvertBinary24ToColor(NativeArray<byte> src)
        {
            var nativeDst = new NativeArray<Color>(src.Length / 3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var job = new Job_ConvertBinary24ToColor()
            {
                src = src,
                dst = nativeDst,
            };
            var jobHandle = job.ScheduleByRef(nativeDst.Length, 64);
            await jobHandle;
            return nativeDst;
        }
        /// <summary>
        /// Convert binary of RGBA color array (32bit) to binary of RGB color array (24bit)
        /// </summary>
        public static async Task<NativeArray<T>> asyncConvertT32ToT24<T>(NativeArray<T> src) where T : unmanaged
        {
            int colorCount = src.Length / 4;
            var nativeDst = new NativeArray<T>(colorCount * 3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var job = new Job_ConvertT32ToT24<T>()
            {
                src = src,
                dst = nativeDst,
            };
            var jobHandle = job.ScheduleByRef(colorCount, 64);
            await jobHandle;
            return nativeDst;
        }

        [BurstCompile]
        private struct Job_ResizeTexture_PixelData<T> : IJobParallelFor where T : unmanaged
        {
            [NativeDisableParallelForRestriction, WriteOnly] public NativeArray<T> dst;

            [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<T> src;
            [ReadOnly] public int srcWidth;
            [ReadOnly] public int srcHeight;
            [ReadOnly] public int outWidth;
            [ReadOnly] public int outHeight;
            [ReadOnly] public float ratio;

            public void Execute(int index)
            {
                var w = index % outWidth;
                var h = index / outWidth;
                var srcW = (int)(w * ratio);
                var srcIndex = srcW + (int)(h * ratio) * srcWidth;
                dst[index] = src[srcIndex];
            }
        }
        [BurstCompile]
        private struct Job_ResizeTexture_Binary : IJobParallelFor
        {
            [NativeDisableParallelForRestriction, WriteOnly] public NativeArray<byte> dst;

            [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<byte> src;
            [ReadOnly] public int srcWidth;
            [ReadOnly] public int srcHeight;
            [ReadOnly] public int outWidth;
            [ReadOnly] public int outHeight;
            [ReadOnly] public float ratio;
            [ReadOnly] public int sizePerPixel;

            public void Execute(int index)
            {
                var w = index % outWidth;
                var h = index / outWidth;
                var srcW = (int)(w * ratio);
                var srcIndex = srcW + (int)(h * ratio) * srcWidth;
                index *= sizePerPixel;
                srcIndex *= sizePerPixel;
                for (int i = 0; i < sizePerPixel; ++i)
                {
                    dst[index + i] = src[srcIndex + i];
                }
            }
        }
        [BurstCompile]
        private struct Job_ConvertBinary32ToColor : IJobParallelFor
        {
            [NativeDisableParallelForRestriction, WriteOnly] public NativeArray<Color> dst;

            [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<byte> src;

            public void Execute(int index)
            {
                var startSrcIndex = index * 4;
                dst[index] = new Color(src[startSrcIndex] / 255f, src[startSrcIndex + 1] / 255f, src[startSrcIndex + 2] / 255f, src[startSrcIndex + 3] / 255f);
            }
        }
        [BurstCompile]
        private struct Job_ConvertBinary24ToColor : IJobParallelFor
        {
            [NativeDisableParallelForRestriction, WriteOnly] public NativeArray<Color> dst;

            [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<byte> src;

            public void Execute(int index)
            {
                var startSrcIndex = index * 4;
                dst[index] = new Color(src[startSrcIndex] / 255f, src[startSrcIndex + 1] / 255f, src[startSrcIndex + 2] / 255f, 1);
            }
        }
        [BurstCompile]
        private struct Job_ConvertT32ToT24<T> : IJobParallelFor where T : unmanaged
        {
            [NativeDisableParallelForRestriction, WriteOnly] public NativeArray<T> dst;

            [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<T> src;

            public void Execute(int index)
            {
                var startSrcIndex = index * 4;
                var startDstIndex = index * 3;
                dst[startDstIndex] = src[startSrcIndex];
                dst[startDstIndex + 1] = src[startSrcIndex + 1];
                dst[startDstIndex + 2] = src[startSrcIndex + 2];
            }
        }
    }
}
