using System;
using UnityEngine;

namespace Nextension.NByteBase
{
    public static class NByteBaseCreator
    {
        internal static AbsNByteBase createNByteBase(short id)
        {
            var nByteBase = NByteBaseDatabase.AllNByteBase[id].ByteBaseType.createInstance<AbsNByteBase>();
            return nByteBase;
        }
        public static AbsNByteBase createNByteBase(Type valueType)
        {
            foreach (var baseTypeInfo in NByteBaseDatabase.AllNByteBase.Values)
            {
                if (baseTypeInfo.valueType == valueType)
                {
                    return createNByteBase(baseTypeInfo.Id);
                }
            }

            Debug.LogError($"NByteBase isn't support type [{valueType}]");

            return null;
        }
        public static NByteBase<T> createNByteBase<T>()
        {
            var valueType = typeof(T);
            return createNByteBase(valueType) as NByteBase<T>;
        }

    }
}
