using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Nextension
{
    [BurstCompile]
    public unsafe struct SetValueJob<T> : IJobParallelFor where T : unmanaged
    {
        [NativeDisableUnsafePtrRestriction] private readonly T* _target;
        private readonly T _value;

        public SetValueJob(IntPtr target, T value)
        {
            _target = (T*)target;
            _value = value;
        }
        public unsafe void Execute(int index)
        {
            _target[index] = _value;
        }
    }
    public static class SetValueUtil
    {
        public static void setAllValue<T>(T[] target, T value) where T : unmanaged
        {
            int length = target.Length;
            if (length >= 0)
            {
                var gcHandle = GCHandle.Alloc(target, GCHandleType.Pinned);
                var job = new SetValueJob<T>(gcHandle.AddrOfPinnedObject(), value);
                job.Run(target.Length);
                gcHandle.Free();
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    target[i] = value;
                }
            }
        }
    }
}
