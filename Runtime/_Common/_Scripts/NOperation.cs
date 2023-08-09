using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public abstract class NOperation : CustomYieldInstruction
    {
        public override bool keepWaiting => !IsFinalized;
        public bool IsFinalized { get; private set; }
        public string Error => ErrorException.Message;
        public bool IsError => ErrorException != null;
        public Exception ErrorException { get; private set; }

        protected event Action onFinalizedEvent;
        protected List<NOperation> dependedOperations = new List<NOperation>();

        public void addFinalizedEvent(Action onFinalized)
        {
            if (IsFinalized)
            {
                try
                {
                    onFinalized?.Invoke();
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
        public void removeFinalizedEvent(Action onFinalized)
        {
            onFinalizedEvent -= onFinalized;
        }
        public void removeFinalizedEvents()
        {
            onFinalizedEvent = null;
        }
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
                innerBeforeRunFinalizeEvent();
                IsFinalized = true;
                try
                {
                    onFinalizedEvent?.Invoke();
                    innerAfterRunFinalizeEvent();
                    finalizeOnDependedOperations();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                onFinalizedEvent = null;
            }
        }

        protected virtual void innerBeforeRunFinalizeEvent() { }
        protected virtual void innerAfterRunFinalizeEvent() { }

        public void dependBy(NOperation otherOperation)
        {
            if (otherOperation == this)
            {
                return;
            }
            otherOperation.dependedOperations.add(otherOperation);
        }

        private void finalizeOnDependedOperations()
        {
            foreach (var dependedOperation in dependedOperations)
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
        protected override void innerBeforeRunFinalizeEvent()
        {
            innerSetProgress(1);
        }
        
        public void addProgressEvent(Action<float> callback)
        {
            onProgressEvent += callback;
        }
        public void removeProgressEvent(Action<float> callback)
        {
            onProgressEvent -= callback;
        }
        public void removeProgressEvents()
        {
            onProgressEvent = null;
        }

        private void setProgressOnDependedOperations()
        {
            foreach (var dependedOperation in dependedOperations)
            {
                if (dependedOperation is NProgressOperation)
                {
                    (dependedOperation as NProgressOperation).innerSetProgress(Progress);
                }
            }
        }
    }
}
