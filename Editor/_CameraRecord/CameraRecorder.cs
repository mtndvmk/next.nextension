using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using System.IO;

namespace Nextension.NEditor
{
    public class CameraRecorder : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private uint _fps = 30;
        [SerializeField] private Vector2Int _size = new(256, 256);
        [SerializeField] private string _savePath;
        [SerializeField] private string _prefixName = "frame";
        [SerializeField] private bool _saveInTimestampDirectory = false;
        [SerializeField] private int _frameCount = 30;

        [SerializeField] private UnityEvent onStartRecord;
        [SerializeField] private UnityEvent onEndRecord;
        [SerializeField] private bool _isRecording;

        [ContextMenu("Start record")]
        public async void startRecordCurrentAnimation()
        {
            if (_fps <= 0)
            {
                throw new Exception("FPS must greater than 0");
            }

            try
            {
                onStartRecord?.Invoke();

                var tmpTextureList = new List<Texture2D>();
                RenderTexture renderTexture = new(_size.x, _size.y, 32, RenderTextureFormat.ARGB32);
                _camera.targetTexture = renderTexture;

                float deltaTime = 1f / _fps;
                float currentTime = deltaTime;
                _isRecording = true;
                while (_isRecording)
                {
                    currentTime += deltaTime;
                    var tex2D = new Texture2D(_size.x, _size.y, TextureFormat.ARGB32, false);
                    Graphics.CopyTexture(renderTexture, tex2D);
                    tmpTextureList.Add(tex2D);
                    if (_frameCount > 0 && tmpTextureList.Count >= _frameCount)
                    {
                        break;
                    }
                    await new NWaitSecond(deltaTime);
                }

                _camera.targetTexture = null;
                DestroyImmediate(renderTexture);

                string newPath;
                if (_saveInTimestampDirectory)
                {
                    newPath = Path.Combine(_savePath, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
                }
                else
                {
                    newPath = _savePath;
                }
                if (Directory.Exists(newPath))
                {
                    Directory.Delete(newPath, true);
                }
                Directory.CreateDirectory(newPath);

                for (int i = 0; i < tmpTextureList.Count; i++)
                {
                    var png = tmpTextureList[i].EncodeToPNG();
                    var path = Path.Combine(newPath, $"{_prefixName}_{i}.png");
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    File.WriteAllBytes(path, png);
                    Object.DestroyImmediate(tmpTextureList[i]);
                }

                tmpTextureList.Clear();
                Debug.Log("Record complete: " + newPath);

                AssetDatabase.Refresh();
            }
            finally
            {
                onEndRecord?.Invoke();
                _isRecording = false;
            }
        }
    }
}
