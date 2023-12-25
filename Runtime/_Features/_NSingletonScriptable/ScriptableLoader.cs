using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    public static class ScriptableLoader
    {
        private static SingletonScriptableContainer _container;
        private static bool _isLoaded = false;

        internal static class Getter<T> where T : ScriptableObject
        {
            internal static T scriptable;
            internal static int version;
            internal static void unload()
            {
                Resources.UnloadAsset(scriptable);
                scriptable = null;
                version = 0;
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void checkType<T>() where T : ScriptableObject
        {
            var typeOfT = typeof(T);
            if (typeOfT.IsAbstract || typeOfT.ContainsGenericParameters)
            {
                throw new Exception($"`{typeOfT}` must not be abstract or does not contain generic parameters");
            }
            if (typeOfT.GetCustomAttribute<SingletonScriptableAttribute>() == null)
            {
                throw new Exception($"`{typeOfT}` requires [{nameof(SingletonScriptableAttribute)}]");
            }
        }

        [StartupMethod]
        private static void startup()
        {
            getContainer();
        }

        private static void loadContainer()
        {
            _container = NAssetUtils.getObjectOnResources<SingletonScriptableContainer>(SingletonScriptableContainer.FileNameOnResource);
        }
        internal static SingletonScriptableContainer getContainer()
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                loadContainer();
            }
            return _container;
        }
        public static bool isSingletonable(Object @object)
        {
            if (@object is not ScriptableObject) return false;
            var type = @object.GetType();
            return type.GetCustomAttribute<SingletonScriptableAttribute>() != null && type.GetCustomAttribute<NonSingletonScriptableAttribute>() == null;
        }
        public static bool isSingletonable(Object @object, out SingletonScriptableAttribute attribute)
        {
            if (@object is not ScriptableObject)
            {
                attribute = null;
                return false;
            }
            var type = @object.GetType();
            attribute = type.GetCustomAttribute<SingletonScriptableAttribute>();
            if (attribute != null && type.GetCustomAttribute<NonSingletonScriptableAttribute>() == null)
            {
                return true;
            }
            attribute = null;
            return false;
        }
        public static T get<T>() where T : ScriptableObject
        {
            if (Getter<T>.scriptable) return Getter<T>.scriptable;

            if (!getContainer())
            {
                return null;
            }

            checkType<T>();

            if (Getter<T>.version != NStartRunner.SessionId || Getter<T>.scriptable == null)
            {
                Getter<T>.scriptable = _container.get<T>();
                Getter<T>.version = NStartRunner.SessionId;
#if UNITY_EDITOR
                addNonPreloadSingletonScriptable(Getter<T>.scriptable);
#endif
            }
            return Getter<T>.scriptable;
        }
        public static bool unload<T>(bool unloadUnusedAssets = true) where T : ScriptableObject
        {
            if (_container == null || Getter<T>.scriptable == null)
            {
                return false;
            }

            Getter<T>.unload();
            if (unloadUnusedAssets)
            {
                Resources.UnloadUnusedAssets();
            }
            return true;
        }

#if UNITY_EDITOR
        private static List<ScriptableObject> _loadedNonPreloadSingletonScriptables = new();
        [EditorQuittingMethod]
        public static void unloadNonPreloadScriptables()
        {
            if (_container == null)
            {
                return;
            }
            foreach (var item in _loadedNonPreloadSingletonScriptables)
            {
                if (!item)
                {
                    continue;
                }
                Resources.UnloadAsset(item);
            }
            _loadedNonPreloadSingletonScriptables.Clear();
            Resources.UnloadUnusedAssets();
        }
        private static SingletonScriptableContainer getOrCreateContainer()
        {
            if (!_container)
            {
                loadContainer();
            }
            if (!_container)
            {
                _container = NAssetUtils.createOnResource<SingletonScriptableContainer>(SingletonScriptableContainer.FileNameOnResource);
                NAssetUtils.refresh();
            }
            return _container;
        }
        private static void addNonPreloadSingletonScriptable(ScriptableObject scriptableObject)
        {
            if (_container.isNonPreloadSingletonScriptable(scriptableObject.GetType()))
            {
                _loadedNonPreloadSingletonScriptables.Add(scriptableObject);
            }
        }
        public static void updateScriptable(ScriptableObject scriptable)
        {
            getOrCreateContainer().updateScriptable(scriptable);
        }
        internal static bool contains(ScriptableObject scriptableObject)
        {
            var container = getContainer();
            if (container == null) return false;
            return container.contains(scriptableObject);
        }
#endif
    }
}