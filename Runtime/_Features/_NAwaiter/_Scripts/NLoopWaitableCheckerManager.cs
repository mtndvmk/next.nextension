using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    internal static class NLoopWaitableCheckerManager
    {
#if UNITY_EDITOR
        [EditorQuittingMethod]
        private static void reset()
        {
            __clearGroups();
        }
        private static void __clearGroups()
        {
            if (_updateGroup != null)
            {
                NUpdater.onUpdateEvent.remove(_updateGroup.update);
                _updateGroup = null;
            }
            if (_lateUpdateGroup != null)
            {
                NUpdater.onLateUpdateEvent.remove(_lateUpdateGroup.update);
                _lateUpdateGroup = null;
            }
            if (_eofGroup != null)
            {
                NUpdater.onEndOfFrameEvent.remove(_eofGroup.update);
                _eofGroup = null;
            }
            if (_editorCheckers != null)
            {
                UnityEditor.EditorApplication.update -= _editorCheckers.update;
                _editorCheckers = null;
            }
        }
#endif

        private class NTaskCheckerGroup
        {
            private List<NLoopWaitableChecker> _checkers = new();
            private int _checkerCount;

            public void addChecker(NLoopWaitableChecker checker, bool isIgnoreFirstFrameCheck)
            {
                _checkers.Add(checker);
                if (!isIgnoreFirstFrameCheck)
                {
                    _checkerCount++;
                }
            }

            public void update()
            {
                if (_checkers.Count > 0)
                {
                    if (_checkerCount > 0)
                    {
                        __update(_checkers, _checkerCount - 1);
                    }
                    _checkerCount = _checkers.Count;
                }
            }
        }

        private static NTaskCheckerGroup _updateGroup;
        private static NTaskCheckerGroup _lateUpdateGroup;
        private static NTaskCheckerGroup _eofGroup;
        private static NTaskCheckerGroup _editorCheckers;

        private static readonly object _lockObj = new object();

        public static void addChecker(NLoopWaitableChecker checker, NLoopType loopType, bool isIgnoreFirstFrameCheck)
        {
            lock (_lockObj)
            {
                switch (loopType)
                {
                    case NLoopType.Update:
                        {
                            EditorCheck.checkEditorMode();
                            if (_updateGroup == null)
                            {
                                _updateGroup = new NTaskCheckerGroup();
                                NUpdater.onUpdateEvent.add(_updateGroup.update);
                            }
                            _updateGroup.addChecker(checker, isIgnoreFirstFrameCheck);
                            return;
                        }
                    case NLoopType.LateUpdate:
                        {
                            EditorCheck.checkEditorMode();
                            if (_lateUpdateGroup == null)
                            {
                                _lateUpdateGroup = new NTaskCheckerGroup();
                                NUpdater.onLateUpdateEvent.add(_lateUpdateGroup.update);
                            }
                            _lateUpdateGroup.addChecker(checker, isIgnoreFirstFrameCheck);
                            return;
                        }
                    case NLoopType.EndOfFrameUpdate:
                        {
                            EditorCheck.checkEditorMode();
                            if (_eofGroup == null)
                            {
                                _eofGroup = new NTaskCheckerGroup();
                                NUpdater.onEndOfFrameEvent.add(_eofGroup.update);
                            }
                            _eofGroup.addChecker(checker, isIgnoreFirstFrameCheck);
                            return;
                        }
                    case NLoopType.EditorUpdate:
                        {
#if UNITY_EDITOR
                            if (_editorCheckers == null)
                            {
                                _editorCheckers = new NTaskCheckerGroup();
                                UnityEditor.EditorApplication.update += _editorCheckers.update;
                            }
                            _editorCheckers.addChecker(checker, isIgnoreFirstFrameCheck);
#else
                            Debug.LogError("NLoopType.EditorUpdate is only available in Editor mode.");
#endif
                            return;
                        }
                }
            }
        }

        private static void __update(List<NLoopWaitableChecker> checkerList, int endIndex)
        {
            var checkerSpan = checkerList.asSpan();
            endIndex = Math.Min(endIndex, checkerSpan.Length - 1);
            for (int i = endIndex; i >= 0; i--)
            {
                var checker = checkerSpan[i];
                try
                {
                    var isFinished = checker.checkComplete();
                    if (isFinished)
                    {
                        __remove(checkerList, checker, i);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    __remove(checkerList, checker, i);
                }
            }
        }
        private static void __remove(List<NLoopWaitableChecker> checkerList, NLoopWaitableChecker checker, int index)
        {
            checker.release();
            checkerList.removeAtSwapBack(index);
        }
    }
}
