namespace Kogase.Core
{
    internal class FileStreamFactory : IFileStreamFactory
    {
        public IFileStream CreateFileStream()
        {
#if UNITY_SWITCH && !UNITY_EDITOR
            var retval = new NullFileStream();
#elif UNITY_WEBGL && !UNITY_EDITOR
            var retval = new PlayerPrefsFileStream();
#else
            var retval = new FileStream();
#endif
            return retval;
        }
    }
}