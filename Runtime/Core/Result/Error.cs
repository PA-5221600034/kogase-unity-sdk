namespace Kogase.Core
{
    public class Error
    {
        public readonly Code Code;
        public readonly string Message;
        public readonly object MessageVariables;
        public readonly Error InnerError;

        public Error(Code code, string message = null, object messageVariables = null, Error innerError = null)
        {
            Code = code;
            Message = string.IsNullOrEmpty(message) ? GetDefaultErrorMessage() : message;
            InnerError = innerError;
            MessageVariables = messageVariables;
        }

        public Error WrapWith(Code code, string message = null, object messageVariables = null)
        {
            return new Error(code, message, messageVariables, this);
        }

        string GetDefaultErrorMessage()
        {
            switch (Code)
            {
                case Code.NONE:
                    return "This error code doesn't make sense and should not happen at all.";

                // HTTP Status Codes
                case Code.BAD_REQUEST:
                    return "The request could not be understood by the server due to malformed syntax.";
                case Code.UNAUTHORIZED:
                    return "The request requires user authentication.";
                case Code.PAYMENT_REQUIRED:
                    return "The request requires a payment.";
                case Code.FORBIDDEN:
                    return "The server understood the request, but is refusing to fulfill it.";
                case Code.NOT_FOUND:
                    return "The server has not found anything matching the Request-URI.";
                case Code.METHOD_NOT_ALLOWED:
                    return
                        "The method specified in the Request-Line is not allowed for the resource identified by the " +
                        "Request-URI.";
                case Code.NOT_ACCEPTABLE:
                    return "The resource identified by the request can not generate content according to the accept " +
                           "headers sent in the request.";
                case Code.PROXY_AUTHENTICATION_REQUIRED:
                    return "The request requires user authentication via proxy.";
                case Code.REQUEST_TIMEOUT:
                    return "The client did not produce a request within the time that the server was prepared to wait.";
                case Code.CONFLICT:
                    return
                        "The request could not be completed due to a conflict with the current state of the resource.";
                case Code.GONE:
                    return "The requested resource is no longer available at the server and no forwarding address is " +
                           "known.";
                case Code.LENGTH_REQUIRED:
                    return "The server refuses to accept the request without a defined Content-Length.";
                case Code.PRECONDITION_FAILED:
                    return "The precondition given in one or more of the request-header fields evaluated to false " +
                           "when it was tested on the server.";
                case Code.REQUEST_ENTITY_TOO_LARGE:
                    return "The request entity is larger than the server is willing or able to process.";
                case Code.REQUEST_URI_TOO_LONG:
                    return "The Request-URI is longer than the server is willing to interpret.";
                case Code.UNSUPPORTED_MEDIA_TYPE:
                    return "The entity of the request is in a format not supported by the requested resource for " +
                           "the requested method.";
                case Code.REQUESTED_RANGE_NOT_SATISFIABLE:
                    return "The request included a Range request-header field but none of the range-specifier " +
                           "values in this field overlap the current extent of the selected resource, and the " +
                           "request did not include an If-Range request-header field.";
                case Code.EXPECTATION_FAILED:
                    return "The expectation given in an Expect request-header field could not be met by this server.";
                case Code.UNPROCESSABLE_ENTITY:
                    return "Entity can not be processed.";
                case Code.INTERNAL_SERVER_ERROR:
                    return "Unexpected condition encountered which prevented the server from fulfilling the request.";
                case Code.NOT_IMPLEMENTED:
                    return "The server does not support the functionality required to fulfill the request.";
                case Code.BAD_GATEWAY:
                    return "The gateway or proxy received an invalid response from the upstream server.";
                case Code.SERVICE_UNAVAILABLE:
                    return "The server is currently unable to handle the request due to a temporary overloading or " +
                           "maintenance of the server.";
                case Code.GATEWAY_TIMEOUT:
                    return "The gateway or proxy, did not receive a timely response from the upstream server.";
                case Code.HTTP_VERSION_NOT_SUPPORTED:
                    return
                        "The server does not support the HTTP protocol version that was used in the request message.";
                default:
                    return "Unknown error: " + Code.ToString("G");
            }
        }
    }
}