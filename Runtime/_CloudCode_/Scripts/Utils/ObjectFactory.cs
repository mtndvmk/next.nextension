
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Nextension
{
    public static class ObjectFactory
    {
        private static readonly ConcurrentDictionary<Type, Func<object>> _ctorCache = new ConcurrentDictionary<Type, Func<object>>();

        private static class StaticFactory<T>
        {
            private static Func<T> _ctor;

            static StaticFactory()
            {
                initialize();
            }

            public static void initialize()
            {
                var type = typeof(T);

                if (!_ctorCache.ContainsKey(type))
                {
                    Func<object> func;

                    if (type.IsValueType)
                    {
                        _ctor = Activator.CreateInstance<T>;
                        func = () => Activator.CreateInstance<T>();
                        _ctorCache.TryAdd(type, func);
                        return;
                    }

                    var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                    var ctor = type.GetConstructor(flags, null, Type.EmptyTypes, null);

                    if (ctor == null)
                    {
                        NDebug.LogWarning($"Type: {type} does not have a default constructor. Use System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject instead.");
                        func = () => System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type);
                        _ctorCache.TryAdd(type, func);
                        _ctor = () => (T)func();
                    }
                    else if (ctor.IsPublic)
                    {
                        _ctor = Activator.CreateInstance<T>;
                        func = () => Activator.CreateInstance<T>();
                        _ctorCache.TryAdd(type, func);
                    }
                    else
                    {
                        _ctor = () => (T)ctor.Invoke(null);
                        func = () => ctor.Invoke(null);
                        _ctorCache.TryAdd(type, func);
                    }
                }
                else
                {
                    var cachedFunc = _ctorCache[type];
                    _ctor = () => (T)cachedFunc();
                }
            }

            public static T createInstance()
            {
                return _ctor();
            }
        }

        public static T createInstance<T>()
        {
            return StaticFactory<T>.createInstance();
        }

        private static Func<object> __tryGetOrInitialize(Type type)
        {
            if (_ctorCache.TryGetValue(type, out var func))
            {
                return func;
            }

            if (type.IsValueType)
            {
                func = () => Activator.CreateInstance(type);
            }
            else
            {
                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var ctor = type.GetConstructor(flags, null, Type.EmptyTypes, null);

                if (ctor == null)
                {
                    NDebug.LogWarning($"Type: {type} does not have a default constructor. Use System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject instead.");
                    func = () => System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type);
                }
                else if (ctor.IsPublic)
                {
                    func = () => Activator.CreateInstance(type);
                }
                else
                {
                    func = () => ctor.Invoke(null);
                }
            }

            _ctorCache.TryAdd(type, func);
            return func;
        }

        public static object createInstance(Type type)
        {
            return __tryGetOrInitialize(type).Invoke();
        }
    }
}
