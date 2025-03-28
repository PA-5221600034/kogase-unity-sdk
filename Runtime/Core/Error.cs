namespace Kogase.Core
{
    public class Error
    {
        public readonly ErrorCode Code;
        public readonly string Message;
        public readonly object MessageVariables;
        public readonly Error InnerError;

        public Error(ErrorCode code, string message = null, object messageVariables = null, Error innerError = null)
        {
            Code = code;
            Message = string.IsNullOrEmpty(message) ? GetDefaultErrorMessage() : message;
            InnerError = innerError;
            MessageVariables = messageVariables;
        }

        public Error WrapWith(ErrorCode code, string message = null, object messageVariables = null)
        {
            return new Error(code, message, messageVariables, this);
        }

        string GetDefaultErrorMessage()
        {
            switch (Code)
            {
                case ErrorCode.NONE:
                    return "This error code doesn't make sense and should not happen at all.";

                // HTTP Status Codes
                case ErrorCode.BAD_REQUEST:
                    return "The request could not be understood by the server due to malformed syntax.";
                case ErrorCode.UNAUTHORIZED:
                    return "The request requires user authentication.";
                case ErrorCode.PAYMENT_REQUIRED:
                    return "The request requires a payment.";
                case ErrorCode.FORBIDDEN:
                    return "The server understood the request, but is refusing to fulfill it.";
                case ErrorCode.NOT_FOUND:
                    return "The server has not found anything matching the Request-URI.";
                case ErrorCode.METHOD_NOT_ALLOWED:
                    return
                        "The method specified in the Request-Line is not allowed for the resource identified by the " +
                        "Request-URI.";
                case ErrorCode.NOT_ACCEPTABLE:
                    return "The resource identified by the request can not generate content according to the accept " +
                           "headers sent in the request.";
                case ErrorCode.PROXY_AUTHENTICATION_REQUIRED:
                    return "The request requires user authentication via proxy.";
                case ErrorCode.REQUEST_TIMEOUT:
                    return "The client did not produce a request within the time that the server was prepared to wait.";
                case ErrorCode.CONFLICT:
                    return
                        "The request could not be completed due to a conflict with the current state of the resource.";
                case ErrorCode.GONE:
                    return "The requested resource is no longer available at the server and no forwarding address is " +
                           "known.";
                case ErrorCode.LENGTH_REQUIRED:
                    return "The server refuses to accept the request without a defined Content-Length.";
                case ErrorCode.PRECONDITION_FAILED:
                    return "The precondition given in one or more of the request-header fields evaluated to false " +
                           "when it was tested on the server.";
                case ErrorCode.REQUEST_ENTITY_TOO_LARGE:
                    return "The request entity is larger than the server is willing or able to process.";
                case ErrorCode.REQUEST_URI_TOO_LONG:
                    return "The Request-URI is longer than the server is willing to interpret.";
                case ErrorCode.UNSUPPORTED_MEDIA_TYPE:
                    return "The entity of the request is in a format not supported by the requested resource for " +
                           "the requested method.";
                case ErrorCode.REQUESTED_RANGE_NOT_SATISFIABLE:
                    return "The request included a Range request-header field but none of the range-specifier " +
                           "values in this field overlap the current extent of the selected resource, and the " +
                           "request did not include an If-Range request-header field.";
                case ErrorCode.EXPECTATION_FAILED:
                    return "The expectation given in an Expect request-header field could not be met by this server.";
                case ErrorCode.UNPROCESSABLE_ENTITY:
                    return "Entity can not be processed.";
                case ErrorCode.INTERNAL_SERVER_ERROR:
                    return "Unexpected condition encountered which prevented the server from fulfilling the request.";
                case ErrorCode.NOT_IMPLEMENTED:
                    return "The server does not support the functionality required to fulfill the request.";
                case ErrorCode.BAD_GATEWAY:
                    return "The gateway or proxy received an invalid response from the upstream server.";
                case ErrorCode.SERVICE_UNAVAILABLE:
                    return "The server is currently unable to handle the request due to a temporary overloading or " +
                           "maintenance of the server.";
                case ErrorCode.GATEWAY_TIMEOUT:
                    return "The gateway or proxy, did not receive a timely response from the upstream server.";
                case ErrorCode.HTTP_VERSION_NOT_SUPPORTED:
                    return
                        "The server does not support the HTTP protocol version that was used in the request message.";
                default:
                    return "Unknown error: " + Code.ToString("G");
            }
        }
    }

    public enum ErrorCode
    {
        NONE = 0,
        CONTINUE = 100,
        SWITCHING_PROTOCOLS = 101,
        PROCESSING = 102,
        OK = 200,
        CREATED = 201,
        ACCEPTED = 202,
        NON_AUTHORITATIVE_INFO = 203,
        NO_CONTENT = 204,
        RESET_CONTENT = 205,
        PARTIAL_CONTENT = 206,
        MULTI_STATUS = 207,
        ALREADY_REPORTED = 208,
        IM_USED = 226,
        MULTIPLE_CHOICES = 300,
        MOVED_PERMANENTLY = 301,
        FOUND = 302,
        SEE_OTHER = 303,
        NOT_MODIFIED = 304,
        USE_PROXY = 305,
        TEMPORARY_REDIRECT = 307,
        PERMANENT_REDIRECT = 308,
        BAD_REQUEST = 400,
        UNAUTHORIZED = 401,
        PAYMENT_REQUIRED = 402,
        FORBIDDEN = 403,
        NOT_FOUND = 404,
        METHOD_NOT_ALLOWED = 405,
        NOT_ACCEPTABLE = 406,
        PROXY_AUTHENTICATION_REQUIRED = 407,
        REQUEST_TIMEOUT = 408,
        CONFLICT = 409,
        GONE = 410,
        LENGTH_REQUIRED = 411,
        PRECONDITION_FAILED = 412,
        REQUEST_ENTITY_TOO_LARGE = 413,
        REQUEST_URI_TOO_LONG = 414,
        UNSUPPORTED_MEDIA_TYPE = 415,
        REQUESTED_RANGE_NOT_SATISFIABLE = 416,
        EXPECTATION_FAILED = 417,
        STATUS_TEAPOT = 418,
        STATUS_MISDIRECTED_REQUEST = 421,
        UNPROCESSABLE_ENTITY = 422,
        STATUS_LOCKED = 423,
        STATUS_FAILED_DEPENDENCY = 424,
        STATUS_UPGRADE_REQUIRED = 426,
        STATUS_PRECONDITION_REQUIRED = 428,
        HTTP_TOO_MANY_REQUESTS = 429,
        STATUS_REQUEST_HEADER_FIELDS_TOO_LARGE = 431,
        RETRY_WITH = 449,
        STATUS_UNAVAILABLE_FOR_LEGAL_REASONS = 451,
        INTERNAL_SERVER_ERROR = 500,
        NOT_IMPLEMENTED = 501,
        BAD_GATEWAY = 502,
        SERVICE_UNAVAILABLE = 503,
        GATEWAY_TIMEOUT = 504,
        HTTP_VERSION_NOT_SUPPORTED = 505,
        STATUS_VARIANT_ALSO_NEGOTIATES = 506,
        STATUS_INSUFFICIENT_STORAGE = 507,
        STATUS_LOOP_DETECTED = 508,
        STATUS_NOT_EXTENDED = 510,
        STATUS_NETWORK_AUTHENTICATION_REQUIRED = 511,
        GENERAL_CLIENT_ERROR = 14000,
        ERROR_FROM_EXCEPTION = 14001,
        INVALID_ARGUMENT = 14002,
        INVALID_REQUEST = 14003,
        INVALID_RESPONSE = 14004,
        NETWORK_ERROR = 14005,
        IS_NOT_LOGGED_IN = 14006,
        USER_PROFILE_IS_NOT_CREATED = 14007,
        GENERATE_TOKEN_FAILED = 14008,
        ACCOUNT_IS_NOT_UPGRADED = 14009,
        VERIFICATION_CODE_IS_NOT_REQUESTED = 14010,
        EMAIL_IS_NOT_VERIFIED = 14011,
        ENTITLEMENT_NOT_CREATED = 14012,
        GENERATE_AUTH_CODE_FAILED = 14013,
        ACCESS_DENIED = 14014,
        MESSAGE_FIELD_TYPE_NOT_SUPPORTED = 14015,
        MESSAGE_FORMAT_INVALID = 14016,
        MESSAGE_FIELD_DOES_NOT_EXIST = 14017,
        MESSAGE_FIELD_CONVERSION_FAILED = 14018,
        MESSAGE_CANNOT_BE_SENT = 14019,
        MESSAGE_TYPE_NOT_SUPPORTED = 14020,
        WEB_SOCKET_CONNECT_FAILED = 14201,
        CACHED_TOKEN_NOT_FOUND = 14301,
        UNABLE_TO_SERIALIZE_DESERIALIZE_CACHED_TOKEN = 14302,
        CACHED_TOKEN_EXPIRED = 14303,
        DEPRECATED = 14901
    }
}