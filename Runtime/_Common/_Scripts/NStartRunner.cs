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

            runStartupMethods();

            Debug.Log("[NStartRunner] Initialized");
        }

        private static void onAppQuitting()
        {
            IsPlaying = false;
            SessionId = DateTimeOffset.Now.ToUnixTimeMilliseconds().GetHashCode();
        }
        private static void runStartupMethods()
        {
            var types = NUtils.getCustomTypes();

            List<(MethodInfo, int)> list = new List<(MethodInfo, int)>();

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute(typeof(StartupMethodAttribute)) as StartupMethodAttribute;
                    if (attr != null)
                    {
                        list.Add((method, attr.priority));
                    }
                }
            }

            list.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            for (int i = list.Count - 1; i >= 0; --i)
            {
                var method = list[i].Item1;
                method.Invoke(null, null);
            }
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

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class StartupMethodAttribute : Attribute
    {
        /// <summary>
        /// Callbacks with higher values are called before ones with lower values.
        /// </summary>
        internal int priority;
        public StartupMethodAttribute(int priority = 0) { this.priority = priority; }
    }
}