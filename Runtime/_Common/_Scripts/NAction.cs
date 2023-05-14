using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public class NAction
    {
        private List<Action> _actions = new List<Action>();
        public int ListenerCount => _actions.Count;
        public void add(Action callback)
        {
            if (callback != null)
            {
                _actions.Add(callback);
            }
        }
        public void remove(Action callback)
        {
            _actions.Remove(callback);
        }
        public void clear()
        {
            _actions.Clear();
        }
        public void invoke()
        {
            if (_actions.Count <= 0)
            {
                return;
            }
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                _actions[i].Invoke();
            }
        }
        public void tryInvoke(Action<Exception> onExceptionCallback = null)
        {
            if (_actions.Count <= 0)
            {
                return;
            }
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                try
                {
                    _actions[i].Invoke();
                }
                catch (Exception e)
                {
                    onExceptionCallback?.Invoke(e);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NAction operator +(NAction nAction, Action callback)
        {
            nAction.add(callback);
            return nAction;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NAction operator -(NAction nAction, Action callback)
        {
            nAction.remove(callback);
            return nAction;
        }
    }
    public class NAction<T>
    {
        private List<Action<T>> _actions = new List<Action<T>>();
        public int ListenerCount => _actions.Count;
        public void add(Action<T> callback)
        {
            if (callback != null)
            {
                _actions.Add(callback);
            }
        }
        public void remove(Action<T> callback)
        {
            _actions.Remove(callback);
        }
        public void clear()
        {
            _actions.Clear();
        }
        public void invoke(T result)
        {
            if (_actions.Count <= 0)
            {
                return;
            }
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                _actions[i].Invoke(result);
            }
        }
        public void tryInvoke(T result, Action<Exception> onExceptionCallback = null)
        {
            if (_actions.Count <= 0)
            {
                return;
            }
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                try
                {
                    _actions[i].Invoke(result);
                }
                catch (Exception e)
                {
                    onExceptionCallback?.Invoke(e);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NAction<T> operator +(NAction<T> nAction, Action<T> callback)
        {
            nAction.add(callback);
            return nAction;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NAction<T> operator -(NAction<T> nAction, Action<T> callback)
        {
            nAction.remove(callback);
            return nAction;
        }
    }

    public class NCallback
    {
        private NAction _nAction = new NAction();
        public int ListenerCount => _nAction.ListenerCount;
        public void add(Action callback)
        {
            _nAction.add(callback);
        }
        public void remove(Action callback)
        {
            _nAction.remove(callback);
        }
        internal void clear()
        {
            _nAction.clear();
        }
        internal void invoke()
        {
            _nAction.invoke();
        }
        internal void tryInvoke(Action<Exception> onExceptionCallback = null)
        {
            _nAction.tryInvoke(onExceptionCallback);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NCallback operator +(NCallback nCallback, Action callback)
        {
            nCallback.add(callback);
            return nCallback;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NCallback operator -(NCallback nCallback, Action callback)
        {
            nCallback.remove(callback);
            return nCallback;
        }
    }
    public class NCallback<T>
    {
        private NAction<T> _nAction = new NAction<T>();
        public int ListenerCount => _nAction.ListenerCount;
        public void add(Action<T> callback)
        {
            _nAction.add(callback);
        }
        public void remove(Action<T> callback)
        {
            _nAction.remove(callback);
        }
        internal void clear()
        {
            _nAction.clear();
        }
        internal void invoke(T result)
        {
            _nAction.invoke(result);
        }
        internal void tryInvoke(T result, Action<Exception> onExceptionCallback = null)
        {
            _nAction.tryInvoke(result, onExceptionCallback);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NCallback<T> operator +(NCallback<T> nCallback, Action<T> callback)
        {
            nCallback.add(callback);
            return nCallback;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NCallback<T> operator -(NCallback<T> nCallback, Action<T> callback)
        {
            nCallback.remove(callback);
            return nCallback;
        }
    }
}
