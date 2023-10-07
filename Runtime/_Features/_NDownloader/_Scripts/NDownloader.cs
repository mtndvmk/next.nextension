using System;
using System.IO;
using UnityEngine;

namespace Nextension
{
    public class NDownloader
    {
        private const int KEEP_CACHE_DAY = 30;
        private const int TOTAL_DAY = 366;

        protected static NScheduler<NDownloadTask> _scheduler;

        private static string _cacheRootPath;
        private static int _cachePeriodNumber;

        public static int MaximumNumberOfTasksAtOnce
        {
            get => _scheduler?.MaxSchedulableAtOnce ?? 0;
            set
            {
                if (_scheduler == null)
                {
                    _scheduler = new(value);
                }
                else
                {
                    _scheduler.MaxSchedulableAtOnce = value;
                }
            }
        }

        private static void requireFunc()
        {
            _scheduler ??= new();
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
                    NUpdater.onLateUpdateEvent.add(updateDownloadingProgress);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        private static void updateDownloadingProgress()
        {
            var executingTasks = _scheduler.Executing;
            if (executingTasks.Count == 0)
            {
                return;
            }
            foreach (var task in executingTasks)
            {
                task.updateProgress();
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

        public static NDownloadTask requestDownload(string url, int priority = 0, DownloadOption option = DownloadOption.None)
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

            if ((option & DownloadOption.NotLoadOnDisk) == 0)
            {
                requireFunc();
                var path = getDataInCache(url, true);
                if (!string.IsNullOrEmpty(path))
                {
                    return NDownloadTask.createLocalFileTask(url, path);
                }
            }


            if (_scheduler != null)
            {
                var executingTasks = _scheduler.Executing;
                foreach (var task in executingTasks)
                {
                    if (task.url.Equals(url))
                    {
                        return task;
                    }
                }

                var pendingTasks = _scheduler.Pending;
                var existTaskIndex = pendingTasks.findIndex(item => item.url.Equals(url));
                if (existTaskIndex >= 0)
                {
                    var task = pendingTasks[existTaskIndex];
                    if (task.priority < priority)
                    {
                        pendingTasks.removeAt(existTaskIndex);
                        task.priority = priority;
                        pendingTasks.addAndSort(task);
                    }
                    return task;
                }
            }
            {
                NDownloadTask task;
                requireFunc();
                if ((option & DownloadOption.NotStoreOnDisk) == 0)
                {
                    var path = generatePath(url, _cachePeriodNumber);
                    task = new NDownloadTask(url, true, path);
                }
                else
                {
                    task = new NDownloadTask(url, false, null);
                }

                task.priority = priority;
                _scheduler.schedule(task);
                return task;
            }
        }
        /// <summary>
        /// return null if data are not exist in cache, else return path of data
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string getDataInCache(string url, bool isMoveToNewestCache)
        {
            var path0 = generatePath(url, _cachePeriodNumber);
            if (File.Exists(path0))
            {
                return path0;
            }
            var path1 = generatePath(url, _cachePeriodNumber - 1);
            if (File.Exists(path1))
            {
                if (isMoveToNewestCache)
                {
                    File.Move(path1, path0);
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
    }
}