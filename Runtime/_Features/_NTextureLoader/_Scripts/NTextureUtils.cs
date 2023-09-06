using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Nextension.TextureLoader
{
    public static class NTextureUtils
    {
        public static async Task<(NativeArray<Color32>, int, int)> resize(NativeArray<Color32> src, int srcWidth, int srcHeight, int maxDimension)
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

            var dst = new NativeArray<Color32>(targetSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var job = new Job_ResizeTexture_Color32()
            {
                src = src,
                dst = dst,
                srcWidth = srcWidth,
                srcHeight = srcHeight,
                outWidth = outWidth,
                outHeight = outHeight,
                ratio = ratio,
            };
            var jobHandle = job.Schedule(targetSize, 64);
            await jobHandle;
            return (dst, outWidth, outHeight);
        }
        public static async Task<(NativeArray<byte>, int, int)> resize(NativeArray<byte> src, int srcWidth, int srcHeight, int maxDimension)
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
            var jobHandle = job.Schedule(targetSize, 64);
            await jobHandle;

            return (nativeDst, outWidth, outHeight);
        }
        public static async Task<Texture2D> resize(Texture2D texture2D, int maxDimension)
        {
            var rawData = new NativeArray<Color32>(texture2D.GetPixels32(), Allocator.TempJob);
            (var nativeDst, var w, var h) = await resize(rawData, texture2D.width, texture2D.height, maxDimension);
            var setting = new TextureSetting();
            Texture2D result = setting.createTexture(w, h);
            rawData.Dispose();
            result.LoadRawTextureData(nativeDst);
            nativeDst.Dispose();
            await setting.apply(result);
            return result;
        }

        /// <summary>
        /// Convert binary of RGBA color array (32bit) to Color array
        /// </summary>
        public static async Task<NativeArray<Color>> convertBinary32ToColor(NativeArray<byte> src)
        {
            var nativeDst = new NativeArray<Color>(src.Length / 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var job = new Job_ConvertBinary32ToColor()
            {
                src = src,
                dst = nativeDst,
            };
            var jobHandle = job.Schedule(nativeDst.Length, 64);
            await jobHandle;
            return nativeDst;
        }
        /// <summary>
        /// Convert binary of RGB color array (24bit) to Color array
        /// </summary>
        public static async Task<NativeArray<Color>> convertBinary24ToColor(NativeArray<byte> src)
        {
            var nativeDst = new NativeArray<Color>(src.Length / 3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var job = new Job_ConvertBinary24ToColor()
            {
                src = src,
                dst = nativeDst,
            };
            var jobHandle = job.Schedule(nativeDst.Length, 64);
            await jobHandle;
            return nativeDst;
        }
        /// <summary>
        /// Convert binary of RGBA color array (32bit) to binary of RGB color array (24bit)
        /// </summary>
        public static async Task<NativeArray<T>> convertT32ToT24<T>(NativeArray<T> src) where T : unmanaged
        {
            var nativeDst = new NativeArray<T>(src.Length / 4 * 3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var job = new Job_ConvertT32ToT24<T>()
            {
                src = src,
                dst = nativeDst,
            };
            var jobHandle = job.Schedule(src.Length / 4, 64);
            await jobHandle;
            return nativeDst;
        }

        [BurstCompile]
        private struct Job_ResizeTexture_Color32 : IJobParallelFor
        {
            [NativeDisableParallelForRestriction, WriteOnly] public NativeArray<Color32> dst;

            [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<Color32> src;
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
