using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    public abstract class NOperation : CustomYieldInstruction
    {
        public override bool keepWaiting => !IsFinalized;
        public bool IsFinalized { get; private set; }
        public string Error => ErrorException?.Message;
        public bool IsError => ErrorException != null;
        public Exception ErrorException { get; private set; }

        protected event Action onFinalizedEvent;
        protected List<NOperation> _dependedOperations;

        public void addFinalizedEvent(Action onFinalized)
        {
            if (IsFinalized)
            {
                try
                {
                    onFinalized.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                onFinalizedEvent += onFinalized;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void removeFinalizedEvent(Action onFinalized)
        {
            onFinalizedEvent -= onFinalized;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void removeFinalizedEvents()
        {
            onFinalizedEvent = null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void innerFinalize()
        {
            innerFinalize(exception: null);

        }
        protected void innerFinalize(Exception exception)
        {
            if (IsFinalized)
            {
                Debug.LogWarning("Operation has completed");
            }
            else
            {
                if (exception != null)
                {
                    if (exception is KeepStackTraceException)
                    {
                        ErrorException = exception;
                    }
                    else
                    {
                        ErrorException = new KeepStackTraceException(exception);
                    }
                }
                runFinalizedEvent();
            }
        }
        private void runFinalizedEvent()
        {
            if (!IsFinalized)
            {
                onBeforeRunFinalizeEvent();
                IsFinalized = true;
                if (onFinalizedEvent != null)
                {
                    try
                    {
                        onFinalizedEvent.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    finally
                    {
                        onFinalizedEvent = null;
                    }
                }

                onAfterRunFinalizeEvent();
                finalizeOnDependedOperations();
            }
        }

        protected virtual void onBeforeRunFinalizeEvent() { }
        protected virtual void onAfterRunFinalizeEvent() { }

        public void dependBy(NOperation otherOperation)
        {
            if (otherOperation == this)
            {
                return;
            }
            (otherOperation._dependedOperations ??= new(1)).addIfNotPresent(otherOperation);
        }

        private void finalizeOnDependedOperations()
        {
            if (_dependedOperations == null)
            {
                return;
            }
            foreach (var dependedOperation in _dependedOperations.AsSpan())
            {
                dependedOperation.innerFinalize(ErrorException);
            }
        }
    }
    public abstract class NProgressOperation : NOperation
    {
        public float Progress { get; private set; }
        private event Action<float> onProgressEvent;
        protected void innerSetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (Progress != progress)
            {
                Progress = progress;
                try
                {
                    onProgressEvent?.Invoke(progress);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                setProgressOnDependedOperations();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void onBeforeRunFinalizeEvent()
        {
            innerSetProgress(1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void addProgressEvent(Action<float> callback)
        {
            onProgressEvent += callback;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void removeProgressEvent(Action<float> callback)
        {
            onProgressEvent -= callback;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void removeProgressEvents()
        {
            onProgressEvent = null;
        }

        private void setProgressOnDependedOperations()
        {
            if (_dependedOperations == null)
            {
                return;
            }
            foreach (var dependedOperation in _dependedOperations.AsSpan())
            {
                if (dependedOperation is NProgressOperation progressOperation)
                {
                    progressOperation.innerSetProgress(Progress);
                }
            }
        }
    }
}
