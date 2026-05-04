using System;
using UnityEngine;

namespace Nextension
{
    internal static class DisposeManager
    {
        private readonly static NBListCompareHashCode<IDisposable> _disposables = new NBListCompareHashCode<IDisposable>();
        private class OnCompiled : IOnCompiled
        {
            static void onLoadOrRecompiled()
            {
                disposeAll();
            }
        }
        static DisposeManager()
        {
            Application.quitting += disposeAll;
        }

        private static void disposeAll()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _disposables.Clear();
        }

        public static void add(IDisposable disposable)
        {
            _disposables.AddAndSortIfNotPresent(disposable);
        }
    }
}
