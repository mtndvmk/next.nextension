using System;

namespace Nextension
{
    public static class NTemp<T> where T : class, ITempable
    {
        private static T _temp;
        public static T get()
        {
            _temp ??= NUtils.createInstance<T>();
            return _temp;
        }
        public static void resetValue()
        {
            _temp?.resetValue();
        }
        public static void clearTemp()
        {
            if (_temp != null)
            {
                if (_temp is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _temp = null;
            }
        }
    }
    public interface ITempable
    {
        void resetValue();
    }
}
