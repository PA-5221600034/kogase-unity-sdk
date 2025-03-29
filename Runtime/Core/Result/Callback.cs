namespace Kogase.Core
{
    public delegate void OkDelegate<T>(T success);
    public delegate void ErrorDelegate<T>(T error);
}