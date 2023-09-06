using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public static class SharedTextureManager
    {
        [StartupMethod]
        private static void reset()
        {
            SHARED_TEXTURE_COUNTER = 0;
            origins = new HashSet<OriginTexture>();
#if UNITY_EDITOR
            NUpdater.onUpdateEvent.add(checkOrigins);
#endif
        }
        internal static uint SHARED_TEXTURE_COUNTER;
        internal static HashSet<OriginTexture> origins;

#if UNITY_EDITOR
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
        private NBList<SharedTexture, uint> _sharedList = new((item) => item.id);

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
            NUtils.destroy(texture);
            texture = null;
            IsDisposed = true;
            _sharedList.clear();
        }

        public bool IsDisposed {get; private set;}
        public SharedTexture createSharedTexture()
        {
            if (IsDisposed)
            {
                throw new System.Exception("OriginTexture has been disposed");
            }
            var sharedTexture = new SharedTexture(++SharedTextureManager.SHARED_TEXTURE_COUNTER, this);
            _sharedList.addAndSort(sharedTexture);
            return sharedTexture;
        }
        public void release(SharedTexture sharedTexture, bool isDisposeIfNoLongerUsed = true)
        {
            if (IsDisposed)
            {
                return;
            }
            if (_sharedList.remove(sharedTexture))
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
        internal SharedTexture(uint id, OriginTexture origin)
        {
            this.id = id;
            this.origin = origin;
        }
        internal OriginTexture origin;

        public readonly uint id;
        public bool IsReleased => origin == null;
        public Texture Texture
        {
            get
            {
                if (IsReleased)
                {
                    throw new System.Exception("OriginTexture has been released");
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
                return;
            }
            origin.release(this, isDisposeIfNoLongerUsed);
            origin = null;
        }
        public SharedTexture clone()
        {
            if (IsReleased)
            {
                throw new System.Exception("OriginTexture has been released");
            }
            if (origin.IsDisposed)
            {
                throw new System.Exception("OriginTexture has been disposed");
            }
            return origin.createSharedTexture();
        }
    }
}
