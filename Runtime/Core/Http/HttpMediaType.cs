namespace Kogase.Core
{
    /// <summary>
    /// Contains common MIME media type constants for use with HTTP requests
    /// </summary>
    internal sealed class HttpMediaType
    {
        readonly string name;

        public static readonly HttpMediaType ApplicationForm = new("application/x-www-form-urlencoded; charset=utf-8");
        public static readonly HttpMediaType ApplicationJson = new("application/json; charset=utf-8");
        public static readonly HttpMediaType TextPlain = new("text/plain; charset=utf-8");
        public static readonly HttpMediaType ApplicationOctetStream = new("application/octet-stream; charset=utf-8");

        HttpMediaType(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }
    }
}