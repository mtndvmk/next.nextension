using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;

namespace Nextension
{
    public static class NDisposer
    {
        public static async void tryDispose_EditorOnly(Task dependOn, params IDisposable[] disposables)
        {
            await dependOn;
            foreach (var disposable in disposables)
            {
                disposable?.Dispose();
            }
        }
        public static async void tryDispose_EditorOnly(NWaitable dependOn, params IDisposable[] disposables)
        {
            await dependOn;
            foreach (var disposable in disposables)
            {
                disposable?.Dispose();
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void tryDispose_EditorOnly(JobHandle dependOn, params IDisposable[] disposables)
        {
#if UNITY_EDITOR
            NDisposer.tryDispose_EditorOnly(new NWaitUntil_Editor(() => dependOn.IsCompleted).startWaitable(), disposables);
#endif
        }
        [Conditional("UNITY_EDITOR")]
        public static void tryDispose_EditorOnly(CustomYieldInstruction dependOn, params IDisposable[] disposables)
        {
#if UNITY_EDITOR
            NDisposer.tryDispose_EditorOnly(new NWaitUntil_Editor(() => !dependOn.keepWaiting).startWaitable(), disposables);
#endif
        }
        [Conditional("UNITY_EDITOR")]
        public static void tryDispose_EditorOnly(AsyncOperation dependOn, params IDisposable[] disposables)
        {
#if UNITY_EDITOR
            NDisposer.tryDispose_EditorOnly(new NWaitUntil_Editor(() => !dependOn.isDone).startWaitable(), disposables);
#endif
        }
    }
}
