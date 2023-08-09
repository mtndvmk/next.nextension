using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension.Tween
{
    public abstract class NTweener : CustomYieldInstruction, ICancellable
    {
        internal NTweener(TweenType tweenType) { this.tweenType = tweenType; }

        internal ChunkIndex chunkIndex;
        internal readonly TweenType tweenType;
        internal TweenLoopType tweenLoopType;
        internal EaseType easeType;
        internal float duration;
        internal float startTime;
        internal bool isFinalized;

        private Action onStartedEvent;
        private Action onUpdatedEvent;
        private Action onCompletedEvent;
        private Action onCancelledEvent;
        private Action onFinalizedEvent;
        internal AbsCancelControlKey controlKey;

        private List<Func<bool>> cancelWhenFuncList;

        public NTweener setDelay(float delayTime)
        {
            if (delayTime < 0)
            {
                Debug.LogError($"{nameof(delayTime)} must equal or greater than 0");
                return this;
            }
            startTime = Time.time + delayTime;
            return this;
        }
        public NTweener setDuration(float duration)
        {
            this.duration = duration;
            return this;
        }
        public NTweener setEase(EaseType easeType)
        {
            this.easeType = easeType;
            return this;
        }

        public RunState Status { get; private set; }

        public override bool keepWaiting => !isFinalized;

        /// <summary>
        /// Stop tweener
        /// </summary>
        public void cancel()
        {
            if (Status.isFinished())
            {
                return;
            }
            Status = RunState.Cancelled;
            if (chunkIndex.chunkId != 0)
            {
                NTweenManager.cancelFromTweener(chunkIndex);
            }
            try
            {
                onCancelledEvent?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                invokeOnFinalize();
            }
        }
        /// <summary>
        /// Execute when NTweener is started
        /// </summary>
        public NTweener onStarted(Action onStarted)
        {
            if (Status >= RunState.Running)
            {
                Debug.LogWarning("Tweener has been started");
            }
            else
            {
                this.onStartedEvent = onStarted;
            }
            return this;
        }
        /// <summary>
        /// Execute when NTweener is updated
        /// </summary>
        public NTweener onUpdated(Action onUpdated)
        {
            if (Status.isFinished())
            {
                Debug.LogWarning("Tweener has been finished");
            }
            else
            {
                this.onUpdatedEvent = onUpdated;
            }
            return this;
        }
        /// <summary>
        /// Execute when Status is [Completed]
        /// </summary>
        public NTweener onCompleted(Action onCompleted)
        {
            if (Status == RunState.Completed)
            {
                try
                {
                    onCompleted?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else if (!Status.isFinished())
            {
                this.onCompletedEvent = onCompleted;
            }
            return this;
        }
        /// <summary>
        /// Execute when Status is [Cancelled]
        /// </summary>
        public NTweener onCancelled(Action onCancelled)
        {
            if (Status == RunState.Cancelled)
            {
                try
                {
                    onCancelled?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else if (!Status.isFinished())
            {
                this.onCancelledEvent = onCancelled;
            }
            return this;
        }
        /// <summary>
        /// Execute when Status is [Cancelled] or [Completed] or [Exception]
        /// </summary>
        public NTweener onFinalized(Action onFinalized)
        {
            if (Status.isFinished())
            {
                Debug.LogWarning("Tweener has been finished");
            }
            else
            {
                onFinalizedEvent = onFinalized;
            }
            return this;
        }
        public NTweener cancelWhen(Func<bool> condition)
        {
            if (cancelWhenFuncList == null)
            {
                cancelWhenFuncList = new List<Func<bool>>();
            }
            cancelWhenFuncList.Add(condition);
            return this;
        }
        public void setCancelControlKey(UnityEngine.Object target)
        {
            if (!target)
            {
                Debug.LogError("Component target is null");
                return;
            }
            setCancelControlKey(new ObjectCancelControlKey(target));
        }
        public void setCancelControlKey(AbsCancelControlKey cancelControlKey)
        {
            if (cancelControlKey.isInvalid())
            {
                Debug.LogError("cancelControlKey is invalid");
                return;
            }
            if (controlKey != null)
            {
                NTweenManager.removeControlledTweener(this);
            }
            controlKey = cancelControlKey;
            NTweenManager.addCancelControlledTweener(this);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void removeCancelControlKey()
        {
            innerRemoveCancelControlKey();
        }

        private void innerRemoveCancelControlKey()
        {
            if (controlKey != null)
            {
                NTweenManager.removeControlledTweener(this);
                controlKey = null;
            }
        }
        internal bool checkCancelFromFunc()
        {
            if (cancelWhenFuncList == null)
            {
                return false;
            }
            foreach (var condition in cancelWhenFuncList)
            {
                if (condition())
                {
                    cancel();
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void invokeOnStart()
        {
            if (Status >= RunState.Running)
            {
                return;
            }
            Status = RunState.Running;

            try
            {
                onStartedEvent?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void invokeOnUpdate()
        {
            try
            {
                onUpdatedEvent?.Invoke();
                checkCancelFromFunc();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void invokeOnComplete()
        {
            if (Status.isFinished())
            {
                return;
            }
            Status = RunState.Completed;
            try
            {
                onCompletedEvent?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                invokeOnFinalize();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void invokeOnFinalize()
        {
            if (!isFinalized)
            {
                isFinalized = true;
                try
                {
                    onFinalizedEvent?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    innerRemoveCancelControlKey();
                }
            }
        }

        internal abstract void doCompleteOnStart();
    }
}
