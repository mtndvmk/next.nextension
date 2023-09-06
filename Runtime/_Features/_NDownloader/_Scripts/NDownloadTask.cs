using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Nextension
{
    public class NDownloadTask : NProgressOperation
    {
        private const string TMP_DOWNLOAD_PATH_SUFFIX = ".tmp";
        internal static NDownloadTask createErrorTask(string url, string errorMsg)
        {
            var operation = new NDownloadTask(url, false);
            operation.innerFinalize(new Exception(errorMsg));
            return operation;
        }
        internal static NDownloadTask createLocalFileTask(string url, string filePath)
        {
            var operation = new NDownloadTask(url, true);
            operation._downloadedPath = Path.GetFullPath(filePath);
            operation.innerFinalize();
            return operation;
        }

        internal NDownloadTask(string url, bool isStoreDisk)
        {
            this.url = url;
            this.isStoreOnDisk = isStoreDisk;
        }
        internal void updateProgress()
        {
            if (_requestOperation == null)
            {
                return;
            }
            innerSetProgress(_requestOperation.progress);
        }

        public readonly string url;
        public readonly bool isStoreOnDisk;

        private byte[] _downloadedData;
        private string _downloadedPath;
        private bool _isStarted;

        private UnityWebRequestAsyncOperation _requestOperation;

        public string DownloadedPath
        {
            get
            {
                if (string.IsNullOrEmpty(_downloadedPath))
                {
                    throw new Exception($"DownloadedPath is null, isStoreOnDisk: {isStoreOnDisk}");
                }
                else
                {
                    return _downloadedPath;
                }
            }
        }
        public byte[] DownloadedData
        {
            get
            {
                if (IsFinalized)
                {
                    var err = $"Download task is not complete: Url={url}";
                    throw new Exception(err);
                }
                if (!IsError)
                {
                    var err = $"Download task is error: Url={url}, Error={Error}";
                    throw new Exception(err);
                }
                try
                {
                    if (_downloadedData == null)
                    {
                        _downloadedData = File.ReadAllBytes(DownloadedPath);
                    }
                    return _downloadedData;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to read data at {DownloadedPath}, Url={url}: " + e);
                    throw e;
                }
            }
        }

        internal void startDownloadAndSaveTo(string filePath)
        {
            if (_isStarted)
            {
                return;
            }
            _isStarted = true;
            var tmpPath = filePath + TMP_DOWNLOAD_PATH_SUFFIX;

            try
            {
                if (File.Exists(tmpPath))
                {
                    File.Delete(tmpPath);
                }

                var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
                if (isStoreOnDisk)
                {
                    _downloadedPath = Path.GetFullPath(filePath);
                    var downloadHandler = new DownloadHandlerFile(tmpPath);
                    downloadHandler.removeFileOnAbort = true;
                    request.downloadHandler = downloadHandler;
                }
                else
                {
                    var downloadHandler = new DownloadHandlerBuffer();
                    request.downloadHandler = downloadHandler;
                }
                _requestOperation = request.SendWebRequest();
                _requestOperation.completed += onDownloadCompleteCallback;
            }
            catch (Exception e)
            {
                innerFinalize(e);
            }
        }

        private void onDownloadCompleteCallback(AsyncOperation asyncOperation)
        {
            try
            {
                if (string.IsNullOrEmpty(_requestOperation.webRequest.error))
                {
                    if (isStoreOnDisk)
                    {
                        moveTmpPathToDownloadedPath();
                    }
                    else
                    {
                        _downloadedData = _requestOperation.webRequest.downloadHandler.data;
                    }
                    innerFinalize();
                }
                else
                {
                    innerFinalize(new Exception(_requestOperation.webRequest.error));
                }
            }
            catch (Exception e)
            {
                innerFinalize(e);
            }
            finally
            {
                dispose();
            }
        }
        private void moveTmpPathToDownloadedPath()
        {
            var tmpPath = DownloadedPath + TMP_DOWNLOAD_PATH_SUFFIX;
            if (File.Exists(DownloadedPath))
            {
                File.Delete(DownloadedPath);
            }
            File.Move(tmpPath, DownloadedPath);
        }
        private void dispose()
        {
            if (_requestOperation != null)
            {
                _requestOperation.webRequest.Dispose();
                _requestOperation = null;
            }
        }
    }
}