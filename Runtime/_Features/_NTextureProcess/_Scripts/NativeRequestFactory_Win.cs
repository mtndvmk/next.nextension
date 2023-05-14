#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && NET_4_6
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Nextension.TextureProcess
{
    internal partial class NativeRequestFactory
    {
        internal class Win : INativeRequest
        {
            public TextureProcessOperation decodeRGBA(byte[] imageData, TextureSetting setting)
            {
                TextureProcessOperation.Win winOperation = new TextureProcessOperation.Win(setting);
                winOperation.startDecode(imageData);
                return winOperation.Operation;
            }

            public TextureProcessOperation decodeRGBA(string localPath, TextureSetting setting)
            {
                TextureProcessOperation.Win winOperation = new TextureProcessOperation.Win(setting);
                winOperation.startDecode(localPath);
                return winOperation.Operation;
            }

            public TextureProcessOperation loadTextureAtNative(string localPath, TextureSetting setting)
            {
                TextureProcessOperation.Win winOperation = new TextureProcessOperation.Win(setting);
                winOperation.startDecode(localPath);
                return winOperation.Operation;
            }
        }
    }
}
#endif