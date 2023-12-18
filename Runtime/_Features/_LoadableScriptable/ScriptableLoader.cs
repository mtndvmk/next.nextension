using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension
{
    public static class ScriptableLoader
    {
        private static LoadableScriptableContainer _container;
        private static bool _isLoaded = false;
        private static Dictionary<Type, Action> _unloadActions;

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
            if (typeOfT.GetCustomAttribute<LoadableScriptableAttribute>() == null)
            {
                throw new Exception($"`{typeOfT}` requires [{nameof(LoadableScriptableAttribute)}]");
            }
        }

        [StartupMethod]
        private static void startup()
        {
            getContainer();
        }

        private static void loadContainer()
        {
            _container = NAssetUtils.getObjectOnResources<LoadableScriptableContainer>(LoadableScriptableContainer.FileNameOnResource);
        }
        internal static LoadableScriptableContainer getContainer()
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                loadContainer();
            }
            return _container;
        }
        internal static bool contains(ScriptableObject scriptableObject)
        {
            var container = getContainer();
            if (container == null) return false;
            return container.contains(scriptableObject);
        }

        public static bool isLoadable(Object @object)
        {
            if (@object is not ScriptableObject) return false;
            var type = @object.GetType();
            return type.GetCustomAttribute<LoadableScriptableAttribute>() != null && type.GetCustomAttribute<NonLoadableScriptableAttribute>() == null;
        }
        public static bool isLoadable(Object @object, out LoadableScriptableAttribute attribute)
        {
            if (@object is not ScriptableObject)
            {
                attribute = null;
                return false;
            }
            var type = @object.GetType();
            attribute = type.GetCustomAttribute<LoadableScriptableAttribute>();
            if (attribute != null && type.GetCustomAttribute<NonLoadableScriptableAttribute>() == null)
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

            if (Getter<T>.version != NStartRunner.SessionId || !Getter<T>.scriptable)
            {
                Getter<T>.scriptable ??= _container.get<T>();
                Getter<T>.version = NStartRunner.SessionId;
                (_unloadActions ??= new Dictionary<Type, Action>(1))[typeof(T)] = Getter<T>.unload;
            }
            return Getter<T>.scriptable;
        }
        public static bool unload<T>() where T : ScriptableObject
        {
            if (_container == null || Getter<T>.scriptable == null)
            {
                return false;
            }

            var type = typeof(T);
            var loadedNonPreload = _container.getLoadedNonPreloadScriptables();
            if (loadedNonPreload != null && loadedNonPreload.ContainsKey(type))
            {
                _unloadActions?.Remove(type);
                loadedNonPreload.Remove(type);
                Getter<T>.unload();
                Resources.UnloadUnusedAssets();
                return true;
            }

            return false;
        }
        [EditorQuittingMethod]
        public static void unloadNonPreloadScriptables()
        {
            if (_container == null)
            {
                return;
            }
            var loadedNonPreload = _container.getLoadedNonPreloadScriptables();
            if (loadedNonPreload != null)
            {
                foreach (var item in loadedNonPreload)
                {
                    if (!item.Value)
                    {
                        continue;
                    }
                    if (_unloadActions != null && _unloadActions.ContainsKey(item.Key))
                    {
                        _unloadActions[item.Key].Invoke();
                        _unloadActions.Remove(item.Key);
                    }
                    else
                    {
                        Resources.UnloadAsset(item.Value);
                    }
                }
                _container.clearLoadedNonPreloadScriptableDictionary();
                Resources.UnloadUnusedAssets();
            }
        }

#if UNITY_EDITOR
        private static LoadableScriptableContainer getOrCreateContainer()
        {
            if (!_container)
            {
                loadContainer();
            }
            if (!_container)
            {
                _container = NAssetUtils.createOnResource<LoadableScriptableContainer>(LoadableScriptableContainer.FileNameOnResource);
            }
            return _container;
        }
        public static void updateContainer(ScriptableObject scriptable)
        {
            getOrCreateContainer().updateScriptable(scriptable);
        }
#endif
    }
}