using System.Collections.Generic;
namespace Nextension.Tween
{
    internal class CancelControlManager
    {
        private Dictionary<AbsCancelControlKey, HashSet<NTweener>> _controlledTweeners = new Dictionary<AbsCancelControlKey, HashSet<NTweener>>();

        public void addControlledTweener(NTweener tweener)
        {
            HashSet<NTweener> hashset;
            if (!_controlledTweeners.TryGetValue(tweener.controlKey, out hashset))
            {
                hashset = new HashSet<NTweener>();
                _controlledTweeners.Add(tweener.controlKey, hashset);
            }
            if (!hashset.Contains(tweener))
            {
                hashset.Add(tweener);
            }
        }
        public void removeControlledTweener(NTweener tweener)
        {
            HashSet<NTweener> hashset;
            if (_controlledTweeners.TryGetValue(tweener.controlKey, out hashset))
            {
                hashset.Remove(tweener);
            }
        }
        public void cancel(AbsCancelControlKey controlKey)
        {
            HashSet<NTweener> hashset;
            if (_controlledTweeners.TryGetValue(controlKey, out hashset))
            {
                foreach (var item in hashset)
                {
                    item.controlKey = null;
                    item.cancel();
                }
            }
            _controlledTweeners.Remove(controlKey);
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

    //public enum CancelType
    //{
    //    ComponentDisabled,
    //    ObjectDeactivated,
    //    ObjectDestroyed,
    //}
    //internal abstract class AbsCancelCondition
    //{
    //    public abstract bool isCancelled();
    //}
    //internal class FuncCancelCondition : AbsCancelCondition
    //{
    //    public readonly Func<bool> condition;
    //    public FuncCancelCondition(Func<bool> condition)
    //    {
    //        this.condition = condition;
    //    }
    //    public override bool isCancelled()
    //    {
    //        try
    //        {
    //            return condition();
    //        }
    //        catch (Exception e)
    //        {
    //            Debug.LogException(e);
    //            return false;
    //        }
    //    }
    //}
    //internal class MonoBehaviourCancelCondition : AbsCancelCondition
    //{
    //    public readonly MonoBehaviour target;
    //    public readonly CancelType type;
    //    public MonoBehaviourCancelCondition(MonoBehaviour target, CancelType type)
    //    {
    //        this.target = target;
    //        this.type = type;
    //    }
    //    public override bool isCancelled()
    //    {
    //        switch (type)
    //        {
    //            case CancelType.ComponentDisabled:
    //                return !target || !target.enabled;
    //            case CancelType.ObjectDeactivated:
    //                return !target || !target.gameObject.activeSelf;
    //            default:
    //                return !target;
    //        }
    //    }
    //}
    //internal abstract class AbsCancelCondition
    //{
    //    public abstract bool isCancelled();
    //}
    //internal class FuncCancelCondition : AbsCancelCondition
    //{
    //    public readonly Func<bool> condition;
    //    public FuncCancelCondition(Func<bool> condition)
    //    {
    //        this.condition = condition;
    //    }
    //    public override bool isCancelled()
    //    {
    //        try
    //        {
    //            return condition();
    //        }
    //        catch (Exception e)
    //        {
    //            Debug.LogException(e);
    //            return false;
    //        }
    //    }
    //}
    //internal class MonoBehaviourCancelCondition : AbsCancelCondition
    //{
    //    public readonly MonoBehaviour target;
    //    public readonly CancelType type;
    //    public MonoBehaviourCancelCondition(MonoBehaviour target, CancelType type)
    //    {
    //        this.target = target;
    //        this.type = type;
    //    }
    //    public override bool isCancelled()
    //    {
    //        switch (type)
    //        {
    //            case CancelType.ComponentDisabled:
    //                return !target || !target.enabled;
    //            case CancelType.ObjectDeactivated:
    //                return !target || !target.gameObject.activeSelf;
    //            default:
    //                return !target;
    //        }
    //    }
    //}
}
