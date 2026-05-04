using System;
using UnityEngine;

namespace Nextension
{
    public class ScreenSizeChangedDetector
    {
        static ScreenSizeChangedDetector()
        {
            _screenSize.x = Screen.width;
            _screenSize.y = Screen.height;
            NUpdater.onLateUpdateEvent.add(lateUpdate);
        }

        private static Vector2Int _screenSize;
        public static Vector2Int ScreenSize => _screenSize;
        public static event Action<Vector2Int> onScreenResizeEvent;
        public static void clearEvent()
        {
            onScreenResizeEvent = null;
        }
        private static void lateUpdate()
        {
            int width = Screen.width;
            int height = Screen.height;

            if (width != _screenSize.x || height != _screenSize.y)
            {
                _screenSize.x = width;
                _screenSize.y = height;
                onScreenResizeEvent?.Invoke(ScreenSize);
            }
        }
    }
}