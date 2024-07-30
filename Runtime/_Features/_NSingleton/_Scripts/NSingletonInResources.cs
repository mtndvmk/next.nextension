using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Nextension
{
    public abstract class NSingletonInResources<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T s_instance;
        private static NWaitable s_initializeTask;
        public static T Instance
        {
            get
            {
                initialize();
                return s_instance;
            }
        }

        public static void destroyAndUnload(bool isImmediate = false, bool unloadUnusedAssets = true)
        {
            if (s_instance.isNull()) return;
            NUtils.destroy(s_instance.gameObject, isImmediate);
            (s_instance as NSingletonInResources<T>).onDestroy();
            if (unloadUnusedAssets) Resources.UnloadUnusedAssets();
            s_instance = null;
        }
        public static void initialize()
        {
            if (!s_instance.isNull())
            {
                return;
            }

            if (s_initializeTask != null)
            {
                s_initializeTask.cancel();
                s_initializeTask = null;
            }

            instantiate(Resources.Load<T>(getPath()));
        }
        public static async Task initializeAsync()
        {
            if (s_instance.isNull())
            {
                await (s_initializeTask ??= innerInitializeAsync());
            }
        }
        private static async NWaitable innerInitializeAsync()
        {
            var prefabPath = getPath();
            var requestOperation = Resources.LoadAsync<T>(prefabPath);
            await requestOperation;

            if (s_instance.isNull())
            {
                instantiate(requestOperation.asset as T);
                
            }
            s_initializeTask = null;
        }
        private static string getPath()
        {
            var typeOfT = typeof(T);
            if (typeOfT.GetCustomAttribute(typeof(PathInResourcesAttribute)) is PathInResourcesAttribute getPrefabPathAttr)
            {
                return getPrefabPathAttr.path;
            }
            else
            {
                return typeOfT.Name;
            }
        }
        private static void instantiate(T prefab)
        {
            if (prefab == null) throw new Exception($"Can't find prefab of [{typeof(T)}], resources path: {getPath()}");
            s_instance = Instantiate(prefab);
            var singleton = s_instance as NSingletonInResources<T>;
            if (singleton._isDontDestroyOnLoad)
            {
                DontDestroyOnLoad(singleton);
            }
            singleton.onInitialized();
        }

        [SerializeField] private bool _isDontDestroyOnLoad;

        protected virtual void onInitialized() { }
        protected virtual void onDestroy() { }
    }
}
