//#define RUN_LIFE_CYCLE_TEST
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Nextension
{
    public static class NStartRunner
    {
        public static bool IsPlaying { get; private set; }
        public static int SessionId { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void startRunner()
        {
            Application.quitting -= onAppQuitting;
            Application.quitting += onAppQuitting;

            SessionId = DateTimeOffset.Now.ToUnixTimeMilliseconds().GetHashCode();
            IsPlaying = true;

            runStaticMethods<StartupMethodAttribute>();

            Debug.Log("[NStartRunner] Initialized");
        }

        private static void onAppQuitting()
        {
            IsPlaying = false;
            SessionId = DateTimeOffset.Now.ToUnixTimeMilliseconds().GetHashCode();
#if UNITY_EDITOR
            runStaticMethods<EditorQuittingMethodAttribute>();
#endif
            runStaticMethods<QuittingMethodAttribute>();
        }
        private static void runStaticMethods<T>() where T : AbsStaticMethodAttribute
        {
            var types = NUtils.getCustomTypes();
            var bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            List<(MethodInfo, int)> list = new();

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
                            Debug.LogWarning($"[{type.Name}.{method.Name}]: `{nameof(T)}` requires parameterless");
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
                        Debug.LogException(ex);
                    }
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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void SubsystemRegistration() => Debug.Log(LOG_PREFIX + "Subsystem Registration"); // 0
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)] static void AfterAssembliesLoaded() => Debug.Log(LOG_PREFIX + "AfterAssembliesLoaded"); // 1
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)] static void BeforeSplashScreen() => Debug.Log(LOG_PREFIX + "Before Splash"); // 2
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void BeforeSceneLoad() => Debug.Log(LOG_PREFIX + "BeforeScene"); // 3
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void AfterSceneLoad() => Debug.Log(LOG_PREFIX + "AfterSceneLoad"); // 4
        [RuntimeInitializeOnLoadMethod] static void DefaultLog() => Debug.Log(LOG_PREFIX + "Default is AfterSceneLoad"); // 5
        static class StaticContructor { static StaticContructor() => Debug.Log(LOG_PREFIX + "Static Constructor"); } // ???
#endif
    }
}