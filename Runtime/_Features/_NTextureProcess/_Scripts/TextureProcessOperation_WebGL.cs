#if UNITY_WEBGL || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Nextension.TextureProcess
{
    public partial class TextureProcessOperation
    {
        internal class WebGL : AbsProcessOperation
        {
            private static NTextureProcessWebGL _processWebGL;
            public string Md5 { get; private set; }
            internal WebGL(string md5, TextureSetting setting) : base(setting)
            {
                Md5 = md5;
                if (!_processWebGL)
                {
                    _processWebGL = new GameObject().AddComponent<NTextureProcessWebGL>();
                }
                _processWebGL.addProcessing(this);
            }
            internal void exeCompleteData(WebGLProcessCompleteData completeData)
            {
                try
                {
                    if (string.IsNullOrEmpty(completeData.error))
                    {
                        var tex2D = _setting.createTexture(completeData.outWidth, completeData.outHeight);
                        tex2D.LoadRawTextureData(completeData.DataPtr, completeData.length);
                        tex2D.Apply(false, !_setting.isReadable);

                        Operation.Texture = tex2D;
                        Operation.OriginWidth = completeData.originWidth;
                        Operation.OriginHeight = completeData.originHeight;
                        Operation.innerSetComplete();
                    }
                    else
                    {
                        Operation.innerSetComplete(completeData.error);
                    }
                }
                catch (Exception e)
                {
                    Operation.innerSetComplete(e.Message);
                }
            }
            internal void exeCompleteLoadIntoTexture(WebGLLoadIntoTextureCompleteData completeData)
            {
                try
                {
                    if (string.IsNullOrEmpty(completeData.error))
                    {
                        var texPtr = new IntPtr(completeData.texPtr);
                        var tex = Texture2D.CreateExternalTexture(completeData.originWidth, completeData.originHeight, TextureFormat.RGBA32, false, true, texPtr);
                        Operation.Texture = tex;
                        Operation.OriginWidth = completeData.originWidth;
                        Operation.OriginHeight = completeData.originHeight;
                        Operation.innerSetComplete();
                    }
                    else
                    {
                        Operation.innerSetComplete(completeData.error);
                    }
                }
                catch (Exception e)
                {
                    Operation.innerSetComplete(e.Message);
                }
            }
            internal void exeCompleteLoadIntoTexture(Texture2D tex, int originWidth, int originHeight)
            {
                tex.Apply(false, !_setting.isReadable);
                Operation.Texture = tex;
                Operation.OriginWidth = originWidth;
                Operation.OriginHeight = originHeight;
                Operation.innerSetComplete();
            }
        }
    }
    internal class NTextureProcessWebGL : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            gameObject.name = "NTextureProcessWebGL";
            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }

        internal List<TextureProcessOperation.WebGL> _processingList = new List<TextureProcessOperation.WebGL>();

        internal void addProcessing(TextureProcessOperation.WebGL p)
        {
            if (!_processingList.Contains(p))
            {
                _processingList.Add(p);
            }
        }

        /// <summary>
        /// call from web lib
        /// </summary>
        private void decodeRGBACompleteCallback(string jsonResult)
        {
            var result = JsonUtility.FromJson<WebGLProcessCompleteData>(jsonResult);

            if (result == null)
            {
                Debug.LogError("Has error when decode image WebGL");
                return;
            }

            for (int i = _processingList.Count - 1; i >= 0; i--)
            {
                var p = _processingList[i];
                if (p.Md5 == result.md5)
                {
                    p.exeCompleteData(result);
                    _processingList.RemoveAt(i);
                }
            }

            result.dispose();
        }

        /// <summary>
        /// call from web lib
        /// </summary>
        private void loadTextureAtNativeCompleteCallback(string jsonResult)
        {
            var result = JsonUtility.FromJson<WebGLLoadIntoTextureCompleteData>(jsonResult);

            if (result == null)
            {
                Debug.LogError("Has error when load into texture");
                return;
            }

            for (int i = _processingList.Count - 1; i >= 0; i--)
            {
                var p = _processingList[i];
                if (p.Md5 == result.md5)
                {
                    _processingList.RemoveAt(i);
                    p.exeCompleteLoadIntoTexture(result);
                }
            }
        }
    }
    internal class WebGLProcessCompleteData
    {
        public string md5;
        public long resultPointer;
        public int length;
        public int originWidth;
        public int originHeight;
        public int outWidth;
        public int outHeight;
        public string error;

        private byte[] _resultData;
        public byte[] readResultData()
        {
            if (_resultData == null)
            {
                _resultData = new byte[length];
                Marshal.Copy(DataPtr, _resultData, 0, length);
            }
            return _resultData;
        }
        public IntPtr DataPtr => new IntPtr(resultPointer);
        public void dispose()
        {
            Marshal.FreeHGlobal(DataPtr);
        }
    }
    internal class WebGLLoadIntoTextureCompleteData
    {
        public int originWidth;
        public int originHeight;
        public int outWidth;
        public int outHeight;
        public int texPtr;
        public string md5;
        public string error;
    }
}
#endif