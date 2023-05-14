using System;
using UnityEngine;

namespace Nextension
{
    public class ScreenResizeDetector
    {
        private static ScreenResizeDetector _instance;
        public static ScreenResizeDetector Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ScreenResizeDetector();
                    NUpdater.onLateUpdateEvent.add(_instance.lateUpdate);
                }
                return _instance;
            }
        }

        public Vector2Int ScreenSize { get; private set; }
        public event Action<Vector2Int> onScreenResizeEvent;
        private void lateUpdate()
        {
            if (ScreenSize.x != Screen.width || ScreenSize.y != Screen.height)
            {
                ScreenSize = new Vector2Int(Screen.width, Screen.height);
                onScreenResizeEvent?.Invoke(ScreenSize);
            }
        }
    }
}
