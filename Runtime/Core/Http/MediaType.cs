namespace Kogase.Core
{
    /// <summary>
    /// Contains common MIME media type constants for use with HTTP requests
    /// </summary>
    public sealed class MediaType
    {
        readonly string name;

        public static readonly MediaType ApplicationForm = new("application/x-www-form-urlencoded; charset=utf-8");

        public static readonly MediaType ApplicationJson = new("application/json; charset=utf-8");
        public static readonly MediaType TextPlain = new("text/plain; charset=utf-8");
        public static readonly MediaType ApplicationOctetStream = new("application/octet-stream; charset=utf-8");

        MediaType(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }
    }
}