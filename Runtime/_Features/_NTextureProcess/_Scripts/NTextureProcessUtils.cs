using System;
using System.IO;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Networking;

namespace Nextension.TextureProcess
{
    public static class NTextureProcessUtils
    {
        public static async Task<byte[]> getBinary(string url)
        {
            Uri uri = new Uri(url);
            if (uri.IsFile && Application.platform != RuntimePlatform.WebGLPlayer)
            {
                return File.ReadAllBytes(url);
            }
            UnityWebRequest unityWebRequest = UnityWebRequest.Get(uri);
            await unityWebRequest.SendWebRequest();
            if (string.IsNullOrEmpty(unityWebRequest.error))
            {
                var bin = unityWebRequest.downloadHandler.data;
                unityWebRequest.Dispose();
                return bin;
            }
            var err = unityWebRequest.error;
            throw new Exception(err);
        }
        public static async Task<(NativeArray<Color32>, int, int)> resize(NativeArray<Color32> src, int srcWidth, int srcHeight, int maxDimension)
        {
            var higher = Mathf.Max(srcWidth, srcHeight);

            int outWidth;
            int outHeight;

            if (higher <= maxDimension)
            {
                outWidth = srcWidth;
                outHeight = srcHeight;
                return (src, outWidth, outHeight);
            }

            var ratio = higher * 1f / maxDimension;
            outWidth = (int)(srcWidth / ratio);
            outHeight = (int)(srcHeight / ratio);
            var targetSize = outWidth * outHeight;

            NativeArray<Color32> dst = new NativeArray<Color32>(targetSize, Allocator.TempJob);

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
            var higher = Mathf.Max(srcWidth, srcHeight);

            int outWidth;
            int outHeight;

            if (higher <= maxDimension)
            {
                outWidth = srcWidth;
                outHeight = srcHeight;
                return (src, outWidth, outHeight);
            }

            var ratio = higher * 1f / maxDimension;
            outWidth = (int)(srcWidth / ratio);
            outHeight = (int)(srcHeight / ratio);
            var targetSize = outWidth * outHeight;

            int sizePerPixel = src.Length / srcWidth / srcHeight;

            NativeArray<byte> nativeDst = new NativeArray<byte>(targetSize * sizePerPixel, Allocator.TempJob);

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
        /// <summary>
        /// Convert binary of RGBA color array (32bit) to Color array
        /// </summary>
        public static async Task<NativeArray<Color>> convertBinaryOfColor32ToColor(NativeArray<byte> src)
        {
            NativeArray<Color> nativeDst = new NativeArray<Color>(src.Length / 4, Allocator.TempJob);
            var job = new Job_ConvertBinaryOfColor32ToColor()
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
        public static async Task<NativeArray<Color>> convertBinaryOfColor24ToColor(NativeArray<byte> src)
        {
            NativeArray<Color> nativeDst = new NativeArray<Color>(src.Length / 3, Allocator.TempJob);
            var job = new Job_ConvertBinaryOfColor24ToColor()
            {
                src = src,
                dst = nativeDst,
            };
            var jobHandle = job.Schedule(nativeDst.Length, 64);
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
        private struct Job_ConvertBinaryOfColor32ToColor : IJobParallelFor
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
        private struct Job_ConvertBinaryOfColor24ToColor : IJobParallelFor
        {
            [NativeDisableParallelForRestriction, WriteOnly] public NativeArray<Color> dst;

            [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<byte> src;

            public void Execute(int index)
            {
                var startSrcIndex = index * 4;
                dst[index] = new Color(src[startSrcIndex] / 255f, src[startSrcIndex + 1] / 255f, src[startSrcIndex + 2] / 255f, 1);
            }
        }
    }
}
