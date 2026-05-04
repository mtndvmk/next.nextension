using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Nextension
{
    public static class RectTransformRebuildList
    {
        private static HashSet<RectTransform> _rectTransform = new HashSet<RectTransform>();

        public static void add(RectTransform rectTransform)
        {
            var layoutRoot = getLayoutRoot(rectTransform);
            _rectTransform.Add(layoutRoot);
        }

        [LoopMethod(NLoopType.LateUpdate)]
        static void lateUpdate()
        {
            if (_rectTransform.Count == 0)
            {
                return;
            }
            foreach (RectTransform rectTransform in _rectTransform)
            {
                if (rectTransform != null)
                {
                    LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
                }
            }
            _rectTransform.Clear();
        }

        private static RectTransform getLayoutRoot(RectTransform rect)
        {
            if (rect == null || rect.gameObject == null)
                return null;

            var comps = ListPool<Component>.Get();
            bool validLayoutGroup = true;
            RectTransform layoutRoot = rect;
            var parent = layoutRoot.parent as RectTransform;
            while (validLayoutGroup && !(parent == null || parent.gameObject == null))
            {
                validLayoutGroup = false;
                parent.GetComponents(typeof(ILayoutGroup), comps);

                for (int i = 0; i < comps.Count; ++i)
                {
                    var cur = comps[i];
                    if (cur != null && cur is Behaviour behaviour && behaviour.isActiveAndEnabled)
                    {
                        validLayoutGroup = true;
                        layoutRoot = parent;
                        break;
                    }
                }

                parent = parent.parent as RectTransform;
            }

            ListPool<Component>.Release(comps);
            return layoutRoot;
        }
    }
}
