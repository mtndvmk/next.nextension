using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nextension.NByteBase
{
    public static class NByteBaseDatabase
    {
        internal static Dictionary<short, NByteBaseInfo> AllNByteBase { get; private set; } = new Dictionary<short, NByteBaseInfo>();

        [StartupMethod]
        public static void init()
        {
            AllNByteBase.Clear();
            Assembly nextensionAssembly = typeof(NByteBaseDatabase).Assembly;
            var types = NUtils.getCustomTypes();
            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<NByteBaseIdAttribute>();
                if (attr == null)
                {
                    continue;
                }
                bool isSupported = false;
                var baseType = type.BaseType;
                while (baseType != null)
                {
                    if (baseType == typeof(AbsNByteBase))
                    {
                        isSupported = true;
                        break;
                    }
                    baseType = baseType.BaseType;
                }
                if (type.IsGenericTypeDefinition)
                {
                    throw new Exception($"Not support Generic Type Definition: `{type.FullName}`");
                }
                if (!isSupported)
                {
                    throw new Exception($"`{type.FullName}` must inherit from {typeof(AbsNByteBase).FullName}");
                }
                if (attr.IsError)
                {
                    throw new Exception($"`{type.FullName}` has error: {attr.Error}");
                }
                if (type.Assembly != nextensionAssembly)
                {
                    if (attr.Id <= 0)
                    {
                        throw new Exception($"Please using [NByteBaseCustomAttribute] istead [NByteBaseDefaultAttribute] for `{type.FullName}`");
                    }
                }
                if (AllNByteBase.ContainsKey(attr.Id))
                {
                    throw new Exception($"Has another NByteBaseId with the same Id [{attr.Id}]: `{AllNByteBase[attr.Id].ByteBaseType.FullName}` <---> `{type.FullName}`");
                }
                var nByteBaseInfo = new NByteBaseInfo(attr.Id, type);
                AllNByteBase.Add(attr.Id, nByteBaseInfo);
            }
        }

        private static short getIdNoLog(Type byteBaseType)
        {
            foreach (var nByteBase in AllNByteBase.Values)
            {
                if (nByteBase.ByteBaseType == byteBaseType)
                {
                    return nByteBase.Id;
                }
            }
            return NByteBaseIdAttribute.DEFAULT_ID;
        }
        public static short getId(Type byteBaseType)
        {
            var id = getIdNoLog(byteBaseType);
            if (id == NByteBaseIdAttribute.DEFAULT_ID)
            {
                throw new Exception($"NByteBase isn't support type `{byteBaseType}`");
            }
            return id;
        }
        public static bool tryGetId(Type byteBaseType, out short id)
        {
            id = getIdNoLog(byteBaseType);
            return id != NByteBaseIdAttribute.DEFAULT_ID;
        }
        public static bool isNByteBaseType(Type byteBaseType)
        {
            return getIdNoLog(byteBaseType) != NByteBaseIdAttribute.DEFAULT_ID;
        }
    }
}
