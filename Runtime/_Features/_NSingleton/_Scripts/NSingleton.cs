using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nextension
{
    public abstract class NSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region Virtual methods
        protected virtual void onInitialized()
        {

        }
        protected virtual void onAwake()
        {

        }
        protected virtual void onDestroy()
        {

        }
        protected virtual void onQuitApp()
        {

        }
        #endregion

        #region SerializeField
        [SerializeField] protected bool m_DontDestroyOnLoad = false;
        [SerializeField] protected int m_Priority = 1;
        #endregion

        #region Unity methods
        private void Awake()
        {
            if (IsDestroyedAndUnused)
            {
                Destroy(gameObject);
            }
            else
            {
                if (!IsInitialized)
                {
                    if (s_Instance.isNull())
                    {
                        localInitialize();
                    }
                    else
                    {
                        if (s_Instance != this)
                        {
                            if (s_Instance.GetComponent<NSingleton<T>>().m_Priority < m_Priority)
                            {
                                Destroy(s_Instance.gameObject);
                                localInitialize();
                            }
                            else
                            {
                                Destroy(gameObject);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (s_Instance != this)
                    {
                        if (s_Instance.GetComponent<NSingleton<T>>().m_Priority < m_Priority)
                        {
                            Destroy(s_Instance.gameObject);
                            localInitialize();
                        }
                        else
                        {
                            Destroy(gameObject);
                            Debug.Log("Destroyed " + typeof(T).Name + ", IsInitialized: " + IsInitialized);
                            return;
                        }
                    }
                }
            }
            if (s_Instance == this)
            {
                onAwake();
            }
        }
        private void OnDestroy()
        {
            if (s_Instance == this)
            {
                if (IsPlaying)
                {
                    if (!_isDestroyedFromInternal)
                    {
                        if (!m_DontDestroyOnLoad)
                        {
                            int sceneHash = gameObject.scene.GetHashCode();
                            new NWaitFrame(1).startWaitable().addCompletedEvent(() =>
                            {
                                int sceneCount = SceneManager.sceneCount;
                                for (int i = 0; i < sceneCount; i++)
                                {
                                    if (SceneManager.GetSceneAt(i).GetHashCode() == sceneHash)
                                    {
                                        Debug.LogWarning($"[NSingleton] Should call {getSingletonName()}.Instance.destroy()");
                                        return;
                                    }
                                }
                            });
                        }
                        else
                        {
                            Debug.LogWarning($"[NSingleton] Should call {getSingletonName()}.Instance.destroy()");
                        }
                    }
                    else
                    {
                        Debug.Log($"{Instance.gameObject.name} was destroyed");
                    }
                    onDestroy();
                }
                else
                {
                    onDestroy();
                    onQuitApp();
                }
            }
        }
        #endregion

        private bool _isDestroyedFromInternal;
        private bool _isLocallyInitialized;
        private void localInitialize()
        {
            if (IsDestroyedAndUnused)
            {
                Destroy(gameObject);
            }
            else if (IsPlaying)
            {
                if (!_isLocallyInitialized)
                {
                    _isLocallyInitialized = true;
                    s_Instance = GetComponent<T>();
                    try
                    {
                        onInitialized();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    if (m_DontDestroyOnLoad)
                    {
                        gameObject.transform.SetParent(null);
                        DontDestroyOnLoad(gameObject);
                    }
                }
            }
        }

        private static string getSingletonName()
        {
            return typeof(T).Name;
        }
        private static T s_Instance;

        public void destroy(bool markUnused = true)
        {
            _isDestroyedFromInternal = true;
            IsDestroyedAndUnused |= markUnused;
            Destroy(gameObject);
        }

        public static T Instance
        {
            get
            {
                if (s_Instance.isNull())
                {
                    try
                    {
                        if (!IsPlaying)
                        {
                            var instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
                            if (!instance) throw new Exception($"Can't create instance of {typeof(T)} while application isn't playing");
                            return instance;
                        }
                        if (IsDestroyedAndUnused)
                        {
                            throw new Exception("NSingleton has been destroyed and marked as unused");
                        }
                        initialize();
                    }
                    catch (Exception e)
                    {
                        if (e.Message == "Application isn't playing")
                        {
                            Debug.LogWarning($"Failed to initialize NSingleton<{typeof(T)}>: " + e.Message);
                        }
                        else
                        {
                            Debug.LogError($"Failed to initialize NSingleton<{typeof(T)}>: " + e.Message);
                        }
                    }
                }
                return s_Instance;
            }
        }

        public static bool isExist()
        {
            return isExist(out _);
        }
        public static bool isExist(out T instance)
        {
            instance = s_Instance;
            if (instance.isNull())
            {
                instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
                return !instance.isNull();
            }
            return false;
        }

        public static bool IsPlaying => NStartRunner.IsPlaying;
        public static bool IsDestroyedAndUnused { get; protected set; }
        public static bool IsInitialized => s_Instance != null;
        public static void initialize(bool? isDontDestroyOnLoad = default)
        {
            if (IsPlaying && !IsDestroyedAndUnused)
            {
                if (s_Instance.isNull())
                {
                    s_Instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
                    NSingleton<T> singleton;
                    if (s_Instance.isNull())
                    {
                        var go = new GameObject("[Generated] " + getSingletonName());
                        s_Instance = go.AddComponent<T>();
                        singleton = s_Instance.GetComponent<NSingleton<T>>();
                        singleton.m_Priority = 0;
                    }
                    else
                    {
                        singleton = s_Instance.GetComponent<NSingleton<T>>();
                    }
                    if (!singleton._isLocallyInitialized)
                    {
                        singleton.localInitialize();
                    }
                    if (isDontDestroyOnLoad.HasValue && isDontDestroyOnLoad.Value)
                    {
                        singleton.gameObject.transform.SetParent(null);
                        DontDestroyOnLoad(singleton.gameObject);
                    }
                }
            }
        }
    }

    public interface ISingletonable : IOnCompiled
    {
        bool isSingleton() => true;
        static new void onPreprocessBuild()
        {
            if (checkHasErrorOnBuild(out var e))
            {
                throw new KeepStackTraceException(e);
            }
        }
        static new void onLoadOrRecompiled()
        {
            if (checkHasErrorOnBuild(out var e))
            {
                throw new KeepStackTraceException(e);
            }
        }
        private static bool checkHasErrorOnBuild(out Exception exception)
        {
            try
            {
                var nSingletonType = typeof(NSingleton<>);
                var types = NUtils.getCustomTypes();

                foreach (var type in types)
                {
                    if (type.IsGenericTypeDefinition) continue;
                    var baseType = type.BaseType;
                    if (baseType == null) continue;
                    if (baseType.FullName.StartsWith(nSingletonType.FullName))
                    {
                        var awakeMethod = type.GetTypeInfo().GetDeclaredMethod("Awake");
                        if (awakeMethod != null)
                        {
                            throw new Exception($"Please override \"onAwake()\" instead using \"Awake()\" at [{type.FullName}]");
                        }

                        var onDestroyMethod = type.GetTypeInfo().GetMethod("OnDestroy");
                        if (onDestroyMethod != null)
                        {
                            throw new Exception("Please override onDestroy() instead using OnDestroy()");
                        }
                    }
                }

                exception = null;
                return false;
            }
            catch (Exception e)
            {
                exception = e;
                return true;
            }
        }
    }

    public static class S_<T> where T : class, ISingletonable
    {
        private static T s_Instance;
        public static T Instance => get();
        public static T get()
        {
            if (s_Instance.isNull())
            {
                var tType = typeof(T);
                if (NUtils.isInherited(tType, typeof(MonoBehaviour)))
                {
                    var allObjects = UnityEngine.Object.FindObjectsByType(tType, FindObjectsInactive.Include, FindObjectsSortMode.None);
                    foreach (var obj in allObjects)
                    {
                        var ins = obj as T;
                        if (ins.isSingleton())
                        {
                            s_Instance = ins;
                            return s_Instance;
                        }
                    }
                    throw new Exception($"Can't found instance of [{tType}]");
                }
                else
                {
                    s_Instance = NUtils.createInstance<T>(tType);
                }
            }
            return s_Instance;
        }
        public static void dispose()
        {
            if (s_Instance != null)
            {
                if (NUtils.isInherited(typeof(T), typeof(MonoBehaviour)))
                {
                    NUtils.destroyObject(s_Instance as MonoBehaviour);
                }
                s_Instance = null;
            }
        }
    }
}