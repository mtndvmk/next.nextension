using UnityEngine;

namespace Nextension.Tween
{
    public abstract class AbsCancelControlKey
    {
        protected readonly long key;
        protected AbsCancelControlKey(long key)
        {
            this.key = key;
        }
        public override int GetHashCode()
        {
            return this.key.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is not AbsCancelControlKey)
            {
                return false;
            }
            var cc = obj as AbsCancelControlKey;
            return cc.key.Equals(key);
        }
        public virtual bool isInvalid() => false;
    }
    internal class ObjectCancelControlKey : AbsCancelControlKey
    {
        public ObjectCancelControlKey(Object target) : base(target.GetInstanceID() << 32)
        {
            this.target = target;
        }
        public readonly Object target;
        public override bool isInvalid() => !target;
    }
    public class Int32CancelControlKey : AbsCancelControlKey
    {
        public Int32CancelControlKey(int key) : base(key)
        {
        }
    }
}