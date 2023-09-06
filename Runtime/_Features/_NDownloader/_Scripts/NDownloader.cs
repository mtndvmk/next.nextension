using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Nextension
{
    public class NDownloader
    {
        private const int MAX_DOWNLOAD_A_TIME = 5;
        private const int KEEP_CACHE_DAY = 21;
        private const int TOTAL_DAY = 366;

        private static List<NDownloadTask> _pendingTasks;
        private static List<NDownloadTask> _downloadingTasks;

        private static string _cacheRootPath;
        private static int _cachePeriodNumber;

        private static void requireFunc()
        {
            if (string.IsNullOrEmpty(_cacheRootPath))
            {
                try
                {
                    _cacheRootPath = Path.Combine(Application.persistentDataPath, "NDownloaderCache");
                    if (!Directory.Exists(_cacheRootPath))
                    {
                        Directory.CreateDirectory(_cacheRootPath);
                    }

                    _cachePeriodNumber = getCachePeriodNumber(DateTime.Now.DayOfYear, KEEP_CACHE_DAY);
                    cleanOldCache(_cachePeriodNumber);
                    Application.quitting += clearAllTasks;
                    NUpdater.onLateUpdateEvent.add(updateAllTask);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private static int getCachePeriodNumber(int dayInYear, int periodDay)
        {
            dayInYear--;
            var halfOfPeriod = periodDay / 2f;
            int totalDay = TOTAL_DAY;
            var period = halfOfPeriod / totalDay;
            var periodNumber = (int)(dayInYear / period / totalDay);
            if (totalDay - periodNumber * halfOfPeriod < halfOfPeriod)
            {
                periodNumber = 0;
            }
            return periodNumber;
        }
        private static int getMaxNumberOfCache(int periodDay)
        {
            int totalDay = TOTAL_DAY;
            return totalDay / periodDay * 2 - 1;
        }
        private static void cleanOldCache(int currentCacheNumber)
        {
            int maxNumber = getMaxNumberOfCache(KEEP_CACHE_DAY);

            var keepPath0 = currentCacheNumber;
            var keepPath1 = currentCacheNumber - 1;
            if (keepPath1 < 0)
            {
                keepPath1 = maxNumber;
            }
            for (int i = 0; i <= maxNumber; i++)
            {
                var oldPath = getCacheNumberPath(i);
                if (i == keepPath0 || i == keepPath1)
                {
                    continue;
                }
                if (Directory.Exists(oldPath))
                {
                    Directory.Delete(oldPath, true);
                }
            }
        }

        public static NDownloadTask requestDownload(string url, DownloadOption option = DownloadOption.None)
        {
            try
            {
                var uri = new Uri(url);
                url = uri.AbsoluteUri;
                if (uri.IsFile && File.Exists(uri.LocalPath))
                {
                    NDownloadTask localFileTask = NDownloadTask.createLocalFileTask(url, uri.LocalPath);
                    return localFileTask;
                }
            }
            catch (Exception ex)
            {
                return NDownloadTask.createErrorTask(url, ex.Message);
            }

            if ((option & DownloadOption.ForceNewDownload) == 0)
            {
                requireFunc();
                var path = getDataInCache(url, true);
                if (!string.IsNullOrEmpty(path))
                {
                    return NDownloadTask.createLocalFileTask(url, path);
                }
            }

            NDownloadTask task = null;

            if (_downloadingTasks != null)
            {
                task = _downloadingTasks.Find(item => item.url.Equals(url));
            }

            if (task == null && _pendingTasks != null)
            {
                task = _pendingTasks.Find(item => item.url.Equals(url));
            }

            if (task == null)
            {
                requireFunc();
                task = new NDownloadTask(url, (option & DownloadOption.NotStoreOnDisk) == 0);
                if (_pendingTasks == null)
                {
                    _pendingTasks = new(1);
                }
                _pendingTasks.Add(task);
            }

            return task;
        }
        /// <summary>
        /// return null if data are not exist in cache, else return path of data
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string getDataInCache(string url, bool isMoveToNewCache)
        {
            var path0 = generatePath(url, _cachePeriodNumber);
            if (File.Exists(path0))
            {
                return path0;
            }
            var path1 = generatePath(url, _cachePeriodNumber - 1);
            if (File.Exists(path1))
            {
                if (isMoveToNewCache)
                {
                    moveToNewestCache(url);
                    return path0;
                }
                return path1;
            }
            return null;
        }
        public static bool hasDataInCache(string url)
        {
            return !string.IsNullOrEmpty(getDataInCache(url, false));
        }
        private static void moveToNewestCache(string url)
        {
            var path0 = generatePath(url, _cachePeriodNumber);
            if (File.Exists(path0))
            {
                Debug.LogWarning("Exists in cache: " + path0);
                return;
            }
            var path1 = generatePath(url, _cachePeriodNumber - 1);
            if (!File.Exists(path1))
            {
                Debug.LogWarning("Not found in cache: " + path1);
            }
            File.Move(path1, path0);
        }
        public static void deleteInCache(string url)
        {
            var path = getDataInCache(url, false);
            if (!string.IsNullOrEmpty(path))
            {
                File.Delete(path);
            }
        }
        public static void cleanAllCache()
        {
            if (Directory.Exists(_cacheRootPath))
            {
                Directory.Delete(_cacheRootPath, true);
            }
        }

        private static void updateAllTask()
        {
            checkDownloadingTask();
            checkPendingTask();
        }
        private static void checkDownloadingTask()
        {
            if (_downloadingTasks == null || _downloadingTasks.Count == 0)
            {
                return;
            }

            for (int i = _downloadingTasks.Count - 1; i >= 0; --i)
            {
                var task = _downloadingTasks[i];
                task.updateProgress();
                if (task.IsFinalized)
                {
                    _downloadingTasks.RemoveAt(i);
                }
            }
        }
        private static void checkPendingTask()
        {
            if (_pendingTasks == null || _pendingTasks.Count == 0)
            {
                return;
            }

            while (_downloadingTasks == null || _downloadingTasks.Count < MAX_DOWNLOAD_A_TIME)
            {
                var task = takeAndRemoveNext();
                startDownload(task);
                if (_pendingTasks.Count == 0) 
                {
                    break;
                }
            }
        }
        private static NDownloadTask takeAndRemoveNext()
        {
            if (_pendingTasks.Count > 0)
            {
                return _pendingTasks.takeAndRemoveAt(0);
            }
            return null;
        }
        private static void startDownload(NDownloadTask task)
        {
            var path = generatePath(task.url, _cachePeriodNumber);
            task.startDownloadAndSaveTo(path);
            if (_downloadingTasks == null)
            {
                _downloadingTasks = new List<NDownloadTask>(1);
            }
            _downloadingTasks.Add(task);
        }
        private static string getCacheNumberPath(int cacheNumber)
        {
            return Path.Combine(_cacheRootPath, cacheNumber.ToString());
        }
        private static string generatePath(string url, int cacheNumber)
        {
            var ext = Path.GetExtension(url);
            var fileName = NUtils.computeMD5(url);
            if (!string.IsNullOrEmpty(ext))
            {
                fileName += ext;
            }
            var cachePath = getCacheNumberPath(cacheNumber);
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }
            return Path.Combine(cachePath, fileName);
        }
        private static void clearAllTasks()
        {
            _pendingTasks?.Clear();
            _downloadingTasks?.Clear();
        }
    }
}