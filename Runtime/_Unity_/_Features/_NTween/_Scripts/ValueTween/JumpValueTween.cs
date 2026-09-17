using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Nextension.Tween
{

    internal class JumpValueTween<TValue> where TValue : unmanaged
    {
        internal sealed class Tweener : AbsJumpTweener<TValue>
        {
            private Action<TValue> _onValueChanged;
            public Tweener(TValue origin, TValue destination, TValue jumpHeight, Action<TValue> onValueChanged) : base(origin, destination, jumpHeight)
            {
                _onValueChanged = onValueChanged;
            }
            internal unsafe override void writeJobDataToAddr(void* dst)
            {
                var data = new JumpData<TValue>(getCommonJobData(), origin, destination, jumpHeight);
                Unsafe.Write(dst, data);
            }
            internal override ushort getRunnerId()
            {
                return TweenRunnerId<Chunk>.id;
            }
            internal override unsafe void invokeValueChanged(void* src)
            {
                _onValueChanged?.Invoke(Unsafe.Read<TValue>(src));
            }
        }
        internal sealed class Chunk : AbsValueTweenChunk<TValue, Job, JumpData<TValue>>
        {
            protected override Job createNewJob()
            {
                return new Job(_jobDataNativeArr, _mask, _results);
            }
        }
        internal struct Job : IJobFor
        {
            [ReadOnly] private NativeArray<JumpData<TValue>> _jobDataNativeArr;
            [ReadOnly] private NativeArray<byte> _mask;
            [WriteOnly] private NativeArray<TValue> _results;

            internal Job(NativeArray<JumpData<TValue>> jobDataNativeArr, NativeArray<byte> mask, NativeArray<TValue> results)
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
                        var deltaTime = currentTime - common.startTime;
                        var t = deltaTime / common.duration;
                        if (t >= 1f)
                        {
                            t = 1f;
                        }
                        NTweenUtils.ease(data.origin, data.destination, t, common.easeType, out var baseResult);
                        NTweenUtils.ease(default(TValue), data.jumpHeight, 4f * t * (1f - t), EaseType.Linear, out var jumpOffset);
                        _results[index] = NTweenUtils.addValue(baseResult, jumpOffset);
                    }
                }
            }
        }
    }
}
