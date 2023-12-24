using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nextension
{
    public static class NRunOnMainThread
    {
        private static List<ActionData> _actions = new();
        private static bool _isInitialized;
#if UNITY_EDITOR
        [EditorQuittingMethod]
        private static void reset()
        {
            if (_isInitialized)
            {
                _isInitialized = false;
                NUpdater.onEndOfFrameEvent.remove(invokeAction);
                SceneManager.sceneUnloaded -= onSceneUnloaded;
            }
            _actions.Clear();
        }
#endif
        private static void initialize()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                NUpdater.onEndOfFrameEvent.add(invokeAction);
                SceneManager.sceneUnloaded += onSceneUnloaded;
            }
        }

        private static void onSceneUnloaded(Scene scene)
        {
#if UNITY_EDITOR
            if (!NStartRunner.IsPlaying)
            {
                return;
            }
#endif
            int clearCount = 0;
            var span = _actions.asSpan();
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                var v = span[i];
                if (v.inScene.HasValue && v.inScene.Value == scene)
                {
                    _actions.removeAtSwapBack(i);
                    clearCount++;
                }
            }
            if (clearCount > 0)
            {
                Debug.Log($"[{nameof(NRunOnMainThread)}] Clear data on load scene: {clearCount} actions");
            }
        }

        public static void run(Action action, bool isClearOnLoadScene = false)
        {
            if (isClearOnLoadScene)
            {
                run(action, SceneManager.GetActiveScene());
            }
            else
            {
                run(action, null);
            }
        }
        public static void run(Action action, Scene? inScene)
        {
            initialize();
            _actions.Add(new ActionData()
            {
                action = action,
                inScene = inScene,
            });
        }
        public static void clear()
        {
            _actions.Clear();
        }
        private static void invokeAction()
        {
            while (_actions.Count > 0)
            {
                try
                {
                    _actions.takeAndRemoveLast().action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        private class ActionData
        {
            public Action action;
            internal Scene? inScene;
        }
    }
}