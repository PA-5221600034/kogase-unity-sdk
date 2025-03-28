#if !UNITY_WEBGL
using System.Text;
#endif

namespace Kogase.Core
{
    internal class FileCacheImpl : ICacheImpl<string>
    {
        const int ReadWriteAsyncWaitMs = 100;
        
        readonly string cacheDirectory;

        readonly IFileStream fs;
        readonly IDebugger logger;

        public FileCacheImpl(string cacheDirectory, IFileStream fs = null, IDebugger logger = null)
        {
            if (fs == null) fs = KogaseSDK.Implementation.FileStream;

            if (string.IsNullOrEmpty(cacheDirectory))
                throw new System.InvalidOperationException("Cache directory is empty.");
            this.cacheDirectory = cacheDirectory;
            this.fs = fs;
            this.logger = logger;
        }

        public bool Contains(string key)
        {
            var itemPath = GetFileFullPath(key);
            var retval = fs.IsFileExist(itemPath);
            return retval;
        }

        public virtual bool Emplace(string key, string item)
        {
            try
            {
                var itemSavePath = GetFileFullPath(key);
                fs.WriteFile(null, item, itemSavePath, null, true);
            }
            catch (System.Exception ex)
            {
                logger?.LogWarning(ex.Message);
                return false;
            }

            return true;
        }

        public virtual async void EmplaceAsync(string key, string item, System.Action<bool> callback = null)
        {
            var filePath = GetFileFullPath(key);
            var writeSuccess = false;
            while (!writeSuccess)
                try
                {
                    fs.WriteFileAsync(item, filePath, (success) => { writeSuccess = success; });
                }
                catch (System.Exception)
                {
                    await System.Threading.Tasks.Task.Delay(ReadWriteAsyncWaitMs);
                }

            callback?.Invoke(true);
        }

        public bool Update(string key, string item)
        {
            if (!Contains(key)) return false;
            return Emplace(key, item);
        }

        public void Empty()
        {
            try
            {
                fs.DeleteDirectory(cacheDirectory, null);
            }
            catch (System.Exception exception)
            {
                logger?.LogWarning($"Failed to delete cache directory: {cacheDirectory}.\n{exception.Message}");
            }
        }

        public virtual string Retrieve(string key)
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
            catch (System.Exception ex)
            {
                logger?.LogWarning(ex.Message);
            }

            return retval;
        }

        public virtual void RetrieveAsync(string key, System.Action<string> callback)
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
                fs.DeleteFile(filePath, instantDelete: true, onDone: (isSuccess) => { result = isSuccess; });
            }
            catch (System.Exception ex)
            {
                logger?.LogWarning($"Failed to delete cache file.\n{ex.Message}");
            }

            return result;
        }

        string GetFileFullPath(string key)
        {
            var retval = $"{cacheDirectory}/{key}";
            return retval;
        }
    }
}