using System;
using UnityEngine;

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
        [SerializeField] protected bool m_DontDestroyOnLoad = true;
        [SerializeField] protected int m_Priority = 1;
        #endregion

        #region Unity methods
        private void Awake()
        {
            if (IsDestroyAndUnuse)
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
                            Debug.Log("---> Destroyed " + typeof(T).Name + "|" + IsInitialized);
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
                    if (!m_IsDestroyFromInternal)
                    {
                        Debug.LogWarning($"[SKIP] [NSingleton] Should call {getSingletonName()}.Instance.destroy()");
                    }
                    Debug.Log($"[SKIP] {Instance.gameObject.name} was destroyed");
                    onDestroy();
                }
                else
                {
                    onQuitApp();
                }
            }
        }
        #endregion

        private bool m_IsDestroyFromInternal;
        private bool m_IsLocalInitialized;
        private void localInitialize()
        {
            if (IsDestroyAndUnuse)
            {
                Destroy(gameObject);
            }
            else if (IsPlaying)
            {
                if (!m_IsLocalInitialized)
                {
                    m_IsLocalInitialized = true;
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

        public void destroy(bool markUnuse = true)
        {
            m_IsDestroyFromInternal = true;
            IsDestroyAndUnuse |= markUnuse;
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
                            throw new Exception("Application is quitting");
                        }
                        if (IsDestroyAndUnuse)
                        {
                            throw new Exception("NSingleton has been destroyd and marked as unused");
                        }
                        initialize();
                    }
                    catch (Exception e)
                    {
                        if (e.Message == "Application is quitting")
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
        public static bool IsDestroyAndUnuse { get; protected set; }
        public static bool IsInitialized => s_Instance != null;
        public static void initialize()
        {
            if (IsPlaying && !IsDestroyAndUnuse)
            {
                if (s_Instance == null)
                {
                    s_Instance = FindObjectOfType<T>(true);
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
                    if (!singleton.m_IsLocalInitialized)
                    {
                        singleton.localInitialize();
                    }
                }
            }
        }
    }
}
