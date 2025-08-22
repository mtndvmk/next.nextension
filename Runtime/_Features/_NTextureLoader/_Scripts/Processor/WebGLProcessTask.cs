#if UNITY_WEBGL || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Nextension.TextureLoader
{
    internal class WebGLProcessTask : AbsProcessTask
    {
        [DllImport("__Internal")]
        private static extern void process(int id, string base64Str, string ext, int maxWidth, int maxHeight);
        [DllImport("__Internal")]
        private static extern void processWithSampleSize(int id, string base64Str, string ext, int inSampleSize);
        [DllImport("__Internal")]
        private static extern void processWithUrlAndSampleSize(int id, string url, int inSampleSize);
        [DllImport("__Internal")]
        private static extern void processWithUrl(int id, string url, int maxWidth, int maxHeight);
        [DllImport("__Internal")]
        private static extern void loadTextureAtNative(int id, string url, int inSampleSize, IntPtr texPtr);

        private static NTextureProcessWebGLCallback Callback
        {
            get
            {
                if (_callback == null)
                {
                    _callback = new GameObject().AddComponent<NTextureProcessWebGLCallback>();
                    _processIdCount = 0;
                }
                return _callback;
            }
        }
        private static NTextureProcessWebGLCallback _callback;
        private static int _processIdCount = 0;

        public readonly int id;
        public WebGLProcessTask() : base()
        {
            id = ++_processIdCount;
            Callback.addProcessing(this);
        }

        private string getExtension(byte[] inData)
        {
            return "png";
        }
        protected override Task exeProcess(byte[] imageData)
        {
            var extension = NTextureUtils.getImageExtension(imageData);
            setImageExtension(extension);
            if (!(extension == ImageExtension.JPG || extension == ImageExtension.PNG))
            {
                setError(new Exception($"Image extension is not supported: {extension}"));
                return Task.CompletedTask;
            }

            var base64Str = Convert.ToBase64String(imageData);
            var ext = getExtension(imageData);

            if (_setting.maxDimension.x > 0 && _setting.maxDimension.y > 0)
            {
                process(id, base64Str, ext, _setting.maxDimension.x, _setting.maxDimension.y);
            }
            else
            {
                processWithSampleSize(id, base64Str, ext, _setting.inSampleSize);
            }
            return Task.CompletedTask;
        }

        protected override async Task exeProcess(Uri uri)
        {
            if (uri.IsFile)
            {
                await exeProcess(await NUtils.getBinaryFrom(uri));
            }
            else
            {
                if (_setting.maxDimension.x > 0 && _setting.maxDimension.y > 0)
                {
                    processWithUrl(id, uri.AbsoluteUri, _setting.maxDimension.x, _setting.maxDimension.y);
                }
                else
                {
                    processWithUrlAndSampleSize(id, uri.AbsoluteUri, _setting.inSampleSize);
                }
            }
        }

        internal async void exeCompleteData(WebGLProcessCompleteData completeData)
        {
            try
            {
                if (string.IsNullOrEmpty(completeData.error))
                {
                    var tex = _setting.createTexture(completeData.outWidth, completeData.outHeight);
                    if (_setting.forceNoAlpha)
                    {
                        var nativeBytes = NConverter.convertToNativeArray<byte>(completeData.DataPtr, completeData.length, Allocator.TempJob);
                        var nativeBinary24 = await NTextureUtils.asyncConvertT32ToT24(nativeBytes);
                        tex.LoadRawTextureData(nativeBinary24);
                        nativeBytes.Dispose();
                        nativeBinary24.Dispose();
                    }
                    else
                    {
                        tex.LoadRawTextureData(completeData.DataPtr, completeData.length);
                    }
                    _setting.apply(tex);
                    setResult(tex, completeData.originWidth, completeData.originHeight);
                }
                else
                {
                    setError(new TextureProcessException(completeData.error));
                }
            }
            catch (Exception e)
            {
                setError(e);
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
                    _setting.apply(tex);

                    setResult(tex, completeData.originWidth, completeData.originHeight);
                }
                else
                {
                    setError(new TextureProcessException(completeData.error));
                }
            }
            catch (Exception e)
            {
                setError(e);
            }
        }
    }

    internal class NTextureProcessWebGLCallback : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            gameObject.name = "NTextureProcessWebGLCallback";
            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }

        internal Dictionary<int, WebGLProcessTask> _waitingProcessing = new Dictionary<int, WebGLProcessTask>();

        internal void addProcessing(WebGLProcessTask p)
        {
            _waitingProcessing[p.id] = p;
        }

        /// <summary>
        /// Called from web lib
        /// </summary>
        private void onProcessCompleteCallback(string jsonResult)
        {
            var result = JsonUtility.FromJson<WebGLProcessCompleteData>(jsonResult);

            if (result == null)
            {
                Debug.LogError("Has error when decode image WebGL");
                return;
            }

            if (_waitingProcessing.TryGetValue(result.id, out var p))
            {
                _waitingProcessing.Remove(result.id);
                p.exeCompleteData(result);
            }

            result.dispose();
        }

        /// <summary>
        /// Called from web lib
        /// </summary>
        private void loadTextureAtNativeCompleteCallback(string jsonResult)
        {
            var result = JsonUtility.FromJson<WebGLLoadIntoTextureCompleteData>(jsonResult);

            if (result == null)
            {
                Debug.LogError("Has error when load into texture");
                return;
            }

            if (_waitingProcessing.TryGetValue(result.id, out var p))
            {
                _waitingProcessing.Remove(result.id);
                p.exeCompleteLoadIntoTexture(result);
            }
        }
    }
    internal class WebGLProcessCompleteData
    {
        public int id;
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
        public int id;
        public string error;
    }
}
#endif