using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Nextension
{
    [BurstCompile]
    public struct SetValueJob<T> : IJobParallelFor where T : unmanaged
    {
        [NativeDisableUnsafePtrRestriction] private IntPtr _target;
        private readonly T _value;

        public SetValueJob(IntPtr target, T value)
        {
            _target = target;
            _value = value;
        }

        public unsafe void Execute(int index)
        {
            ((T*)_target)[index] = _value;
        }
    }
    public static class SetValueUtil
    {
        public static void setAllValue<T>(T[] target, T value) where T : unmanaged
        {
            int length = target.Length;
            if (length >= 32)
            {
                var gcHandle = GCHandle.Alloc(target, GCHandleType.Pinned);
                var job = new SetValueJob<T>(gcHandle.AddrOfPinnedObject(), value);
                job.RunByRef(target.Length);
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
