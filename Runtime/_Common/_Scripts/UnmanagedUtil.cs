using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nextension
{
    public static class UnmanagedUtil
    {
        private static readonly Dictionary<Type, bool> _cachedTypes = new();

        public static bool isUnmanaged(this Type type)
        {
            if (_cachedTypes.TryGetValue(type, out var result))
            {
                return result;
            }
            if (type.IsPrimitive || type.IsPointer || type.IsEnum)
            {
                result = true;
            }
            else if (type.IsValueType)
            {
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                bool isManaged = false;
                foreach (var field in fields)
                {
                    if (!isUnmanaged(field.FieldType))
                    {
                        isManaged = true;
                        break;
                    }
                }
                if (!isManaged)
                {
                    result = true;
                }
            }

            _cachedTypes.Add(type, result);
            return result;
        }
    }
}
