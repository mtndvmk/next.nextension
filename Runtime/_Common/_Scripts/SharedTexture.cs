using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public static class SharedTextureManager
    {
#if UNITY_EDITOR
        static SharedTextureManager()
        {
            NUpdater.onUpdateEvent.add(checkOrigins);
        }
        [EditorQuittingMethod]
        static void reset()
        {
            origins.Clear();
        }
        private static void checkOrigins()
        {
            foreach (var origin in origins)
            {
                if (origin.texture == null && !origin.IsDisposed)
                {
                    Debug.LogWarning("Texture of OriginTexture has been destroy without dispose?");
                }
            }
        }
#endif
        internal static HashSet<OriginTexture> origins = new();

        public static void disposeAllOrigins()
        {
            foreach (var origin in origins)
            {
                origin.dispose();
            }
            origins.Clear();
        }
    }
    public class OriginTexture
    {
        internal Texture texture;
        private HashSet<SharedTexture> _sharedList = new();

        internal OriginTexture(Texture texture)
        {
            this.texture = texture;
            SharedTextureManager.origins.Add(this);
        }
        internal void dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            IsDisposed = true;
            NUtils.destroy(texture);
            texture = null;
            _sharedList.Clear();
            SharedTextureManager.origins.Remove(this);
        }

        public bool IsDisposed { get; private set; }
        public SharedTexture createSharedTexture()
        {
            if (IsDisposed)
            {
                throw new System.Exception("OriginTexture has been disposed");
            }
            var sharedTexture = new SharedTexture(this);
            _sharedList.Add(sharedTexture);
            return sharedTexture;
        }
        public void release(SharedTexture sharedTexture, bool isDisposeIfNoLongerUsed = true)
        {
            if (IsDisposed)
            {
                Debug.LogWarning("OriginTexture has been disposed");
                return;
            }
            if (_sharedList.Remove(sharedTexture))
            {
                if (isDisposeIfNoLongerUsed && _sharedList.Count == 0)
                {
                    dispose();
                }
            }
        }
    }
    public class SharedTexture
    {
        internal SharedTexture(OriginTexture origin)
        {
            this.origin = origin;
        }
        internal OriginTexture origin;

        public bool IsReleased => origin == null;
        public Texture Texture
        {
            get
            {
                if (IsReleased)
                {
                    throw new System.Exception("SharedTexture has been released");
                }
                if (origin.IsDisposed)
                {
                    throw new System.Exception("OriginTexture has been disposed");
                }
                return origin.texture;
            }
        }

        public void release(bool isDisposeIfNoLongerUsed)
        {
            if (IsReleased)
            {
                Debug.LogWarning("SharedTexture has been released");
                return;
            }
            if (origin.IsDisposed)
            {
                Debug.LogWarning("OriginTexture has been disposed");
                origin = null;
                return;
            }
            origin.release(this, isDisposeIfNoLongerUsed);
            origin = null;
        }
        public SharedTexture clone()
        {
            if (IsReleased)
            {
                throw new System.Exception("SharedTexture has been released");
            }
            if (origin.IsDisposed)
            {
                throw new System.Exception("OriginTexture has been disposed");
            }
            return origin.createSharedTexture();
        }
    }
}
