using System;
using UnityEngine;

namespace Nextension
{
    [Serializable]
    public sealed class RefInResources<T> : IRefInResources where T : UnityEngine.Object
    {
        [SerializeField] private string _path;
        [SerializeField] private string _guid;
        private T _value;
        public T Value
        {
            get
            {
                if (_value.isNull())
                {
#if UNITY_EDITOR
                    Debug.LogError("Path is null or empty");
#endif
                    _value = Resources.Load(_path) as T;
                }
                return _value;
            }
        }
        public bool IsLoaded => !_value.isNull();

#if UNITY_EDITOR
        public string getGuid()
        {
            return _guid;
        }
        public string getPath()
        {
            return _path;
        }
        public void setValue(UnityEngine.Object @object)
        {
            if (NAssetUtils.getPathInResources(@object, out var path))
            {
                _guid = NAssetUtils.getGUID(@object);
                _path = path.removeExtension();
            }
            else
            {
                _guid = string.Empty;
                _path = string.Empty;
            }
        }
#endif

        public Type getRefValueType()
        {
            return typeof(T);
        }
        public void unload()
        {
            if (_value.isNull()) return;
            Resources.UnloadAsset(_value);
            _value = null;
        }
    }
    public interface IRefInResources
    {
#if UNITY_EDITOR
        string getGuid();
        string getPath();
        void setValue(UnityEngine.Object @object);
#endif
        Type getRefValueType();
    }
}
