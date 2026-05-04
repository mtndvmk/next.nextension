//#define RUN_LIFE_CYCLE_TEST
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace Nextension
{
    public static class NStartRunner
    {
        public static bool IsPlaying { get; private set; }
        public static int SessionId { get; private set; }
        public static int MainThreadId { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void startRunner()
        {
#if UNITY_EDITOR
            Application.quitting -= onAppQuitting;
#endif
            MainThreadId = Thread.CurrentThread.ManagedThreadId;
            Application.quitting += onAppQuitting;

            SessionId = DateTimeOffset.Now.ToUnixTimeMilliseconds().GetHashCode();
            IsPlaying = true;

            runStaticMethodsFromCache(StaticMethodCache.Instance != null ? StaticMethodCache.Instance.OnStartups : null);

            NDebug.Log("[NStartRunner] Initialized");
        }

        private static void onAppQuitting()
        {
#if UNITY_EDITOR
            SessionId = DateTimeOffset.Now.ToUnixTimeMilliseconds().GetHashCode();
            runStaticMethods<EditorQuittingMethodAttribute>();
#endif
            runStaticMethodsFromCache(StaticMethodCache.Instance != null ? StaticMethodCache.Instance.OnQuittings : null);
            IsPlaying = false;
        }
        private static void runStaticMethods<T>() where T : AbsStaticMethodAttribute
        {
            var types = NUtils.getCustomTypes();
            var bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            List<(MethodInfo, int)> list = new List<(MethodInfo, int)>();

            foreach (var type in types)
            {
                if (type.ContainsGenericParameters)
                {
                    continue;
                }
                var methods = type.GetMethods(bindingFlags);
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<T>();
                    if (attr != null)
                    {
                        if (method.GetParameters().Length > 0)
                        {
                            NDebug.LogWarning($"[{type.Name}.{method.Name}]: `{nameof(T)}` requires parameterless");
                            continue;
                        }
                        list.Add((method, attr.priority));
                    }
                }
            }

            int listCount = list.Count;

            if (listCount > 0)
            {
                list.Sort((a, b) => a.Item2.CompareTo(b.Item2));
                var span = list.asSpan();

                for (int i = listCount - 1; i >= 0; --i)
                {
                    var method = span[i].Item1;
                    try
                    {
                        method.Invoke(null, null);
                    }
                    catch (Exception ex)
                    {
                        NDebug.LogException(ex);
                    }
                }
            }
        }

        private static void runStaticMethodsFromCache(StaticMethodData[] cachedMethods)
        {
            if (cachedMethods == null || cachedMethods.Length == 0)
            {
                return;
            }
            var bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var data in cachedMethods)
            {
                var type = Type.GetType(data.typeName);
                if (type == null)
                {
                    NDebug.LogWarning($"[NStartRunner] Cannot resolve type: {data.typeName}");
                    continue;
                }
                var method = type.GetMethod(data.methodName, bindingFlags);
                if (method == null)
                {
                    NDebug.LogWarning($"[NStartRunner] Cannot resolve method: {data.typeName}.{data.methodName}");
                    continue;
                }
                try
                {
                    method.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    NDebug.LogException(ex);
                }
            }
        }

        public static void quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

#if RUN_LIFE_CYCLE_TEST && UNITY_EDITOR
        static string LOG_PREFIX = "LifeCycleOrder_";
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void SubsystemRegistration() => NDebug.Log(LOG_PREFIX + "Subsystem Registration"); // 0
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)] static void AfterAssembliesLoaded() => NDebug.Log(LOG_PREFIX + "AfterAssembliesLoaded"); // 1
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)] static void BeforeSplashScreen() => NDebug.Log(LOG_PREFIX + "Before Splash"); // 2
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void BeforeSceneLoad() => NDebug.Log(LOG_PREFIX + "BeforeScene"); // 3
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void AfterSceneLoad() => NDebug.Log(LOG_PREFIX + "AfterSceneLoad"); // 4
        [RuntimeInitializeOnLoadMethod] static void DefaultLog() => NDebug.Log(LOG_PREFIX + "Default is AfterSceneLoad"); // 5
        static class StaticContructor { static StaticContructor() => NDebug.Log(LOG_PREFIX + "Static Constructor"); } // ???
#endif
    }
}
