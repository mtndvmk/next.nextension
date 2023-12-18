using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nextension.NEditor
{
    public class CameraAnimatorRecorder : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Animator _animator;
        [SerializeField] private uint _fps = 30;
        [SerializeField] private Vector2Int _size = new(256, 256);
        [SerializeField] private string _savePath;
        [SerializeField] private string _prefixName = "frame";
        [SerializeField] private bool _saveInTimestampDirectory = false;

        [ContextMenu("Record current animator state")]
        public void startRecordCurrentAnimation()
        {
            if (_fps <= 0)
            {
                throw new Exception("FPS must greater than 0");
            }

            var defaultAnimatorController = _animator.runtimeAnimatorController;

            if (defaultAnimatorController == null)
            {
                throw new Exception("AnimatorController is null");
            }

            var overrideAnimatorController = new AnimatorOverrideController(defaultAnimatorController);
            _animator.runtimeAnimatorController = overrideAnimatorController;

            _animator.enabled = false;

            try
            {
                float maxDuration = 0;
                for (int i = 0; i < _animator.layerCount; i++)
                {
                    var state = _animator.GetCurrentAnimatorStateInfo(i);
                    maxDuration = Mathf.Max(maxDuration, state.length);
                    _animator.Play(state.fullPathHash, i, 0);
                }

                if (maxDuration == 0)
                {
                    throw new Exception("Current state has length is 0");
                }

                var tmpTextureList = new List<Texture2D>();
                RenderTexture renderTexture = new(_size.x, _size.y, 16, RenderTextureFormat.ARGB32);
                _camera.targetTexture = renderTexture;
                var tmpActiveTexture = RenderTexture.active;
                RenderTexture.active = renderTexture;

                var skinMeshes = _animator.GetComponentsInChildren<SkinnedMeshRenderer>();

                _animator.Update(0);
                foreach (var skinMesh in skinMeshes)
                {
                    skinMesh.forceMatrixRecalculationPerRender = true;
                }

                _camera.Render();
                var tex2D = new Texture2D(_size.x, _size.y, TextureFormat.ARGB32, false);
                tex2D.ReadPixels(new Rect(0, 0, _size.x, _size.y), 0, 0);
                tex2D.Apply();
                tmpTextureList.Add(tex2D);
                RenderTexture.active = tmpActiveTexture;

                float deltaTime = 1f / _fps;
                float currentTime = deltaTime;
                while (currentTime < maxDuration)
                {
                    currentTime += deltaTime;
                    _animator.Update(deltaTime);
                    _camera.Render();
                    RenderTexture.active = renderTexture;
                    tex2D = new Texture2D(_size.x, _size.y, TextureFormat.ARGB32, false);
                    tex2D.ReadPixels(new Rect(0, 0, _size.x, _size.y), 0, 0);
                    tex2D.Apply();
                    RenderTexture.active = tmpActiveTexture;
                    tmpTextureList.Add(tex2D);
                }

                for (int i = 0; i < _animator.layerCount; i++)
                {
                    var state = _animator.GetCurrentAnimatorStateInfo(i);
                    _animator.Play(state.fullPathHash, i, 0);
                }
                _animator.Update(0);

                RenderTexture.active = tmpActiveTexture;
                _camera.targetTexture = null;
                Object.DestroyImmediate(renderTexture);

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
                _animator.runtimeAnimatorController = defaultAnimatorController;
            }
        }
    }
}
