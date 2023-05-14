//#define DEBUG_LOG

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public static class NRunOnMainThread
    {
        private static Queue<ActionData> m_ActionQueue = new Queue<ActionData>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void initialize()
        {
            NUpdater.onEndOfFrameEvent.add(executeAction);
        }
        public static void run(Action action, bool isClearOnLoadScene = true)
        {
            m_ActionQueue.Enqueue(new ActionData()
            {
                unityAction = () => action?.Invoke(),
                isClearOnLoadScene = isClearOnLoadScene
            });
        }
        public static void clearQueue()
        {
            m_ActionQueue.Clear();
        }
        public static int clearActionOnLoadScene()
        {
            var queues = new Queue<ActionData>();
            var clearCount = queues.Count;
            while (m_ActionQueue.Count > 0)
            {
                var actionData = m_ActionQueue.Dequeue();
                if (!actionData.isClearOnLoadScene)
                {
                    queues.Enqueue(actionData);
                }
            }
            m_ActionQueue = queues;
            return clearCount - m_ActionQueue.Count;
        }

        private static void executeAction()
        {
            while (m_ActionQueue.Count > 0)
            {
                try
                {
                    m_ActionQueue.Dequeue()?.unityAction?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void clearDataOnLoadScene()
        {
            var clearCount = clearActionOnLoadScene();
            if (clearCount > 0)
            {
                Debug.Log("[NRunOnMainThread] Clear data on load scene: " + clearCount);
            }
        }
        private class ActionData
        {
            public Action unityAction;
            public bool isClearOnLoadScene;
        }
    }
}