using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Kogase.Core
{
    public class FileStream : IFileStream
    {
        internal Action OnPop;
        readonly List<Action> ioQueue = new();
        IDebugger logger;

        public void SetLogger(IDebugger logger)
        {
            this.logger = logger;
        }

        public bool IsFileExist(string path)
        {
            var retval = File.Exists(path);
            return retval;
        }

        public bool IsDirectoryExist(string path)
        {
            var retval = Directory.Exists(path);
            return retval;
        }

        public void WriteFile
        (
            IFormatter formatter,
            string content,
            string path,
            Action<bool> onDone,
            bool instantWrite = false
        )
        {
            Action writeAction = () =>
            {
                try
                {
                    var pathDir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(pathDir)) Directory.CreateDirectory(pathDir!);

                    if (formatter != null)
                        using (System.IO.FileStream stream = new(path, FileMode.OpenOrCreate))
                        {
                            formatter.Serialize(stream, content);
                        }
                    else
                        using (var outputFile = new StreamWriter(path))
                        {
                            outputFile.Write(content);
                        }

                    onDone?.Invoke(true);
                }
                catch (Exception ex)
                {
                    logger?.LogVerbose($"Write file failure.\n{ex.Message}");
                    onDone?.Invoke(false);
                }
            };

            if (!instantWrite)
                lock (ioQueue)
                {
                    ioQueue.Add(writeAction);
                }
            else
                writeAction();
        }

        public async void WriteFileAsync(string content, string path, Action<bool> onDone)
        {
            try
            {
                var pathDirectory = Path.GetDirectoryName(path);
                if (!Directory.Exists(pathDirectory)) Directory.CreateDirectory(pathDirectory);

                using (var outputFile = new StreamWriter(path))
                {
                    await outputFile.WriteAsync(content);
                    onDone?.Invoke(true);
                    return;
                }
            }
            catch (Exception ex)
            {
                logger?.LogVerbose($"Write file failure.\n{ex.Message}");
                onDone?.Invoke(false);
            }
        }

        public void ReadFile(IFormatter formatter, string path, Action<bool, string> onDone, bool instantRead = false)
        {
            Action readAction = () =>
            {
                if (!File.Exists(path))
                {
                    onDone?.Invoke(false, null);
                    return;
                }

                try
                {
                    string result = null;

                    if (formatter != null)
                        using (System.IO.FileStream stream = new(path, FileMode.Open))
                        {
                            result = (string)formatter.Deserialize(stream);
                        }
                    else
                        result = File.ReadAllText(path);

                    onDone?.Invoke(true, result);
                }
                catch (Exception ex)
                {
                    logger?.LogVerbose($"Read file failure.\n{ex.Message}");
                    onDone?.Invoke(false, null);
                }
            };

            if (!instantRead)
                lock (ioQueue)
                {
                    ioQueue.Add(readAction);
                }
            else
                readAction();
        }

        public async void ReadFileAsync(string path, Action<bool, string> onDone)
        {
            if (!File.Exists(path))
            {
                onDone?.Invoke(false, null);
                return;
            }

            try
            {
                string result = null;
                using (var reader = File.OpenText(path))
                {
                    result = await reader.ReadToEndAsync();
                }

                onDone?.Invoke(true, result);
            }
            catch (Exception ex)
            {
                logger?.LogVerbose($"Read file failure.\n{ex.Message}");
                onDone?.Invoke(false, null);
            }
        }

        public void DeleteFile(string path, Action<bool> onDone, bool instantDelete = false)
        {
            Action deleteAction = () =>
            {
                if (!File.Exists(path))
                {
                    onDone?.Invoke(false);
                    return;
                }

                try
                {
                    File.Delete(path);
                    onDone?.Invoke(true);
                }
                catch (Exception ex)
                {
                    logger?.LogVerbose($"Delete file failure.\n{ex.Message}");
                    onDone?.Invoke(false);
                }
            };

            if (!instantDelete)
                lock (ioQueue)
                {
                    ioQueue.Add(deleteAction);
                }
            else
                deleteAction();
        }

        public void DeleteDirectory(string directory, Action<bool> onDone)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
                onDone?.Invoke(true);
                return;
            }

            onDone?.Invoke(false);
        }

        void IFileStream.Dispose()
        {
        }

        void IFileStream.Pop()
        {
            lock (ioQueue)
            {
                if (ioQueue.Count == 0) return;

                var ioActions = new List<Action>(ioQueue);
                ioQueue.Clear();

                foreach (var action in ioActions) action.Invoke();
                OnPop?.Invoke();
            }
        }

        void IFileStream.AddOnPop(Action action)
        {
            OnPop += action;
        }

        void IFileStream.RemoveOnPop(Action action)
        {
            OnPop -= action;
        }
    }
}