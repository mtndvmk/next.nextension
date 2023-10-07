using System.Collections.Generic;
namespace Nextension.Tween
{
    internal class CancelControlManager
    {
        private Dictionary<AbsCancelControlKey, HashSet<NTweener>> _controlledTweeners = new();

        public void addControlledTweener(NTweener tweener)
        {
            if (!_controlledTweeners.TryGetValue(tweener.controlKey, out var hashset))
            {
                hashset = new(1);
                _controlledTweeners.Add(tweener.controlKey, hashset);
            }
            if (!hashset.Contains(tweener))
            {
                hashset.Add(tweener);
            }
        }
        public void removeControlledTweener(NTweener tweener)
        {
            if (_controlledTweeners.TryGetValue(tweener.controlKey, out var hashset))
            {
                hashset.Remove(tweener);
            }
        }
        public void cancel(AbsCancelControlKey controlKey)
        {
            if (_controlledTweeners.TryGetValue(controlKey, out var hashset))
            {
                foreach (var item in hashset)
                {
                    item.controlKey = null;
                    item.cancel();
                }
                _controlledTweeners.Remove(controlKey);
            }
        }
        public void cancelInvalid()
        {
            var keys = _controlledTweeners.Keys;
            foreach (var k in keys)
            {
                if (k.isInvalid())
                {
                    cancel(k);
                }
            }
        }
    }
}
