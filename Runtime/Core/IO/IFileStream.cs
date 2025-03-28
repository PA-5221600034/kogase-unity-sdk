using System.Runtime.Serialization;
namespace Kogase.Core
{
    public interface IFileStream
    {
        bool IsFileExist(string path);
        bool IsDirectoryExist(string path);
        void WriteFile(IFormatter formatter, string content, string path, System.Action<bool> onDone, bool instantWrite = false);
        void ReadFile(IFormatter formatter, string path, System.Action<bool, string> onDone, bool instantRead = false);
        void DeleteFile(string path, System.Action<bool> onDone, bool instantDelete = false);
        void DeleteDirectory(string directory, System.Action<bool> onDone);
        void WriteFileAsync(string content, string path, System.Action<bool> onDone);
        void ReadFileAsync(string path, System.Action<bool, string> onDone);
        internal void Dispose();
        internal void AddOnPop(System.Action action);
        internal void RemoveOnPop(System.Action action);
        internal void Pop();
    }
}
