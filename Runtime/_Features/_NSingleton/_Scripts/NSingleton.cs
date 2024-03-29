using System;
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
                    if (s_Instance == null)
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
                    onInitialized();
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
                if (s_Instance == null)
                {
                    try
                    {
                        if (!IsPlaying)
                        {
                            throw new Exception("Application isn't playing");
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
        public static bool IsPlaying => NStartRunner.IsPlaying;
        public static bool IsDestroyedAndUnused { get; protected set; }
        public static bool IsInitialized => s_Instance != null;
        public static void initialize()
        {
            if (IsPlaying && !IsDestroyedAndUnused)
            {
                if (s_Instance == null)
                {
                    s_Instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
                    NSingleton<T> singleton;
                    if (s_Instance == null)
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
                }
            }
        }
    }
}
