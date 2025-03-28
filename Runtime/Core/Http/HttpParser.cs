using System.Net;

namespace Kogase.Core
{
    public static class HttpParser
    {
        static readonly string NoResponseMessage = "There is no response.";

        internal static Error ParseError(IHttpResponse response)
        {
            if (response == null) return new Error(ErrorCode.NETWORK_ERROR, NoResponseMessage);

            if (response.Code is >= 200 and < 300) return null;

            if (response.Code is < 400 or >= 600) return ParseDefaultError(response);

            if (response.BodyBytes == null) return new Error((ErrorCode)response.Code);

            return new Error((ErrorCode)response.Code);
        }

        internal static bool IsHasServerError(IHttpResponse response)
        {
            if (response == null)
            {
                return false;
            }
            else
            {
                switch ((HttpStatusCode)response.Code)
                {
                    case HttpStatusCode.InternalServerError:
                    case HttpStatusCode.BadGateway:
                    case HttpStatusCode.ServiceUnavailable:
                    case HttpStatusCode.GatewayTimeout:
                        return true;
                }

                return false;
            }
        }

        internal static bool IsInternalErrorRetriable(IHttpResponse response)
        {
            var error = ParseError(response);
            if (error == null) return false;

            switch (error.Code)
            {
                case ErrorCode.HTTP_TOO_MANY_REQUESTS:
                case ErrorCode.RETRY_WITH:
                    return true;
            }

            return false;
        }

        static Error ParseDefaultError(IHttpResponse response)
        {
            Error retval;
            if (response.BodyBytes == null)
            {
                retval = new Error((ErrorCode)response.Code);
            }
            else
            {
                var body = System.Text.Encoding.UTF8.GetString(response.BodyBytes);
                retval = new Error((ErrorCode)response.Code, "Unknown error: " + body);
            }

            return retval;
        }
    }
}