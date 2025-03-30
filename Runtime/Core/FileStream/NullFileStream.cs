using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kogase.Core
{
    public class NullFileStream : IFileStream
    {
        readonly Dictionary<string, string> saveDict = new();

        public bool IsFileExist(string path)
        {
            var keyExist = saveDict.ContainsKey(path);
            return keyExist;
        }

        public void DeleteFile(
            string path,
            Action<bool> onDone,
            bool instantDelete = false
        )
        {
            if (!IsFileExist(path))
            {
                onDone?.Invoke(false);
                return;
            }

            var removeSuccess = saveDict.Remove(path);
            onDone?.Invoke(removeSuccess);
        }

        public bool IsDirectoryExist(string path)
        {
            foreach (var key in saveDict.Keys)
                if (key.StartsWith(path))
                    return true;

            return false;
        }

        public void DeleteDirectory(string directory, Action<bool> onDone)
        {
            if (saveDict.Count == 0)
            {
                onDone?.Invoke(false);
                return;
            }

            var isDirectoryFound = false;
            List<string> keys = new();
            foreach (var key in saveDict.Keys) keys.Add(key);
            foreach (var key in keys)
                if (key.StartsWith(directory))
                {
                    isDirectoryFound = true;
                    saveDict.Remove(key);
                }

            onDone?.Invoke(isDirectoryFound);
        }

        public void WriteFile(
            IFormatter formatter,
            string content,
            string path,
            Action<bool> onDone,
            bool instantWrite = false
        )
        {
            saveDict[path] = content;
            onDone?.Invoke(true);
        }

        public void ReadFile(
            IFormatter formatter,
            string path,
            Action<bool, string> onDone,
            bool instantRead = false
        )
        {
            if (!IsFileExist(path))
            {
                onDone?.Invoke(false, null);
                return;
            }

            var output = saveDict[path];
            onDone?.Invoke(true, output);
        }

        public void WriteFileAsync(
            string content,
            string path,
            Action<bool> onDone
        )
        {
            saveDict[path] = content;
            onDone?.Invoke(true);
        }

        public void ReadFileAsync(string path, Action<bool, string> onDone)
        {
            if (!IsFileExist(path))
            {
                onDone?.Invoke(false, null);
                return;
            }

            var output = saveDict[path];
            onDone?.Invoke(true, output);
        }

        void IFileStream.Dispose()
        {
        }

        void IFileStream.AddOnPop(Action action)
        {
        }

        void IFileStream.RemoveOnPop(Action action)
        {
        }

        void IFileStream.Pop()
        {
        }
    }
}