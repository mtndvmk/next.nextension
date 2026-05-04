using UnityEngine;

namespace Nextension
{
    internal class InsPoolContainer : MonoBehaviour
    {
        private static Transform _copiedPrefabContainer;
        public static Transform CopiedPrefabContainer
        {
            get
            {
                EditorCheck.checkEditorMode();
                if (!_copiedPrefabContainer)
                {
                    _copiedPrefabContainer = new GameObject("[InsPool.CopiedPrefabContainer]").transform;
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
                EditorCheck.checkEditorMode();
                if (!_container)
                {
                    _container = new GameObject("[InsPool.Container]").transform;
                    _container.setActive(false);
                    DontDestroyOnLoad(_container.gameObject);
                }
                return _container;
            }
        }
    }
}