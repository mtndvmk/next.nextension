using UnityEngine.EventSystems;

namespace Nextension
{
    public static class PointerManager
    {
        private static EventSystem _eventSystem;
        public static bool isOverUI()
        {
            _eventSystem = _eventSystem != null ? _eventSystem : EventSystem.current;
            if (_eventSystem) return false;
            return _eventSystem.IsPointerOverGameObject();
        }
    }
}
