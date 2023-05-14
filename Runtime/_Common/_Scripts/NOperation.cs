using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public abstract class NOperation : CustomYieldInstruction
    {
        public override bool keepWaiting => !IsCompleted;
        public bool IsCompleted { get; private set; }
        public string Error => ErrorException.Message;
        public bool IsError { get; private set; }
        public Exception ErrorException { get; private set; }

        protected event Action onCompleteEvent;
        protected List<NOperation> dependedOperations = new List<NOperation>();

        public void addCompleteListener(Action callback)
        {
            if (IsCompleted)
            {
                try
                {
                    callback?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                return;
            }
            onCompleteEvent += callback;
        }
        public void removeCompleteListener(Action callback)
        {
            onCompleteEvent -= callback;
        }
        public void removeAllCompleteListener()
        {
            onCompleteEvent = null;
        }
        protected void innerSetComplete()
        {
            innerSetComplete(exception: null);

        }
        protected void innerSetComplete(string error)
        {
            if (IsCompleted)
            {
                Debug.LogWarning("Operation has completed");
                return;
            }
            if (string.IsNullOrEmpty(error))
            {
                innerSetComplete(exception: null);
            }
            else
            {
                var exception = new Exception(error);
                innerSetComplete(exception);
            }
        }
        protected void innerSetComplete(Exception exception)
        {
            if (IsCompleted)
            {
                Debug.LogWarning("Operation has completed");
                return;
            }

            ErrorException = exception;
            onInnerComplete();
            IsCompleted = true;
            IsError = exception != null;

            if (IsError)
            {
                Debug.LogWarning("NOperation error: " + Error);
            }

            invokeCompleteListener();
            invokeCompleteEventOnDependedOperations();
        }
        private void invokeCompleteListener()
        {
            if (!IsCompleted)
            {
                return;
            }
            try
            {
                onCompleteEvent?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            onCompleteEvent = null;
        }
        protected virtual void onInnerComplete() { }

        public void dependBy(NOperation otherOperation)
        {
            if (otherOperation == this)
            {
                return;
            }
            otherOperation.dependedOperations.add(otherOperation);
        }

        private void invokeCompleteEventOnDependedOperations()
        {
            foreach (var dependedOperation in dependedOperations)
            {
                dependedOperation.innerSetComplete(ErrorException);
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
                invokeProgressEventOnDependedOperations();
            }
        }
        protected override void onInnerComplete()
        {
            innerSetProgress(1);
        }
        public void addProgressListener(Action<float> callback)
        {
            onProgressEvent += callback;
        }
        public void removeProgressListener(Action<float> callback)
        {
            onProgressEvent -= callback;
        }
        public void removeAllProgressListener()
        {
            onProgressEvent = null;
        }

        private void invokeProgressEventOnDependedOperations()
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
