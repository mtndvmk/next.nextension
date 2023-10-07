using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension.Tween
{
    public abstract class AbsCancelControlKey
    {
        protected readonly long _key;
        protected AbsCancelControlKey(long key)
        {
            this._key = key;
        }
        public override int GetHashCode()
        {
            return this._key.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null || obj is not AbsCancelControlKey cc)
            {
                return false;
            }
            return cc._key.Equals(_key);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool isInvalid() => false;
    }
    internal class ObjectCancelControlKey : AbsCancelControlKey
    {
        public ObjectCancelControlKey(Object target) : base((long)target.GetInstanceID() << 32)
        {
            this.target = target;
        }
        public readonly Object target;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool isInvalid() => !target;
    }
    public class Int32CancelControlKey : AbsCancelControlKey
    {
        public Int32CancelControlKey(int key) : base(key)
        {
        }
    }
}