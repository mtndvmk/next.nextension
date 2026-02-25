using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public class NAction
    {
        public static readonly Action<Exception> UnityLogException = UnityEngine.Debug.LogException;

        private readonly List<Action> _actions = new();
        public int Count => _actions.Count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void add(Action callback)
        {
            EditorCheck.requireNotNull(callback);
            _actions.Add(callback);
        }
        public void addIfNotPresent(Action callback)
        {
            EditorCheck.requireNotNull(callback);
            _actions.addIfNotPresent(callback);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool remove(Action callback)
        {
            return _actions.removeSwapBack(callback);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void clear()
        {
            _actions.Clear();
        }
        public void invoke()
        {
            int count = _actions.Count;
            if (count <= 0)
            {
                return;
            }
            var span = _actions.AsSpan();
            for (int i = 0; i < count; i++)
            {
                span[i].Invoke();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void tryInvoke()
        {
            tryInvoke(UnityLogException);
        }
        public void tryInvoke(Action<Exception> onExceptionCallback)
        {
            int count = _actions.Count;
            if (count <= 0)
            {
                return;
            }
            var span = _actions.AsSpan();
            if (onExceptionCallback != null)
            {
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        span[i].Invoke();
                    }
                    catch (Exception e)
                    {
                        onExceptionCallback.Invoke(e);
                    }
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        span[i].Invoke();
                    }
                    catch { }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NAction operator +(NAction nAction, Action callback)
        {
            (nAction ??= new()).add(callback);
            return nAction;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NAction operator -(NAction nAction, Action callback)
        {
            nAction?.remove(callback);
            return nAction;
        }
    }
    public class NAction<T>
    {
        public List<Action<T>>.Enumerator GetEnumerator()
        {
            return _actions.GetEnumerator();
        }
        private readonly List<Action<T>> _actions = new();
        public int Count => _actions.Count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void add(Action<T> callback)
        {
            EditorCheck.requireNotNull(callback);
            _actions.Add(callback);
        }
        public void addIfNotPresent(Action<T> callback)
        {
            EditorCheck.requireNotNull(callback);
            _actions.addIfNotPresent(callback);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool remove(Action<T> callback)
        {
            return _actions.removeSwapBack(callback);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void clear()
        {
            _actions.Clear();
        }
        public void invoke(T result)
        {
            int count = _actions.Count;
            if (count <= 0)
            {
                return;
            }
            var span = _actions.AsSpan();
            for (int i = 0; i < count; i++)
            {
                span[i].Invoke(result);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void tryInvoke(T result)
        {
            tryInvoke(result, NAction.UnityLogException);
        }
        public void tryInvoke(T result, Action<Exception> onExceptionCallback)
        {
            int count = _actions.Count;
            if (count <= 0)
            {
                return;
            }
            var span = _actions.AsSpan();
            if (onExceptionCallback != null)
            {
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        span[i].Invoke(result);
                    }
                    catch (Exception e)
                    {
                        onExceptionCallback.Invoke(e);
                    }
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        span[i].Invoke(result);
                    }
                    catch { }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NAction<T> operator +(NAction<T> nAction, Action<T> callback)
        {
            (nAction ??= new()).add(callback);
            return nAction;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NAction<T> operator -(NAction<T> nAction, Action<T> callback)
        {
            nAction?.remove(callback);
            return nAction;
        }
    }

    public class NCallback
    {
        private readonly NAction _nAction = new();
        public int Count => _nAction.Count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void add(Action callback)
        {
            _nAction.add(callback);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool remove(Action callback)
        {
            return _nAction.remove(callback);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void clear()
        {
            _nAction.clear();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void invoke()
        {
            _nAction.invoke();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void tryInvoke()
        {
            _nAction.tryInvoke();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void tryInvoke(Action<Exception> onExceptionCallback)
        {
            _nAction.tryInvoke(onExceptionCallback);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NCallback operator +(NCallback nCallback, Action callback)
        {
            (nCallback ??= new()).add(callback);
            return nCallback;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NCallback operator -(NCallback nCallback, Action callback)
        {
            nCallback?.remove(callback);
            return nCallback;
        }
    }
    public class NCallback<T>
    {
        private readonly NAction<T> _nAction = new();
        public int Count => _nAction.Count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void add(Action<T> callback)
        {
            _nAction.add(callback);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool remove(Action<T> callback)
        {
            return _nAction.remove(callback);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void clear()
        {
            _nAction.clear();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void invoke(T result)
        {
            _nAction.invoke(result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void tryInvoke(T result)
        {
            _nAction.tryInvoke(result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void tryInvoke(T result, Action<Exception> onExceptionCallback)
        {
            _nAction.tryInvoke(result, onExceptionCallback);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NCallback<T> operator +(NCallback<T> nCallback, Action<T> callback)
        {
            (nCallback ??= new()).add(callback);
            return nCallback;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NCallback<T> operator -(NCallback<T> nCallback, Action<T> callback)
        {
            nCallback?.remove(callback);
            return nCallback;
        }
    }
}
