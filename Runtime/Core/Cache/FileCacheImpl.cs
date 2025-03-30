using System;
using UnityEngine;
#if !UNITY_WEBGL
#endif

namespace Kogase.Core
{
    internal sealed class FileCacheImpl : ICacheImpl<string>
    {
        const int ReadWriteAsyncWaitMs = 100;

        readonly string cacheDirectory;

        readonly IFileStream fs;

        public FileCacheImpl(string cacheDirectory, IFileStream fs = null)
        {
            fs ??= KogaseSDK.Implementation.FileStream;

            if (string.IsNullOrEmpty(cacheDirectory))
                throw new InvalidOperationException("Cache directory is empty.");

            this.cacheDirectory = cacheDirectory;
            this.fs = fs;
        }

        public bool Contains(string key)
        {
            var itemPath = GetFileFullPath(key);
            var retval = fs.IsFileExist(itemPath);
            return retval;
        }

        public bool Emplace(string key, string item)
        {
            try
            {
                var itemSavePath = GetFileFullPath(key);
                fs.WriteFile(null, item, itemSavePath, null, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to emplace cache file: {item}.\n{e.Message}");
                return false;
            }

            return true;
        }

        public async void EmplaceAsync(string key, string item, Action<bool> callback = null)
        {
            try
            {
                var filePath = GetFileFullPath(key);
                var writeSuccess = false;
                while (!writeSuccess)
                    try
                    {
                        fs.WriteFileAsync(item, filePath, success => { writeSuccess = success; });
                    }
                    catch (Exception)
                    {
                        await System.Threading.Tasks.Task.Delay(ReadWriteAsyncWaitMs);
                    }

                callback?.Invoke(true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to emplace cache file: {item}.\n{e.Message}");
            }
        }

        public bool Update(string key, string item)
        {
            return Contains(key) && Emplace(key, item);
        }

        public void Empty()
        {
            try
            {
                fs.DeleteDirectory(cacheDirectory, null);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to delete cache directory: {cacheDirectory}.\n{e.Message}");
            }
        }

        public string Retrieve(string key)
        {
            var filePath = GetFileFullPath(key);
            string retval = null;

            try
            {
                fs.ReadFile(null, filePath, instantRead: true, onDone: (isSucess, readResult) =>
                {
                    if (isSucess) retval = (string)readResult;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to read cache file: {filePath}.\n{e.Message}");
            }

            return retval;
        }

        public void RetrieveAsync(string key, Action<string> callback)
        {
            if (callback != null)
            {
                var filePath = GetFileFullPath(key);
                fs.ReadFileAsync(filePath, (success, readResult) => { callback?.Invoke(readResult); });
            }
        }

        public string Peek(string key)
        {
            return Retrieve(key);
        }

        public bool Remove(string key)
        {
            var result = false;

            try
            {
                var filePath = GetFileFullPath(key);
                fs.DeleteFile(
                    filePath,
                    instantDelete: true,
                    onDone: isSuccess => { result = isSuccess; }
                );
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to delete cache file: {key}.\n{e.Message}");
            }

            return result;
        }

        string GetFileFullPath(string key)
        {
            return $"{cacheDirectory}/{key}";
        }
    }
}