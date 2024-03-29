using UnityEngine;

namespace Nextension
{
    internal class InstancesPoolContainer : MonoBehaviour
    {
        private static Transform _copiedPrefabContainer;
        public static Transform CopiedPrefabContainer
        {
            get
            {
                InternalCheck.checkEditorMode();
                if (!_copiedPrefabContainer)
                {
                    _copiedPrefabContainer = new GameObject("[InstancesPool.CopiedPrefabContainer]").transform;
                    _copiedPrefabContainer.SetParent(Container);
                }
                return _copiedPrefabContainer;
            }
        }
        private static Transform _container;
        public static Transform Container
        {
            get
            {
                InternalCheck.checkEditorMode();
                if (!_container)
                {
                    _container = new GameObject("[InstancesPool.Container]").transform;
                    _container.setActive(false);
                    DontDestroyOnLoad(_container.gameObject);
                }
                return _container;
            }
        }
    }
}