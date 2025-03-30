using System;
using System.Text;
using Kogase.Utils;

namespace Kogase.Core
{
    internal static class HttpParser
    {
        /// <summary>
        /// Parses an HTTP response for errors
        /// </summary>
        /// <param name="response">The HTTP response to check</param>
        /// <returns>An Error object if an error occurred, null otherwise</returns>
        public static Error ParseError(IHttpResponse response)
        {
            if (response == null) return new Error(Code.NETWORK_ERROR, "No response received");

            if (response.Code < 200 || response.Code >= 300)
            {
                var errorCode = GetErrorCodeFromStatusCode(response.Code);
                string errorMessage = null;

                try
                {
                    var responseText = Encoding.UTF8.GetString(response.BodyBytes);
                    if (!string.IsNullOrEmpty(responseText)) errorMessage = responseText;
                }
                catch (Exception e)
                {
                    errorMessage = $"Error parsing response: {e.Message}";
                }

                return new Error(errorCode, errorMessage);
            }

            return null;
        }

        /// <summary>
        /// Parse the response and invoke the appropriate callback
        /// </summary>
        /// <typeparam name="T">Type to deserialize response to</typeparam>
        /// <param name="response">HTTP response</param>
        /// <param name="okCallback">Success callback</param>
        /// <param name="errorCallback">Error callback</param>
        public static void ParseResponse<T>(IHttpResponse response, OkDelegate<T> okCallback,
            ErrorDelegate<Error> errorCallback)
        {
            var error = ParseError(response);
            if (error != null)
            {
                errorCallback?.Invoke(error);
                return;
            }

            try
            {
                var result = response.BodyBytes.ToObject<T>();
                okCallback?.Invoke(result);
            }
            catch (Exception e)
            {
                error = new Error(Code.ERROR_FROM_EXCEPTION, e.Message);
                errorCallback?.Invoke(error);
            }
        }

        /// <summary>
        /// Maps HTTP status codes to error codes
        /// </summary>
        static Code GetErrorCodeFromStatusCode(long statusCode)
        {
            return statusCode switch
            {
                400 => Code.BAD_REQUEST,
                401 => Code.UNAUTHORIZED,
                402 => Code.PAYMENT_REQUIRED,
                403 => Code.FORBIDDEN,
                404 => Code.NOT_FOUND,
                405 => Code.METHOD_NOT_ALLOWED,
                406 => Code.NOT_ACCEPTABLE,
                407 => Code.PROXY_AUTHENTICATION_REQUIRED,
                408 => Code.REQUEST_TIMEOUT,
                409 => Code.CONFLICT,
                410 => Code.GONE,
                411 => Code.LENGTH_REQUIRED,
                412 => Code.PRECONDITION_FAILED,
                413 => Code.REQUEST_ENTITY_TOO_LARGE,
                414 => Code.REQUEST_URI_TOO_LONG,
                415 => Code.UNSUPPORTED_MEDIA_TYPE,
                416 => Code.REQUESTED_RANGE_NOT_SATISFIABLE,
                417 => Code.EXPECTATION_FAILED,
                422 => Code.UNPROCESSABLE_ENTITY,
                429 => Code.HTTP_TOO_MANY_REQUESTS,
                500 => Code.INTERNAL_SERVER_ERROR,
                501 => Code.NOT_IMPLEMENTED,
                502 => Code.BAD_GATEWAY,
                503 => Code.SERVICE_UNAVAILABLE,
                504 => Code.GATEWAY_TIMEOUT,
                505 => Code.HTTP_VERSION_NOT_SUPPORTED,
                _ => statusCode >= 500 ? Code.INTERNAL_SERVER_ERROR : Code.BAD_REQUEST
            };
        }
    }
}