using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Nextension.Tween
{
    internal class FromToValueTween<TValue> where TValue : unmanaged
    {
        internal class Tweener : AbsFromToTweener<TValue>
        {
            private Action<TValue> _onValueChanged;
            public Tweener(TValue from, TValue to, Action<TValue> onValueChanged) : base(from, to)
            {
                this.from = from;
                this.destination = to;
                _onValueChanged = onValueChanged;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal unsafe override void writeJobDataToAddr(void* dst)
            {
                var data = new FromToData<TValue>(getCommonJobData(), from, destination);
                Unsafe.Write(dst, data);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal override ushort getRunnerId()
            {
                return TweenRunnerId<Chunk>.id;
            }

            internal override unsafe void invokeValueChanged(void* src)
            {
                _onValueChanged?.Invoke(Unsafe.Read<TValue>(src));
            }
        }
        internal unsafe class UnsafeTweener : AbsFromToTweener<TValue>
        {
            private object _target;
            private delegate*<object, TValue, void> _setPtr;

            public UnsafeTweener(TValue from, TValue to, object target, delegate*<object, TValue, void> setPtr) : base(from, to)
            {
                this.from = from;
                this.destination = to;
                _target = target;
                _setPtr = setPtr;
            }

            internal override void writeJobDataToAddr(void* dst)
            {
                var data = new FromToData<TValue>(getCommonJobData(), from, destination);
                Unsafe.Write(dst, data);
            }

            internal override ushort getRunnerId()
            {
                return TweenRunnerId<Chunk>.id;
            }

            internal override void invokeValueChanged(void* src)
            {
                _setPtr(_target, Unsafe.Read<TValue>(src));
            }
        }
        internal sealed class Chunk : AbsValueTweenChunk<TValue, Job, FromToData<TValue>>
        {
            protected override Job createNewJob()
            {
                return new Job(_jobDataNativeArr, _mask, _results);
            }
        }
        internal struct Job : IJobFor
        {
            [ReadOnly] private NativeArray<FromToData<TValue>> _jobDataNativeArr;
            [ReadOnly] private NativeArray<byte> _mask;
            [WriteOnly] private NativeArray<TValue> _results;

            internal Job(in NativeArray<FromToData<TValue>> jobDataNativeArr, in NativeArray<byte> mask, in NativeArray<TValue> results)
            {
                _jobDataNativeArr = jobDataNativeArr;
                _results = results;
                _mask = mask;
            }

            [BurstCompile]
            public void Execute(int index)
            {
                if (NUtils.checkBitMask(_mask, index))
                {
                    var data = _jobDataNativeArr[index];
                    var common = data.common;
                    var currentTime = common.updateMode == NUpdateMode.ScaleTime ? TweenStaticManager.currentTimeInJob.Data : TweenStaticManager.currentUnscaledTimeInJob.Data;
                    if (currentTime >= common.startTime)
                    {
                        var t = (currentTime - common.startTime) / common.duration;
                        NTweenUtils.ease(data.from, data.to, t, common.easeType, out var result);
                        _results[index] = result;
                    }
                }
            }
        }
    }
}