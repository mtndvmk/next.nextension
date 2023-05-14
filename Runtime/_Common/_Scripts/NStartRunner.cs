using System;
using UnityEngine;

namespace Nextension
{
    public static class NStartRunner
    {
        public static bool IsQuitApp { get; private set; } = false;
        public static int SessionId { get; private set; }
        public static event Action onRunnerStartedEvent;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void startRunner()
        {
            Application.quitting += () =>
            {
                IsQuitApp = true;
                SessionId = DateTimeOffset.Now.ToUnixTimeMilliseconds().GetHashCode();
            };
            SessionId = DateTimeOffset.Now.ToUnixTimeMilliseconds().GetHashCode();
            NUpdater.initialize();
            NCoroutine.initialize();
            Debug.Log("[NStartRunner] Initialized");
            onRunnerStartedEvent?.Invoke();
        }
    }
}