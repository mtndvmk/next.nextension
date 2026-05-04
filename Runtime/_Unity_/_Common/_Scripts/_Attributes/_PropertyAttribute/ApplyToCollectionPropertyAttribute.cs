using UnityEngine;

namespace Nextension
{
    public abstract class ApplyToCollectionPropertyAttribute : PropertyAttribute
    {
#if UNITY_6000_0_OR_NEWER
        public ApplyToCollectionPropertyAttribute(bool applyToCollection = true) : base(applyToCollection)
        {

        }
#else
        public ApplyToCollectionPropertyAttribute(bool applyToCollection = true) : base()
        {

        }
#endif
    }
}
