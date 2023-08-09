using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Nextension.NByteBase
{
    public static class NByteBaseUtils
    {
        private static Dictionary<Type, FieldInfo[]> _FieldCache = new Dictionary<Type, FieldInfo[]>();

        public static void clearCache()
        {
            _FieldCache.Clear();
        }

        public static bool isSupportByteBaseType(Type valueType)
        {
            foreach (var baseTypeInfo in NByteBaseDatabase.AllNByteBase.Values)
            {
                if (baseTypeInfo.valueType == valueType)
                {
                    return true;
                }
            }
            return false;
        }

        public static AbsNByteBase getNext(byte[] inData, ref int startIndex)
        {
            var id = NConverter.fromBytes<short>(inData, startIndex);
            var nByteBase = NByteBaseCreator.createNByteBase(id);

            nByteBase.setBytes(inData, ref startIndex);
            return nByteBase;
        }
        public static AbsNByteBase get(byte[] inData)
        {
            int startIndex = 0;
            return getNext(inData, ref startIndex);
        }

        public static NList toNList<T>(T instance)
        {
            return localToNList(instance, typeof(T), NByteBaseableMode.ALL);
        }
        public static NList toNList(object instance, Type objectType)
        {
            return localToNList(instance, objectType, NByteBaseableMode.ALL);
        }
        public static NList toNList<T>(T instance, NByteBaseableMode mode)
        {
            return localToNList(instance, typeof(T), mode);
        }
        public static NList toNList(object instance, Type objectType, NByteBaseableMode mode)
        {
            return localToNList(instance, objectType, mode);
        }

        private static NList localToNList(object instance, Type objectType, NByteBaseableMode mode)
        {
            try
            {
                var inType = objectType;
                if (!_FieldCache.ContainsKey(inType))
                {
                    var fs = inType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    _FieldCache.Add(inType, fs);
                }
                var fields = _FieldCache[inType];

                NList nList = new NList();

                bool isAllowNormalField = mode == NByteBaseableMode.ALL || mode == NByteBaseableMode.NORMAL_FIELD_ONLY;
                bool isAllowIdField = mode == NByteBaseableMode.ALL || mode == NByteBaseableMode.ID_FIELD_ONLY;

                foreach (var field in fields)
                {
                    if (!field.FieldType.IsSerializable)
                    {
                        continue;
                    }
                    var fieldValue = field.GetValue(instance);
                    if (fieldValue == null)
                    {
                        continue;
                    }
                    NByteBaseableIdAttribute attr = field.GetCustomAttribute(typeof(NByteBaseableIdAttribute)) as NByteBaseableIdAttribute;

                    if (attr == null)
                    {
                        if (isAllowNormalField)
                        {
                            var nByteBase = getNPairValue(field.Name, field.FieldType, fieldValue, mode);
                            if (nByteBase == null)
                            {
                                continue;
                            }
                            nList.add(nByteBase);
                        }
                    }
                    else
                    {
                        if (isAllowIdField)
                        {
                            var nByteBase = getNPairValue(attr.OrderId, field.FieldType, fieldValue, mode);
                            if (nByteBase == null)
                            {
                                continue;
                            }
                            nList.add(nByteBase);
                        }
                    }
                }

                if (nList.Count > 0)
                {
                    return nList;
                }
                return null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        private static AbsNByteBase getNPairValue(byte key, Type fieldType, object fieldValue, NByteBaseableMode mode)
        {
            var tempNByteBase = getNByteBaseableHashItem(fieldType, fieldValue, mode);
            if (tempNByteBase == null) return null;
            var nIdValuePair = new NIdValuePair();
            nIdValuePair.set(key, tempNByteBase);
            return nIdValuePair;
        }
        private static AbsNByteBase getNPairValue(string key, Type fieldType, object fieldValue, NByteBaseableMode mode)
        {
            var tempNByteBase = getNByteBaseableHashItem(fieldType, fieldValue, mode);
            if (tempNByteBase == null) return null;
            var nIdValuePair = new NStringValuePair();
            nIdValuePair.set(key, tempNByteBase);
            return nIdValuePair;
        }
        private static AbsNByteBase getNByteBaseableHashItem(Type fieldType, object fieldValue, NByteBaseableMode mode)
        {
            AbsNByteBase fieldByteBase = null;

            // check field type is NByteBase
            if (NByteBaseDatabase.isNByteBaseType(fieldType))
            {
                fieldByteBase = fieldValue as AbsNByteBase;
                if (fieldByteBase == null)
                {
                    return null;
                }
                return fieldByteBase;
            }

            // check field type is NByteBase supported
            if (isSupportByteBaseType(fieldType))
            {
                if (fieldValue == null)
                {
                    return null;
                }
                fieldByteBase = NByteBaseCreator.createNByteBase(fieldType);
                fieldByteBase.setValue(fieldValue);
                if (fieldByteBase == null)
                {
                    return null;
                }
                return fieldByteBase;
            }

            // check field type is Array or List
            if (fieldType.GetInterface("IList") != null)
            {
                var elementTypes = fieldType.GetGenericArguments();
                var elementType = elementTypes.Length > 0 ? elementTypes[0] : fieldType.GetElementType();
                if (!elementType.IsSerializable)
                {
                    return null;
                }
                NList tempList = new NList();
                var arr = fieldValue as IList;
                foreach (var item in arr)
                {
                    var tempNByteBase = getNByteBaseableHashItem(elementType, item, mode);
                    if (tempNByteBase != null)
                    {
                        tempList.add(tempNByteBase);
                    }
                }
                return tempList;
            }

            //// check field type is Array
            //if (fieldType.BaseType == typeof(Array))
            //{
            //    var elementType = fieldType.GetElementType();
            //    if (!elementType.IsSerializable)
            //    {
            //        return null;
            //    }
            //    NList tempList = new NList();
            //    var arr = fieldValue as Array;
            //    foreach (var item in arr)
            //    {
            //        var tempNByteBase = getNByteBaseableHashItem(elementType, item, mode);
            //        if (tempNByteBase != null)
            //        {
            //            tempList.add(tempNByteBase);
            //        }
            //    }
            //    return tempList;
            //}

            //// check field type is List<>
            //if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            //{
            //    var elementType = fieldType.GetGenericArguments()[0];
            //    if (!elementType.IsSerializable)
            //    {
            //        return null;
            //    }
            //    NList tempList = new NList();
            //    var arr = fieldValue as IList;
            //    foreach (var item in arr)
            //    {
            //        var tempNByteBase = getNByteBaseableHashItem(elementType, item, mode);
            //        if (tempNByteBase != null)
            //        {
            //            tempList.add(tempNByteBase);
            //        }
            //    }
            //    return tempList;
            //}

            fieldByteBase = localToNList(fieldValue, fieldType, mode);
            if (fieldByteBase == null)
            {
                return null;
            }
            return fieldByteBase;
        }


        public static T fromNByteBase<T>(NList nList)
        {
            var inType = typeof(T);
            var instance = fromNByteBase(nList, inType);
            return (T)instance;
        }
        public static object fromNByteBase(NList nList, Type objectType)
        {
            var instance = objectType.createInstance();
            try
            {
                for (byte i = 0; i < nList.Count; ++i)
                {
                    var nByteBaseType = nList[i].GetType();
                    if (nByteBaseType == typeof(NIdValuePair))
                    {
                        var nIdValuePair = nList[i] as NIdValuePair;
                        foreach (var idValue in nIdValuePair.Value)
                        {
                            instance.setNByteBaseableValue(objectType, idValue.Key, idValue.Value);
                        }
                    }
                    else
                    {
                        var nStrValuePair = nList[i] as NStringValuePair;
                        foreach (var strValue in nStrValuePair.Value)
                        {
                            instance.setNByteBaseableValue(objectType, strValue.Key, strValue.Value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return instance;
        }

        private static void setNByteBaseableValue(this object instance, Type instanceType, byte orderId, AbsNByteBase value)
        {
            var inType = instanceType;
            if (!_FieldCache.ContainsKey(inType))
            {
                var fs = inType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                _FieldCache.Add(inType, fs);
            }
            var fields = _FieldCache[inType];

            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute(typeof(NByteBaseableIdAttribute)) as NByteBaseableIdAttribute;
                if (attr == null)
                {
                    continue;
                }
                if (attr.OrderId == orderId)
                {
                    instance.setNByteBaseValue(field, value);
                    return;
                }
            }
        }
        private static void setNByteBaseableValue(this object instance, Type instanceType, string fieldName, AbsNByteBase value)
        {
            var inType = instanceType;
            var fields = inType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (field.Name == fieldName)
                {
                    instance.setNByteBaseValue(field, value);
                    return;
                }
            }
        }

        private static void setNByteBaseValue(this object instance, FieldInfo fieldInfo, AbsNByteBase value)
        {
            var fieldType = fieldInfo.FieldType;
            // if field type is a NByteBaseType
            if (NByteBaseDatabase.isNByteBaseType(fieldType))
            {
                fieldInfo.SetValue(instance, value);
                return;
            }
            // if field type is NByteBase supported
            if (isSupportByteBaseType(fieldType))
            {
                fieldInfo.SetValue(instance, value.getValue());
                return;
            }

            // if field type is Array
            if (fieldType.BaseType == typeof(Array))
            {
                var tempNList = value as NList;
                var elementType = fieldType.GetElementType();
                var arr = Array.CreateInstance(elementType, tempNList.Count);

                for (byte i = 0; i < tempNList.Count; ++i)
                {
                    var item = tempNList[i].getValue();
                    arr.SetValue(item, i);
                }

                fieldInfo.SetValue(instance, arr);
                return;
            }

            // if field type is List<>
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var tempNList = value as NList;
                var arr = fieldType.createInstance() as IList;

                for (byte i = 0; i < tempNList.Count; ++i)
                {
                    var item = tempNList[i].getValue();
                    arr.Add(item);
                }

                fieldInfo.SetValue(instance, arr);
                return;
            }

            var serializedObject = fromNByteBase(value as NList, fieldType);
            fieldInfo.SetValue(instance, serializedObject);
        }
    }

    public class NByteBaseInfo
    {
        public short Id { get; private set; }
        public Type ByteBaseType { get; private set; }
        public Type valueType { get; private set; }

        public NByteBaseInfo(short id, Type byteBaseType)
        {
            Id = id;
            ByteBaseType = byteBaseType;
            try
            {
                valueType = byteBaseType.BaseType.GetProperty("Value").PropertyType;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}