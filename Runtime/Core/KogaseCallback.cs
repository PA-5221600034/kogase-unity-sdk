namespace Kogase.Core
{
    public delegate void SuccessDelegate<TSuccess>(TSuccess success);
    public delegate void ErrorDelegate<TError>(TError error);
}