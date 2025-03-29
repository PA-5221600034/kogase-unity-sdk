using System.Net;
using Kogase.Dtos;
using Kogase.Utils;

namespace Kogase.Core
{
    public static class HttpParser
    {
        static readonly string NoResponseMessage = "There is no response.";

        internal static Error ParseError(IHttpResponse response)
        {
            if (response == null) 
                return new Error(ErrorCode.NETWORK_ERROR, NoResponseMessage);
            
            if (response.Code is >= 200 and < 300) 
                return null;
            
            if (response.BodyBytes == null) 
                return new Error((ErrorCode)response.Code);

            ErrorResponse error = response.BodyBytes.ToObject<ErrorResponse>();
            
            if (error == null)
                return new Error((ErrorCode)response.Code);

            return new Error((ErrorCode)response.Code, error.Message);
        }

        internal static bool IsHasServerError(IHttpResponse response)
        {
            if (response == null)
                return false;

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

        internal static bool IsInternalErrorRetriable(IHttpResponse response)
        {
            Error error = ParseError(response);
            
            if (error == null) 
                return false;

            switch (error.Code)
            {
                case ErrorCode.HTTP_TOO_MANY_REQUESTS:
                case ErrorCode.RETRY_WITH:
                    return true;
            }

            return false;
        }
    }
}